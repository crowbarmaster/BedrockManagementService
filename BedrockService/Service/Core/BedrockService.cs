using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Server;
using NCrontab;
using System.Threading;
using System.Timers;

namespace BedrockService.Service.Core
{
    public class BedrockService : ServiceControl, IBedrockService
    {
        private enum ServiceStatus
        {
            Stopped,
            Starting,
            Started,
            Stopping
        }
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        private readonly IUpdater _updater;
        private readonly ITCPListener _tCPListener;
        private readonly CrontabSchedule _shed;
        private readonly CrontabSchedule _updaterCron;
        private HostControl _hostControl;
        private readonly List<IBedrockServer> _bedrockServers = new List<IBedrockServer>();
        private System.Timers.Timer _updaterTimer;
        private System.Timers.Timer _cronTimer;

        public BedrockService(IConfigurator configurator, IUpdater updater, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, ITCPListener tCPListener)
        {
            _tCPListener = tCPListener;
            _configurator = configurator;
            _configurator.LoadAllConfigurations().Wait();
            _serviceConfiguration = serviceConfiguration;
            _processInfo = serviceProcessInfo;
            _updater = updater;
            _updater.CheckUpdates().Wait();
            _logger = logger;
            _shed = CrontabSchedule.TryParse(serviceConfiguration.GetProp("BackupCron").ToString());
            _updaterCron = CrontabSchedule.TryParse(serviceConfiguration.GetProp("UpdateCron").ToString());
            Initialize();
            _tCPListener.SetKeyContainer(_configurator.GetKeyContainer());
        }

