using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes {
    public class ServiceConfigurator : ServiceInfo, IServiceConfiguration {
        private readonly IProcessInfo _processInfo;
        public ServiceConfigurator(IProcessInfo processInfo) : base() {
            _processInfo = processInfo;
            PlayerManager = new ServicePlayerManager(this);
        }

        public bool InitializeDefaults() {
            globals.Clear();
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.ServersPath], @"C:\MCBedrockService"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.AcceptedMojangLic], "false"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.ClientPort], "19134"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.LogServerOutput], "true"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.LogApplicationOutput], "true"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.TimestampLogEntries], "true"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.GlobalizedPlayerDatabase], "false"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.DefaultGlobalPermLevel], "member"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.LatestLiteLoaderVersion], "2.10.1"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.UseBetaLiteLoaderVersions], "false"));
            return true;
        }

        public Task CalculateTotalBackupsAllServers() {
            return Task.Run(() => {
                TotalBackupsServiceWide = 0;
                TotalBackupSizeServiceWideMegabytes = 0;
                ServerList.ForEach(x => {
                    var results = CalculateSingleServerBackupTotals(x).Result;
                    TotalBackupsServiceWide += results.totalBackups;
                    TotalBackupSizeServiceWideMegabytes += results.totalSize;
                });
            });
        }

        public bool ValidateLatestVersion() {
            string propFile = GetServiceFilePath(BmsFileNameKeys.StockProps);
            if (LatestServerVersion != "None" && _processInfo.DeclaredType() != "Client") {
                if (!File.Exists(propFile)) {
                    FileInfo file = new(propFile);
                    MinecraftUpdatePackageProcessor packageProcessor = new(LatestServerVersion, file.Directory.FullName);
                    if (!packageProcessor.ExtractCoreFiles()) {
                        return false;
                    }
                }
                DefaultServerProps.Clear();
                File.ReadAllLines(propFile).ToList().ForEach(entry => {
                    string[] splitEntry = entry.Split('=');
                    DefaultServerProps.Add(new Property(splitEntry[0], splitEntry[1]));
                });
            }
            return true;
        }

        public Task<(int totalBackups, int totalSize)> CalculateSingleServerBackupTotals(IServerConfiguration serverConfiguration) {
            return Task.Run(() => {
                int TotalServerBackupCount = 0;
                int TotalServerBackupSize = 0;
                string backupPath = serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                string serverName = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerName).ToString();
                DirectoryInfo serverBackupDirInfo = new($"{backupPath}\\{serverName}");
                try {
                    IEnumerable<FileInfo> backupFileList = serverBackupDirInfo.GetFiles();
                    foreach (FileInfo backupFile in backupFileList) {
                        TotalServerBackupCount++;
                        TotalServerBackupSize += (int)(backupFile.Length / 1000);
                    }
                } catch (DirectoryNotFoundException) {

                }
                serverConfiguration.SetBackupTotals(TotalServerBackupCount, TotalServerBackupSize);
                return (TotalServerBackupCount, TotalServerBackupSize);
            });
        }

        public (int totalBackups, int totalSize) GetServiceBackupInfo() => (TotalBackupsServiceWide, TotalBackupSizeServiceWideMegabytes);

        public void SetLatestBDSVersion(string version) => LatestServerVersion = version;

        public string GetLatestBDSVersion() => LatestServerVersion;

        public void ProcessConfiguration(string[] fileEntries) {
            foreach (string line in fileEntries) {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                    string[] split = line.Split('=');
                    if (split.Length == 1) {
                        split[1] = "";
                    }
                    if (split[0] == "LogServiceToFile") {
                        split[0] = ServicePropertyStrings[ServicePropertyKeys.LogApplicationOutput];
                    }
                    if (split[0] == "LogServersToFile") {
                        split[0] = ServicePropertyStrings[ServicePropertyKeys.LogServerOutput];
                    }
                    SetProp(split[0], split[1]);
                }
            }
        }

        public bool SetProp(string name, string newValue) {
            try {
                Property GlobalToEdit = globals.First(glob => glob.KeyName == name);
                globals[globals.IndexOf(GlobalToEdit)].SetValue(newValue);
                return true;
            } catch {
                // handle soon.
                return false;
            }
        }

        public bool SetProp(Property propToSet) {
            try {
                Property GlobalToEdit = globals.First(glob => glob.KeyName == propToSet.KeyName);
                globals[globals.IndexOf(GlobalToEdit)].SetValue(propToSet.StringValue);
            } catch {
                // handle soon.
                return false;
            }
            return true;
        }

        public Property GetProp(string name) {
            return globals.FirstOrDefault(prop => prop.KeyName == name);
        }

        public List<Property> GetAllProps() => globals;

        public void SetAllProps(List<Property> props) {
            foreach (Property prop in props) {
                SetProp(prop.KeyName, prop.StringValue);
            }
        }

        public IServerConfiguration GetServerInfoByName(string serverName) {
            return ServerList.FirstOrDefault(info => info.GetServerName() == serverName);
        }

        public IServerConfiguration GetServerInfoByIndex(int index) {
            return ServerList[index];
        }

        public void RemoveServerInfo(IServerConfiguration serverConfiguration) {
            ServerList.Remove(serverConfiguration.GetServerInfo());
        }

        public void SetServerInfo(IServerConfiguration newInfo) {
            ServerList[ServerList.IndexOf(newInfo.GetServerInfo())] = newInfo.GetServerInfo();
        }

        public List<IServerConfiguration> GetServerList() => ServerList;

        public void SetAllServerInfos(List<IServerConfiguration> newInfos) {
            ServerList = newInfos;
        }

        public void AddNewServerInfo(IServerConfiguration serverConfiguration) {
            if (ServerList.Count == 0) {

            }
            if (serverConfiguration.GetProp("server-port").StringValue == "19132" && serverConfiguration.GetProp("server-portv6").StringValue == "19133") {
                ServerList.Insert(0, serverConfiguration);
                return;
            }
            ServerList.Add(serverConfiguration);
        }

        public void RemoveServerInfoByIndex(int serverIndex) {
            ServerList.RemoveAt(serverIndex);
        }

        public List<LogEntry> GetLog() => serviceLog ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => serviceLog = newLog;

        public List<Property> GetServerDefaultPropList() => new(DefaultServerProps);

        public byte GetServerIndex(IServerConfiguration server) => (byte)ServerList.IndexOf(server);

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(GetProp(ServicePropertyStrings[ServicePropertyKeys.DefaultGlobalPermLevel]).ToString());
                player.Initialize(xuid, username);
                PlayersList.Add(player);
                return player;
            }
            return foundPlayer;
        }

        public List<IPlayer> GetPlayerList() => PlayersList;

        public void SetPlayerList(List<IPlayer> playerList) => PlayersList = playerList;

        public Property GetProp(ServicePropertyKeys keyName) {
            return globals.FirstOrDefault(prop => prop.KeyName == ServicePropertyStrings[keyName]);
        }

        public void SetProp(ServicePropertyKeys keyName, string value) {
            globals.FirstOrDefault(prop => prop.KeyName == ServicePropertyStrings[keyName]).SetValue(value);
        }

        public Property GetProp(BmsDependServerPropKeys key) {
            throw new NotImplementedException();
        }

        public void SetLatestLLVersion(string version) => GetProp(ServicePropertyKeys.LatestLiteLoaderVersion).StringValue = version;


        public string GetLatestLLVersion() => GetProp(ServicePropertyKeys.LatestLiteLoaderVersion).StringValue;

        public LLServerPluginRegistry GetPluginRegistry() => LLServerPluginRegistry;

        public PluginVersionInfo GetServerPluginInfo(int serverIndex, string pluginFilename) {
            BmsServerPluginDatabase serverBase = LLServerPluginRegistry.ServerPluginList.Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).FirstOrDefault();
            if (serverBase == null) {
                GetPluginRegistry().ServerPluginList.Add(new BmsServerPluginDatabase { BmsServerName = GetServerInfoByIndex(serverIndex).GetServerName(), InstalledPlugins = new() });
            }
            PluginVersionInfo pluginVersionInfo = GetPluginRegistry().ServerPluginList
                .Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Where(y => y.PluginFileName == pluginFilename).FirstOrDefault();
            if (pluginVersionInfo == null) {
                pluginVersionInfo = new PluginVersionInfo() { PluginFileName = pluginFilename };
                LLServerPluginRegistry.ServerPluginList
                .Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Add(pluginVersionInfo);
            }
            return pluginVersionInfo;
        }

        public void SetServerPluginInfo(int serverIndex, PluginVersionInfo info) {
            BmsServerPluginDatabase serverBase = LLServerPluginRegistry.ServerPluginList.Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).FirstOrDefault();
            if (serverBase == null) {
                GetPluginRegistry().ServerPluginList.Add(new BmsServerPluginDatabase { BmsServerName = GetServerInfoByIndex(serverIndex).GetServerName(), InstalledPlugins = new() });
            }
            PluginVersionInfo pluginVersionInfo = GetPluginRegistry().ServerPluginList
                .Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Where(y => y.PluginFileName == info.PluginFileName).FirstOrDefault();
            if (pluginVersionInfo == null) {
                LLServerPluginRegistry.ServerPluginList
                .Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Add(info);
            }
            pluginVersionInfo.LiteLoaderVersion = info.LiteLoaderVersion;
            pluginVersionInfo.BedrockVersion = info.BedrockVersion;
            pluginVersionInfo.PluginFileName = info.PluginFileName;
        }

        public void RemoveServerPluginInfo(int serverIndex, string pluginFilename) {
            BmsServerPluginDatabase serverBase = LLServerPluginRegistry.ServerPluginList.Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).FirstOrDefault();
            if (serverBase == null) {
                return;
            }
            PluginVersionInfo pluginVersionInfo = GetPluginRegistry().ServerPluginList
                .Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Where(y => y.PluginFileName == pluginFilename).FirstOrDefault();
            if (pluginVersionInfo == null) {
                return;
            }
            GetPluginRegistry().ServerPluginList
                .Where(x => x.BmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Remove(pluginVersionInfo);
        }
    }
}
