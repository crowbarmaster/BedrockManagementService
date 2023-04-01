
using BedrockService.Service.Networking.Interfaces;
using BedrockService.Service.Server;
using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.SerializeModels;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Core {
    public class BedrockService : IBedrockService {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        private readonly ITCPListener _tCPListener;
        private readonly FileUtilities _fileUtils;
        private readonly IPlayerManager _playerManager;
        private DateTime _upTime;

        private List<IBedrockServer> _bedrockServers { get; set; } = new();
        private ServiceStatus _CurrentServiceStatus { get; set; }

        public BedrockService(IConfigurator configurator, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, ITCPListener tCPListener, FileUtilities fileUtils) {
            _fileUtils = fileUtils;
            _tCPListener = tCPListener;
            _configurator = configurator;
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _processInfo = serviceProcessInfo;
            _playerManager = new ServicePlayerManager(serviceConfiguration);
            _logger = logger;
        }

        public Task<bool> Initialize() {
            return Task.Run(() => {
                string? startedVersion = Process.GetCurrentProcess().MainModule?.FileVersionInfo.ProductVersion;
                foreach (BmsDirectoryKeys key in BmsDirectoryStrings.Keys) {
                    if (key != BmsDirectoryKeys.WorkingDirectory && !Directory.Exists(GetServiceDirectory(key))) {
                        Directory.CreateDirectory(GetServiceDirectory(key));
                    }
                }
                if (!File.Exists($@"{_processInfo.GetDirectory()}\ServiceVersion.ini")) {
                    if (UpgradeAssistant_26RC2.IsUpgradeRequired(_processInfo.GetDirectory())) {
                        _logger.AppendLine("Former service of version 2.6RC2 found. Upgrading...");
                        UpgradeAssistant_26RC2.PerformUpgrade(_processInfo.GetDirectory());
                        _logger.AppendLine($"Upgrade to version {startedVersion} has completed.");
                    }
                }
                if (startedVersion != null) {
                    File.WriteAllText($@"{_processInfo.GetDirectory()}\ServiceVersion.ini", startedVersion);
                }
                _CurrentServiceStatus = ServiceStatus.Starting;
                _configurator.LoadGlobals().Wait();
                _logger.Initialize();
                if (!_serviceConfiguration.GetProp(ServicePropertyKeys.AcceptedMojangLic).GetBoolValue()) {
                    if (Environment.UserInteractive == true) {
                        _logger.AppendLine("You must agree to the terms set by Mojang for use of this software.\n" +
                            "Read terms at: https://minecraft.net/terms \n" +
                            "Type \"Yes\" to affirm you agree to afformentioned terms to continue:");
                        string answer = Console.ReadLine();
                        if (answer != null && answer == "Yes") {
                            _serviceConfiguration.GetProp(ServicePropertyKeys.AcceptedMojangLic).SetValue("True");
                        }
                    } else {
                        return false;
                    }
                }
                _configurator.LoadServerConfigurations().Wait();
                _bedrockServers.Clear();
                InstanciateServers();
                _configurator.SaveGlobalFile();
                _serviceConfiguration.CalculateTotalBackupsAllServers();
                _tCPListener.Initialize();
                return true;
            });
        }

        public ServiceStatusModel GetServiceStatus() {
            List<IPlayer> serviceActivePlayers = new();
            _bedrockServers.ForEach(server => {
                serviceActivePlayers.AddRange(server.GetServerStatus().ActivePlayerList);
            });
            return new ServiceStatusModel {
                ServiceStatus = _CurrentServiceStatus,
                ServiceUptime = _upTime,
                ActivePlayerList = serviceActivePlayers,
                TotalBackups = _serviceConfiguration.GetServiceBackupInfo().totalBackups,
                TotalBackupSize = _serviceConfiguration.GetServiceBackupInfo().totalSize,
                LatestVersion = _serviceConfiguration.GetLatestBDSVersion()
            };
        }

        public IPlayerManager GetPlayerManager() => _playerManager;

        public bool Start(HostControl? hostControl) {
            if (!Initialize().Result) {
                _logger.AppendLine("BedrockService did not initialize correctly.");
                Task.Delay(3000).Wait();
                Environment.Exit(1);
            }
            try {
                ValidSettingsCheck().Wait();
                foreach (var brs in _bedrockServers) {
                    if (!brs.ServerAutostartEnabled() && brs.IsPrimaryServer()) {
                        continue;
                    }
                    brs.AwaitableServerStart().Wait();
                    brs.StartWatchdog();
                }

                _tCPListener.SetServiceStarted();
                _CurrentServiceStatus = ServiceStatus.Started;
                _upTime = DateTime.Now;
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
                    GC.Collect();
                    Task.Delay(1000).Wait();
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
            IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo, _fileUtils, _playerManager);
            bedrockServer.Initialize();
            _bedrockServers.Add(bedrockServer);
            _serviceConfiguration.AddNewServerInfo(server);
            ValidSettingsCheck().Wait();
            bedrockServer.AwaitableServerStart().Wait();
            bedrockServer.StartWatchdog();
        }

        private void InstanciateServers() {
            try {
                List<IServerConfiguration> temp = _serviceConfiguration.GetServerList();
                foreach (IServerConfiguration server in temp) {
                    IBedrockServer bedrockServer = new BedrockServer(server, _configurator, _logger, _serviceConfiguration, _processInfo, _fileUtils, _playerManager);
                    bedrockServer.Initialize();
                    _bedrockServers.Add(bedrockServer);
                }
            } catch (Exception e) {
                _logger.AppendLine($"Error creating server instances: {e.Message} {e.StackTrace}");
            }
        }

        private Task ValidSettingsCheck() {
            return Task.Run(() => {
                if (_serviceConfiguration.GetServerList().Count() < 1) {
                    throw new Exception("No Servers Configured");
                }
                var duplicatePortList = _serviceConfiguration.GetServerList()
                    .Select(x => x.GetAllProps()
                        .GroupBy(z => z.StringValue)
                        .SelectMany(z => z
                            .Where(y => y.KeyName.StartsWith(BmsDependServerPropStrings[BmsDependServerPropKeys.PortI4]))))
                    .GroupBy(z => z.Select(x => x.StringValue))
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
                    string serverExePath = $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\{server.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue}";
                    if (!File.Exists(serverExePath)) {
                        string deployedVersion = server.GetSelectedVersion() == "Latest"
                                                ? _serviceConfiguration.GetLatestBDSVersion()
                                                : server.GetSelectedVersion();
                        _configurator.ReplaceServerBuild(server, deployedVersion).Wait();
                    }
                    if (File.Exists(GetServerFilePath(BdsFileNameKeys.DeployedBedrockVerIni, server))) {
                        server.SetServerVersion(File.ReadAllText(GetServerFilePath(BdsFileNameKeys.DeployedBedrockVerIni, server)));
                    }
                    if (File.Exists(GetServerFilePath(BdsFileNameKeys.DeployedLLBDSIni, server))) {
                        server.GetSettingsProp(ServerPropertyKeys.DeployedLiteLoaderVersion).SetValue(File.ReadAllText(GetServerFilePath(BdsFileNameKeys.DeployedLLBDSIni, server)));
                    }
                    if (server.GetServerVersion() != "None" && server.GetSelectedVersion() != "Latest" && server.GetSelectedVersion() != server.GetServerVersion()) {
                        _logger.AppendLine("Manually configured server found with wrong version. Replacing server build...");
                        if (Updater.FetchBuild(server.GetSelectedVersion()).Result) {
                            _configurator.ReplaceServerBuild(server, server.GetSelectedVersion()).Wait();
                            _configurator.SaveServerConfiguration(server);
                        }
                    }
                }
                return true;
            });
        }
    }
}