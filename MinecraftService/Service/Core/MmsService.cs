
using MinecraftService.Service.Core.Interfaces;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Server.Updaters;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.SerializeModels;
using NCrontab;
using Newtonsoft.Json.Linq;
using System.Timers;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Core
{
    public class MmsService : ServiceControl {
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly MmsLogger _logger;
        private readonly ProcessInfo _processInfo;
        private readonly UserConfigManager _configurator;
        private readonly ITCPObject _tCPListener;
        private DateTime _upTime;
        private UpdaterContainer _updaterContainer;
        private List<IServerController> _loadedServers { get; set; } = new();
        private ServiceStatus _CurrentServiceStatus { get; set; }
        private Dictionary<MinecraftServerArch, IUpdater> _serverUpdaters { get; set; } = new();

        public MmsService(UserConfigManager configurator, MmsLogger logger, ServiceConfigurator serviceConfiguration, ProcessInfo serviceProcessInfo, ITCPObject tCPListener, UpdaterContainer updaters) {
            _tCPListener = tCPListener;
            _configurator = configurator;
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _processInfo = serviceProcessInfo;
            _logger = logger;
            _updaterContainer = updaters;
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
                if (!VerifyMojangLicenseAcceptance()) {
                    return false;
                }
                _updaterContainer.SetUpdaterTable(new Dictionary<MinecraftServerArch, IUpdater>(new Dictionary<MinecraftServerArch, IUpdater> {
                    { MinecraftServerArch.Bedrock, new BedrockUpdater(_logger, _serviceConfiguration) },
                    { MinecraftServerArch.Java, new JavaUpdater(_logger, _serviceConfiguration) },
                }));
                _configurator.LoadServerConfigurations().Wait();
                _loadedServers.Clear();
                InstanciateServers();
                _configurator.SaveGlobalFile();
                _serviceConfiguration.CalculateTotalBackupsAllServers();
                _tCPListener.Initialize();
                return true;
            });
        }

        private bool VerifyMojangLicenseAcceptance() {
            if (!_serviceConfiguration.GetProp(ServicePropertyKeys.AcceptedMojangLic).GetBoolValue()) {
                if (Environment.UserInteractive == true) {
                    _logger.AppendLine("You must agree to the terms set by Mojang for use of this software.\n" +
                        "Read terms at: https://minecraft.net/terms \n" +
                        "Type \"Yes\" to affirm you agree to afformentioned terms to continue:");
                    string answer = Console.ReadLine();
                    if (answer != null && answer == "Yes") {
                        _serviceConfiguration.GetProp(ServicePropertyKeys.AcceptedMojangLic).SetValue("True");
                        return true;
                    }
                }
                _logger.AppendLine("You have not accepted Mojang's EULA.\n Read terms at: https://minecraft.net/terms \n MinecraftService will now terminate.");
                return false;
            }
            return true;
        }

        public ServiceStatusModel GetServiceStatus() {
            List<Player> serviceActivePlayers = new();
            if (_loadedServers.Any()) { 
                _loadedServers.ForEach(server => {
                    serviceActivePlayers.AddRange(server.GetServerStatus().ActivePlayerList);
                });
            }
            return new ServiceStatusModel {
                ServiceStatus = _CurrentServiceStatus,
                ServiceUptime = _upTime,
                ActivePlayerList = serviceActivePlayers,
                TotalBackups = _serviceConfiguration.GetServiceBackupInfo().totalBackups,
                TotalBackupSize = _serviceConfiguration.GetServiceBackupInfo().totalSize,
                LatestVersion = _serviceConfiguration.GetLatestVersion(MinecraftServerArch.Bedrock)
            };
        }

        public bool Start(HostControl? hostControl) {
            if (!Initialize().Result) {
                _logger.AppendLine("MinecraftService did not initialize correctly.");
                Task.Delay(3000).Wait();
                Environment.Exit(1);
            }
            try {
                ValidSettingsCheck().Wait();
                if (_loadedServers.Count == 0) {
                    _logger.AppendLine("No servers are configured. Please create a configuration file or use the GUI to create a new server.");
                } else {
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
            Task.Run(() => {
                if (Start(null)) {
                    while(_CurrentServiceStatus != ServiceStatus.Stopped) {
                        Task.Delay(1000).Wait();
                    }
                }
            });
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
                while (!AllServersStopped()) {
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

        public bool RemoveServerInfoByIndex(int serverIndex) {
            try {
                string serverName = GetServerByIndex(serverIndex).GetServerName();
                _loadedServers.RemoveAt(serverIndex);
                _logger.AppendLine($"Removed server info for server {serverName}");
                if (_loadedServers.Count == 0) {
                    _logger.AppendLine("No servers are configured. Please create a configuration file or use the GUI to create a new server.");
                }
                return true;
            } catch {
                return false;
            }
        }

        public List<IServerController> GetAllServers() => _loadedServers;

        public void InitializeNewServer(IServerConfiguration server) {
            IServerController minecraftServer = ServerTypeLookup.PrepareNewServerController(server.GetServerArch(), server, _configurator, _logger, _serviceConfiguration, _processInfo);
            minecraftServer.Initialize();
            _loadedServers.Add(minecraftServer);
            _serviceConfiguration.AddNewServerInfo(server);
            ValidSettingsCheck().Wait();
            minecraftServer.ServerStart().Wait();
            minecraftServer.StartWatchdog();
        }

        private bool AllServersStopped() {
            foreach (IServerController server in _loadedServers) {
                if (!server.IsServerStopped()) {
                    return false;
                }
            }
            return true;
        }

        private void InstanciateServers() {
            try {
                foreach (IServerConfiguration server in _serviceConfiguration.GetServerList()) {
                    IServerController minecraftServer = ServerTypeLookup.PrepareNewServerController(server.GetServerArch(), server, _configurator, _logger, _serviceConfiguration, _processInfo);
                    minecraftServer.Initialize();
                    _loadedServers.Add(minecraftServer);
                }
            } catch (Exception e) {
                _logger.AppendLine($"Error creating server instances: {e.Message} {e.StackTrace}");
            }
        }

        private Task ValidSettingsCheck() {
            return Task.Run(() => {
                if (_serviceConfiguration.GetServerList().Count() > 1) {
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
                            server.GetUpdater().ReplaceBuild(server).Wait();
                        }
                        if (server.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                            if (server.GetServerVersion() != _serviceConfiguration.GetLatestVersion(server.GetServerArch())) {
                                server.GetUpdater().ReplaceBuild(server, _serviceConfiguration.GetLatestVersion(server.GetServerArch())).Wait();
                            }
                        } else {
                            if (server.GetServerVersion() != server.GetDeployedVersion()) {
                                server.GetUpdater().ReplaceBuild(server).Wait();
                            }
                        }
                    }
                }
            });
        }
    }
}