        public bool Start(HostControl hostControl)
        {
            _hostControl = hostControl;
            try
            {
                ValidSettingsCheck();

                foreach (var brs in _bedrockServers)
                {
                    if (hostControl != null)
                        _hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(30));
                    brs.SetServerStatus(BedrockServer.ServerStatus.Starting);
                    brs.StartWatchdog(_hostControl);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.AppendLine($"Error Starting BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public bool Stop(HostControl hostControl)
        {
            _hostControl = hostControl;
            try
            {
                foreach (var brs in _bedrockServers)
                {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                    while (brs.GetServerStatus() == BedrockServer.ServerStatus.Stopping && !Program.IsExiting)
                        Thread.Sleep(100);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
                return false;
            }
        }

        public void RestartService()
        {
            try
            {
                foreach (IBedrockServer brs in _bedrockServers)
                {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                    while (brs.GetServerStatus() == BedrockServer.ServerStatus.Stopping && !Program.IsExiting)
                        Thread.Sleep(100);
                }
                try
                {
                    _tCPListener.ResetListener();
                }
                catch (ThreadAbortException) { }
                _configurator.LoadAllConfigurations().Wait();
                Initialize();
                foreach (var brs in _bedrockServers)
                {
                    brs.SetServerStatus(BedrockServer.ServerStatus.Starting);
                }
                Start(_hostControl);
            }
            catch (Exception e)
            {
                _logger.AppendLine($"Error Stopping BedrockServiceWrapper {e.StackTrace}");
            }
        }

        public IBedrockServer GetBedrockServerByIndex(int serverIndex)
        {
            return _bedrockServers[serverIndex];
        }

        public IBedrockServer GetBedrockServerByName(string name)
        {
            return _bedrockServers.FirstOrDefault(brs => brs.GetServerName() == name);
        }

        public List<IBedrockServer> GetAllServers() => _bedrockServers;

        public void InitializeNewServer(IServerConfiguration server)
        {
            IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo);
            _bedrockServers.Add(bedrockServer);
            _serviceConfiguration.AddNewServerInfo(server);
            if (ValidSettingsCheck())
            {
                bedrockServer.SetServerStatus(BedrockServer.ServerStatus.Starting);
                bedrockServer.StartWatchdog(_hostControl);
            }
        }

        private void Initialize()
        {
            _bedrockServers.Clear();
            if (_serviceConfiguration.GetProp("BackupEnabled").ToString() == "true" && _shed != null)
            {
                _cronTimer = new System.Timers.Timer((_shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                _cronTimer.Elapsed += CronTimer_Elapsed;
                _cronTimer.Start();
            }
            if (_serviceConfiguration.GetProp("CheckUpdates").ToString() == "true" && _updaterCron != null)
            {
                _updaterTimer = new System.Timers.Timer((_updaterCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                _updaterTimer.Elapsed += UpdateTimer_Elapsed;
                _logger.AppendLine($"Updates Enabled, will be checked in: {((float)_updaterTimer.Interval / 1000)} seconds.");
                _updaterTimer.Start();
            }
            try
            {
                List<IServerConfiguration> temp = _serviceConfiguration.GetServerList();
                foreach (IServerConfiguration server in temp)
                {
                    IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo);
                    _bedrockServers.Add(bedrockServer);
                }
            }
            catch (Exception e)
            {
                _logger.AppendLine($"Error Instantiating BedrockServiceWrapper: {e.StackTrace}");
            }
        }

        private void CronTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_cronTimer != null)
                {
                    _cronTimer.Stop();
                    _cronTimer = null;
                }
                if (_serviceConfiguration.GetProp("BackupEnabled").ToString() == "true" && _shed != null)
                {
                    BackupAllServers();
                    _cronTimer = new System.Timers.Timer((_shed.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    _cronTimer.Elapsed += CronTimer_Elapsed;
                    _cronTimer.Start();
                }
            }
            catch (Exception ex)
            {
                _logger.AppendLine($"Error in BackupTimer_Elapsed {ex}");
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_updaterTimer != null)
                {
                    _updaterTimer.Stop();
                    _updaterTimer = null;
                }
                _updater.CheckUpdates().Wait();
                if (_serviceConfiguration.GetProp("CheckUpdates").ToString() == "true" && _updater != null)
                {
                    if (_updater.CheckVersionChanged())
                    {
                        _logger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                        foreach (IBedrockServer server in _bedrockServers)
                        {
                            server.RestartServer(false);
                        }
                    }

                    _updaterTimer = new System.Timers.Timer((_updaterCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds);
                    _updaterTimer.Elapsed += UpdateTimer_Elapsed;
                    _updaterTimer.Start();
                }
            }
            catch (Exception ex)
            {
                _logger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }

        private void BackupAllServers()
        {
            _logger.AppendLine("Service started backup manager.");
            foreach (var brs in _bedrockServers)
            {
                brs.InitializeBackup();
            }
            _logger.AppendLine("Backups have been completed.");
        }

        private bool ValidSettingsCheck()
        {
            bool validating = true;
            bool dupedSettingsFound = false;
            while (validating)
            {
                if (_serviceConfiguration.GetServerList().Count() < 1)
                {
                    throw new Exception("No Servers Configured");
                }
                else
                {
                    foreach (IServerConfiguration server in _serviceConfiguration.GetServerList())
                    {
                        foreach (IServerConfiguration compareServer in _serviceConfiguration.GetServerList())
                        {
                            if (server != compareServer)
                            {
                                if (server.GetProp("server-port").Equals(compareServer.GetProp("server-port")) ||
                                    server.GetProp("server-portv6").Equals(compareServer.GetProp("server-portv6")) ||
                                    server.GetProp("server-name").Equals(compareServer.GetProp("server-name")))
                                {
                                    _logger.AppendLine($"Duplicate server settings between servers {server.GetFileName()} and {compareServer.GetFileName()}.");
                                    dupedSettingsFound = true;
                                }
                            }
                        }
                    }
                    if (dupedSettingsFound)
                    {
                        throw new Exception("Duplicate settings found! Check logs.");
                    }
                    foreach (var server in _serviceConfiguration.GetServerList())
                    {
                        if (_updater.CheckVersionChanged() || !File.Exists(server.GetProp("ServerPath") + "\\bedrock_server.exe"))
                        {
                            _configurator.ReplaceServerBuild(server).Wait();
                        }
                        if (server.GetProp("ServerExeName").ToString() != "bedrock_server.exe" && File.Exists(server.GetProp("ServerPath") + "\\bedrock_server.exe") && !File.Exists(server.GetProp("ServerPath") + "\\" + server.GetProp("ServerExeName")))
                        {
                            File.Copy(server.GetProp("ServerPath") + "\\bedrock_server.exe", server.GetProp("ServerPath") + "\\" + server.GetProp("ServerExeName"));
                        }
                    }
                    if (_updater.CheckVersionChanged())
                        _updater.MarkUpToDate();
                    else
                    {
                        validating = false;
                    }
                }
            }
            return true;
        }

        public void RemoveBedrockServerByIndex(int serverIndex)
        {
            _bedrockServers.RemoveAt(serverIndex);
        }
    }
}
