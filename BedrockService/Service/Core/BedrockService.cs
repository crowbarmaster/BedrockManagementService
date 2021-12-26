using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Server;
using NCrontab;
using System.Timers;

namespace BedrockService.Service.Core {
    public enum ServiceStatus {
        Stopped,
        Starting,
        Started,
        Stopping
    }

    public class BedrockService : ServiceControl, IBedrockService {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        private readonly IUpdater _updater;
        private readonly ITCPListener _tCPListener;
        private CrontabSchedule? _backupCron { get; set; }
        private CrontabSchedule? _updaterCron { get; set; }
        private HostControl? _hostControl { get; set; }
        private List<IBedrockServer> _bedrockServers { get; set; } = new();
        private System.Timers.Timer? _updaterTimer { get; set; }
        private System.Timers.Timer? _backupTimer { get; set; }
        private ServiceStatus _CurrentServiceStatus { get; set; }

        public BedrockService(IConfigurator configurator, IUpdater updater, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, ITCPListener tCPListener) {
                _tCPListener = tCPListener;
                _configurator = configurator;
                _logger = logger;
                _serviceConfiguration = serviceConfiguration;
                _processInfo = serviceProcessInfo;
                _updater = updater;
                _logger = logger;
        }

        public Task<bool> Initialize() {
            return Task.Run(() => {
                _CurrentServiceStatus = ServiceStatus.Starting;
                _tCPListener.BlockClientConnections();
                _updater.Initialize();
                _configurator.LoadAllConfigurations().Wait();
                _backupCron = CrontabSchedule.TryParse(_serviceConfiguration.GetProp("BackupCron").ToString());
                _updaterCron = CrontabSchedule.TryParse(_serviceConfiguration.GetProp("UpdateCron").ToString());
                _logger.Initialize();
                if (!bool.Parse(_serviceConfiguration.GetProp("AcceptedMojangLic").ToString())) {
                    _logger.AppendLine("You have not accepted the license. Please visit the readme for more info!");
                    return false;
                }
                _updater.CheckUpdates().Wait();
                _bedrockServers.Clear();
                InitializeTimers();
                try {
                    List<IServerConfiguration> temp = _serviceConfiguration.GetServerList();
                    foreach (IServerConfiguration server in temp) {
                        IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo);
                        bedrockServer.Initialize();
                        _bedrockServers.Add(bedrockServer);
                    }
                }
                catch (Exception e) {
                    _logger.AppendLine($"Error Instantiating BedrockServiceWrapper: {e.StackTrace}");
                }
                _tCPListener.Initialize();
                return true;
            });
        }

