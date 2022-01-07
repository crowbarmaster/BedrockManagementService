using BedrockService.Service.Management.Interfaces;
using BedrockService.Service.Networking.Interfaces;
using BedrockService.Service.Server;
using BedrockService.Service.Server.Interfaces;
using NCrontab;
using System.Timers;

namespace BedrockService.Service.Core {
    public enum ServiceStatus {
        Stopped,
        Starting,
        Started,
        Stopping
    }

    public class BedrockService : IBedrockService {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        private readonly IUpdater _updater;
        private readonly ITCPListener _tCPListener;
        private readonly FileUtilities _fileUtils;
        private CrontabSchedule? _backupCron { get; set; }
        private CrontabSchedule? _updaterCron { get; set; }
        private List<IBedrockServer> _bedrockServers { get; set; } = new();
        private System.Timers.Timer? _updaterTimer { get; set; }
        private System.Timers.Timer? _backupTimer { get; set; }
        private ServiceStatus _CurrentServiceStatus { get; set; }

        public BedrockService(IConfigurator configurator, IUpdater updater, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, ITCPListener tCPListener, FileUtilities fileUtils) {
            _fileUtils = fileUtils;
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
                _updater.Initialize();
                _configurator.LoadGlobals().Wait();
                _logger.Initialize();
                if (!bool.Parse(_serviceConfiguration.GetProp("AcceptedMojangLic").ToString())) {
                    _logger.AppendLine("You have not accepted the license. Please visit the readme for more info!");
                    return false;
                }
                _updater.CheckUpdates().Wait();
                VerifyCoreFiles();
                _configurator.LoadServerConfigurations().Wait();
                _backupCron = CrontabSchedule.TryParse(_serviceConfiguration.GetProp("BackupCron").ToString());
                _updaterCron = CrontabSchedule.TryParse(_serviceConfiguration.GetProp("UpdateCron").ToString());
                _bedrockServers.Clear();
                InitializeTimers();
                InstanciateServers();
                _tCPListener.Initialize();
                return true;
            });
        }

        public ServiceStatus GetServiceStatus() => _CurrentServiceStatus;

        public bool Start(HostControl? hostControl) {
            if (!Initialize().Result) {
                _logger.AppendLine("BedrockService did not initialize correctly.");
                Task.Delay(3000).Wait();
                Environment.Exit(1);
            }
            try {
                if (ValidSettingsCheck()) {
                    foreach (var brs in _bedrockServers) {
                        brs.AwaitableServerStart().Wait();
                        brs.StartWatchdog();
                    }
                }
                _tCPListener.SetServiceStarted();
                _CurrentServiceStatus = ServiceStatus.Started;
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error: {e.Message}.\n{e.StackTrace}");
                return false;
            }
        }

        public void TestStart() {
            Task.Run(() => Start(null));
        }

        public bool Stop(HostControl? hostControl) {
            if (ServiceShutdown()) {
                _tCPListener.CancelAllTasks().Wait();
                _logger.AppendLine("Service shutdown completed successfully.");
                return true;
            }
            _logger.AppendLine("Service shutdown completed with errors. Check logs!");
            return false;
        }

        public void TestStop() {
            Task.Run(() => Stop(null));
        }

        public bool ServiceShutdown() {
            _CurrentServiceStatus &= ServiceStatus.Stopping;
            _logger.AppendLine("Shutdown initiated...");
            try {
                foreach (var brs in _bedrockServers) {
                    brs.AwaitableServerStop(true).Wait();
                }
                _CurrentServiceStatus = ServiceStatus.Stopped;
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public Task RestartService() {
            return Task.Run(() => {
                try {
                    _tCPListener.SetServiceStopped();
                    foreach (IBedrockServer brs in _bedrockServers) {
                        brs.AwaitableServerStop(true).Wait();
                    }
                    Start(null);
                } catch (Exception e) {
                    _logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.Message} StackTrace: {e.StackTrace}");
                }
            });
        }

        public IBedrockServer GetBedrockServerByIndex(int serverIndex) {
            return _bedrockServers[serverIndex];
        }

        public IBedrockServer? GetBedrockServerByName(string name) {
            return _bedrockServers.FirstOrDefault(brs => brs.GetServerName() == name);
        }

        public void RemoveBedrockServerByIndex(int serverIndex) {
            _bedrockServers.RemoveAt(serverIndex);
        }

        public List<IBedrockServer> GetAllServers() => _bedrockServers;

        public void InitializeNewServer(IServerConfiguration server) {
            IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo, _fileUtils);
            bedrockServer.Initialize();
            _bedrockServers.Add(bedrockServer);
            _serviceConfiguration.AddNewServerInfo(server);
            if (ValidSettingsCheck()) {
                bedrockServer.AwaitableServerStart().Wait();
                bedrockServer.StartWatchdog();
            }
        }

        private void InstanciateServers() {
            try {
                List<IServerConfiguration> temp = _serviceConfiguration.GetServerList();
                foreach (IServerConfiguration server in temp) {
                    IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo, _fileUtils);
                    bedrockServer.Initialize();
                    _bedrockServers.Add(bedrockServer);
                }
            } catch (Exception e) {
                _logger.AppendLine($"Error creating server instances: {e.Message} {e.StackTrace}");
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
            while (validating) {
                if (_serviceConfiguration.GetServerList().Count() < 1) {
                    throw new Exception("No Servers Configured");
                } else {
                    var duplicatePortList = _serviceConfiguration.GetServerList()
                        .Select(x => x.GetAllProps()
                            .GroupBy(z => z.Value)
                            .SelectMany(z => z
                                .Where(y => y.KeyName.StartsWith("server-port"))))
                        .GroupBy(z => z.Select(x => x.Value))
                        .SelectMany(x => x.Key)
                        .GroupBy(x => x)
                        .Where(x => x.Count() > 1)
                        .ToList();
                    var duplicateNameList = _serviceConfiguration.GetServerList()
                        .GroupBy(x => x.GetServerName())
                        .Where(x => x.Count() > 1)
                        .ToList();
                    if (duplicateNameList.Count() > 0) {
                        throw new Exception($"Duplicate server name {duplicateNameList.First().First().GetServerName()} was found. Please check configuration files");
                    }
                    if (duplicatePortList.Count() > 0) {
                        string serverPorts = string.Join(", ", duplicatePortList.Select(x => x.Key).ToArray());
                        throw new Exception($"Duplicate ports used! Check server configurations targeting port(s) {serverPorts}");
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

        private void VerifyCoreFiles() {
            if (!File.Exists($@"{_processInfo.GetDirectory()}\Server\stock_packs.json") || !File.Exists($@"{_processInfo.GetDirectory()}\Server\stock_props.conf")) {
                _logger.AppendLine("Core file(s) found missing. Rebuilding!");
                string version = _serviceConfiguration.GetServerVersion();
                MinecraftUpdatePackageProcessor packageProcessor = new(_logger, _processInfo, _serviceConfiguration.GetServerVersion(), $@"{_processInfo.GetDirectory()}\Server");
                packageProcessor.ExtractFilesToDirectory();
                _configurator.LoadGlobals().Wait();
                _serviceConfiguration.SetServerVersion(version);
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
            } catch (Exception ex) {
                _logger.AppendLine($"Error in BackupTimer_Elapsed {ex}");
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                _updater.CheckUpdates().Wait();
                if (_updater.CheckVersionChanged()) {
                    _logger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                    foreach (IBedrockServer server in _bedrockServers) {
                        server.RestartServer();
                    }
                }
                InitializeTimers();
            } catch (Exception ex) {
                _logger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }
    }
}