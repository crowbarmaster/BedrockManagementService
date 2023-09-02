
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.SerializeModels;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Core {
    public class MmsService : IMinecraftService {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        private readonly ITCPListener _tCPListener;
        private readonly IPlayerManager _playerManager;
        private DateTime _upTime;

        private List<IServerController> _loadedServers { get; set; } = new();
        private ServiceStatus _CurrentServiceStatus { get; set; }

        public MmsService(IConfigurator configurator, IServerLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo serviceProcessInfo, ITCPListener tCPListener) {
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
                DirectoryInfo bmsConfigDir = new($"{GetServiceDirectory(ServiceDirectoryKeys.WorkingDirectory)}\\BmsConfig");
                if (bmsConfigDir.Exists) {
                    bmsConfigDir.MoveTo(GetServiceDirectory(ServiceDirectoryKeys.MmsConfig));
                }
                string? startedVersion = Process.GetCurrentProcess().MainModule?.FileVersionInfo.ProductVersion;
                foreach (ServiceDirectoryKeys key in MmsDirectoryStrings.Keys) {
                    if (key != ServiceDirectoryKeys.WorkingDirectory && !Directory.Exists(GetServiceDirectory(key)) && !MmsDirectoryStrings[key].Contains("{0}")) {
                        Directory.CreateDirectory(GetServiceDirectory(key));
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
                        } else {
                            _logger.AppendLine("You have not accepted Mojang's EULA.\n Read terms at: https://minecraft.net/terms \n MinecraftService will now terminate.");
                            return false;
                        }
                    } else {
                        _logger.AppendLine("You have not accepted Mojang's EULA.\n Read terms at: https://minecraft.net/terms \n MinecraftService will now terminate.");
                        return false;
                    }
                }
                EnumTypeLookup typeLookup = new EnumTypeLookup(_logger, _serviceConfiguration);
                foreach (KeyValuePair<MinecraftServerArch, IUpdater> kvp in typeLookup.UpdatersByArch) {
                    kvp.Value.Initialize();
                }
                _configurator.LoadServerConfigurations().Wait();
                _loadedServers.Clear();
                InstanciateServers();
                _configurator.SaveGlobalFile();
                _serviceConfiguration.CalculateTotalBackupsAllServers();
                _tCPListener.Initialize();
                return true;
            });
        }

        public ServiceStatusModel GetServiceStatus() {
            List<IPlayer> serviceActivePlayers = new();
            _loadedServers.ForEach(server => {
                serviceActivePlayers.AddRange(server.GetServerStatus().ActivePlayerList);
            });
            return new ServiceStatusModel {
                ServiceStatus = _CurrentServiceStatus,
                ServiceUptime = _upTime,
                ActivePlayerList = serviceActivePlayers,
                TotalBackups = _serviceConfiguration.GetServiceBackupInfo().totalBackups,
                TotalBackupSize = _serviceConfiguration.GetServiceBackupInfo().totalSize,
                LatestVersion = _serviceConfiguration.GetLatestVersion(MinecraftServerArch.Bedrock)
            };
        }

        public IPlayerManager GetPlayerManager() => _playerManager;

        public bool Start(HostControl? hostControl) {
            if (!Initialize().Result) {
                _logger.AppendLine("MinecraftService did not initialize correctly.");
                Task.Delay(3000).Wait();
                Environment.Exit(1);
            }
            try {
                ValidSettingsCheck().Wait();
                foreach (var brs in _loadedServers) {
                    if (brs.IsPrimaryServer()) {
                        brs.ServerStart().Wait();
                    }
                    if (!brs.ServerAutostartEnabled()) {
                        continue;
                    }
                    if (!brs.IsPrimaryServer()) {
                        brs.ServerStart();
                    }
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
                foreach (var brs in _loadedServers) {
                    brs.ServerStop(true);
                }
                while (HasRunningServer()) {
                    Task.Delay(200).Wait();
                }
                _CurrentServiceStatus = ServiceStatus.Stopped;
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error Stopping MinecraftService {e.StackTrace}");
                return false;
            }
        }

        public Task RestartService() {
            return Task.Run(() => {
                try {
                    _tCPListener.SetServiceStopped();
                    foreach (IServerController brs in _loadedServers) {
                        brs.ServerStop(true).Wait();
                    }
                    GC.Collect();
                    Task.Delay(1000).Wait();
                    Start(null);
                } catch (Exception e) {
                    _logger.AppendLine($"Error Stopping MinecraftServiceWrapper {e.Message} StackTrace: {e.StackTrace}");
                }
            });
        }

        public IServerController GetServerByIndex(int serverIndex) {
            return _loadedServers[serverIndex];
        }

        public IServerController? GetServerByName(string name) {
            return _loadedServers.FirstOrDefault(brs => brs.GetServerName() == name);
        }

        public void RemoveServerByIndex(int serverIndex) {
            _loadedServers.RemoveAt(serverIndex);
        }

        public List<IServerController> GetAllServers() => _loadedServers;

        public void InitializeNewServer(IServerConfiguration server) {
            IServerController minecraftServer = ServerTypeLookup.GetServerControllerByArch(server.GetServerArch(), server, _configurator, _logger, _serviceConfiguration, _processInfo, _playerManager);
            minecraftServer.Initialize();
            _loadedServers.Add(minecraftServer);
            _serviceConfiguration.AddNewServerInfo(server);
            ValidSettingsCheck().Wait();
            minecraftServer.ServerStart().Wait();
            minecraftServer.StartWatchdog();
        }

        private bool HasRunningServer() {
            foreach(IServerController server in _loadedServers) {
                if (server.IsServerStarted()) {
                    return true;
                }
            }
            return false;
        }

        private void InstanciateServers() {
            try {
                foreach (IServerConfiguration server in _serviceConfiguration.GetServerList()) {
                    IServerController minecraftServer = ServerTypeLookup.GetServerControllerByArch(server.GetServerArch(), server, _configurator, _logger, _serviceConfiguration, _processInfo, _playerManager);
                    minecraftServer.Initialize();
                    _loadedServers.Add(minecraftServer);
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
                            .Where(y => y.KeyName.StartsWith(MmsDependServerPropStrings[MmsDependServerPropKeys.PortI4]))))
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
                    string deployedVersion = server.GetDeployedVersion();
                    string serverExePath = $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\{server.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue}";
                    FileInfo oldExeFile = new($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\BedrockService.{server.GetServerName()}.exe");
                    FileInfo oldJarFile = new($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\BedrockService.{server.GetServerName()}.jar");
                    if (oldExeFile.Exists) {
                        oldExeFile.MoveTo(serverExePath);
                    }
                    if (oldJarFile.Exists) {
                        oldJarFile.MoveTo(serverExePath);
                    }
                    if (deployedVersion == "None" || !File.Exists(serverExePath)) {
                        server.GetUpdater().ReplaceServerBuild().Wait();
                    }
                    if (server.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        if (server.GetServerVersion() != _serviceConfiguration.GetLatestVersion(server.GetServerArch())) {
                            server.GetUpdater().ReplaceServerBuild(_serviceConfiguration.GetLatestVersion(server.GetServerArch())).Wait();
                        }
                    } else {
                        if(server.GetServerVersion() != server.GetDeployedVersion()) {
                            server.GetUpdater().ReplaceServerBuild().Wait();
                        }
                    }
                }
                return true;
            });
        }
    }
}