        public bool Start(HostControl hostControl) {
            if (!Initialize().Result) {
                _logger.AppendLine("BedrockService did not initialize correctly.");
                Task.Delay(3000).Wait();
                Environment.Exit(1);
            }
            _hostControl = hostControl;
            try {
                ValidSettingsCheck();
                foreach (var brs in _bedrockServers) {
                    if (hostControl != null)
                        try {
                            _hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(30));
                        }
                        catch (Exception ex) {
                            _logger.AppendLine("Error!");
                        }
                    brs.SetServerStatus(BedrockServer.ServerStatus.Starting);
                    brs.StartWatchdog(_hostControl);
                }
                _tCPListener.UnblockClientConnections();
                _CurrentServiceStatus = ServiceStatus.Started;
                return true;
            }
            catch (Exception e) {
                _logger.AppendLine($"Error: {e.Message}.\n{e.StackTrace}");
                return false;
            }
        }

        public bool Stop(HostControl hostControl) {
            _CurrentServiceStatus &= ServiceStatus.Stopping;
            _hostControl = hostControl;
            try {
                foreach (var brs in _bedrockServers) {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                    while (brs.GetServerStatus() == BedrockServer.ServerStatus.Stopping && !Program.IsExiting)
                        Thread.Sleep(100);
                }
                _CurrentServiceStatus = ServiceStatus.Stopped;
                return true;
            }
            catch (Exception e) {
                _logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public Task RestartService() { 
            return Task.Run(() => {
                try {
                    _tCPListener.BlockClientConnections();
                    foreach (IBedrockServer brs in _bedrockServers) {
                        brs.StopServer(true).Wait();
                    }
                    Start(_hostControl);
                } catch (Exception e) {
                    _logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.Message} StackTrace: {e.StackTrace}");
                }
            });
        }

        public ServiceStatus GetServiceStatus() => _CurrentServiceStatus;

        public IBedrockServer GetBedrockServerByIndex(int serverIndex) {
            return _bedrockServers[serverIndex];
        }

        public IBedrockServer? GetBedrockServerByName(string name) {
            return _bedrockServers.FirstOrDefault(brs => brs.GetServerName() == name);
        }

        public List<IBedrockServer> GetAllServers() => _bedrockServers;

        public void InitializeNewServer(IServerConfiguration server) {
            IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo);
            bedrockServer.Initialize();
            _bedrockServers.Add(bedrockServer);
            _serviceConfiguration.AddNewServerInfo(server);
            if (ValidSettingsCheck()) {
                bedrockServer.SetServerStatus(BedrockServer.ServerStatus.Starting);
                bedrockServer.StartWatchdog(_hostControl);
            }
        }

        private void InitializeTimers() {
            if (_backupTimer != null) {
                _backupTimer.Stop();
                _backupTimer = null;
            }
            if (_updaterTimer != null) {
                _updaterTimer.Stop();
                _updaterTimer = null;
            }
            if (_serviceConfiguration.GetProp("BackupEnabled").ToString().ToLower() == "true" && _backupCron != null) {
                double interval = (_backupCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds;
                if (interval >= 0) {
                    _backupTimer = new System.Timers.Timer(interval);
                    _logger.AppendLine($"Automatic backups Enabled, next backup in: {((float)_backupTimer.Interval / 1000)} seconds.");
                    _backupTimer.Elapsed += BackupTimer_Elapsed;
                    _backupTimer.Start();
                }
            }
            if (_serviceConfiguration.GetProp("CheckUpdates").ToString().ToLower() == "true" && _updaterCron != null) {
                double interval = (_updaterCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds;
                if (interval >= 0) {
                    _updaterTimer = new System.Timers.Timer(interval);
                    _updaterTimer.Elapsed += UpdateTimer_Elapsed;
                    _logger.AppendLine($"Automatic updates Enabled, will be checked in: {((float)_updaterTimer.Interval / 1000)} seconds.");
                    _updaterTimer.Start();
                }
            }
        }

        private void BackupTimer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                BackupAllServers();
                InitializeTimers();
            }
            catch (Exception ex) {
                _logger.AppendLine($"Error in BackupTimer_Elapsed {ex}");
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                _updater.CheckUpdates().Wait();
                if (_updater.CheckVersionChanged()) {
                    _logger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                    foreach (IBedrockServer server in _bedrockServers) {
                        server.RestartServer(false);
                    }
                }
                InitializeTimers();
            }
            catch (Exception ex) {
                _logger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }

        private void BackupAllServers() {
            _logger.AppendLine("Service started backup of all servers...");
            foreach (var brs in _bedrockServers) {
                brs.InitializeBackup();
            }
            _logger.AppendLine("Backups have been completed.");
        }

        private bool ValidSettingsCheck() {
            bool validating = true;
            bool dupedSettingsFound = false;
            while (validating) {
                if (_serviceConfiguration.GetServerList().Count() < 1) {
                    throw new Exception("No Servers Configured");
                }
                else {
                    foreach (IServerConfiguration server in _serviceConfiguration.GetServerList()) {
                        foreach (IServerConfiguration compareServer in _serviceConfiguration.GetServerList()) {
                            if (server != compareServer) {
                                if (server.GetProp("server-port").Equals(compareServer.GetProp("server-port")) ||
                                    server.GetProp("server-portv6").Equals(compareServer.GetProp("server-portv6")) ||
                                    server.GetProp("server-name").Equals(compareServer.GetProp("server-name"))) {
                                    _logger.AppendLine($"Duplicate server settings between servers {server.GetFileName()} and {compareServer.GetFileName()}.");
                                    dupedSettingsFound = true;
                                }
                            }
                        }
                    }
                    if (dupedSettingsFound) {
                        throw new Exception("Duplicate settings found! Check logs.");
                    }
                    foreach (var server in _serviceConfiguration.GetServerList()) {
                        if (_updater.CheckVersionChanged() || !File.Exists(server.GetProp("ServerPath") + "\\bedrock_server.exe")) {
                            _configurator.ReplaceServerBuild(server).Wait();
                        }
                        if (server.GetProp("ServerExeName").ToString() != "bedrock_server.exe" && File.Exists(server.GetProp("ServerPath") + "\\bedrock_server.exe") && !File.Exists(server.GetProp("ServerPath") + "\\" + server.GetProp("ServerExeName"))) {
                            File.Copy(server.GetProp("ServerPath") + "\\bedrock_server.exe", server.GetProp("ServerPath") + "\\" + server.GetProp("ServerExeName"));
                        }
                    }
                    if (_updater.CheckVersionChanged())
                        _updater.MarkUpToDate();
                    else {
                        validating = false;
                    }
                }
            }
            return true;
        }

        public void RemoveBedrockServerByIndex(int serverIndex) {
            _bedrockServers.RemoveAt(serverIndex);
        }
    }
}
