using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public class ServiceConfigurator : ServiceInfo, IServiceConfiguration {
        private readonly IProcessInfo _processInfo;
        public ServiceConfigurator(IProcessInfo processInfo) : base() {
            _processInfo = processInfo;
        }

        public bool InitializeDefaults() {
            globals.Clear();
            globals.Add(new Property("ServersPath", @"C:\MCBedrockService"));
            globals.Add(new Property("AcceptedMojangLic", "false"));
            globals.Add(new Property("ClientPort", "19134"));
            globals.Add(new Property("LogServerOutput", "true"));
            globals.Add(new Property("LogApplicationOutput", "true"));
            globals.Add(new Property("TimestampLogEntries", "true"));
            globals.Add(new Property("GlobalizedPlayerDatabase", "false"));
            globals.Add(new Property("DefaultGlobalPermLevel", "member"));
            globals.Add(new Property("LatestLiteLoaderVersion", "1.19.30.04|2.7.2"));
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
            if (LatestServerVersion != "None" && _processInfo.DeclaredType() != "Client") {
                if (!File.Exists($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{LatestServerVersion}\stock_packs.json") || !File.Exists($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{LatestServerVersion}\stock_props.conf")) {
                    MinecraftUpdatePackageProcessor packageProcessor = new(_processInfo, LatestServerVersion, $@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{LatestServerVersion}");
                    if (!packageProcessor.ExtractCoreFiles()) {
                        return false;
                    }
                }
                DefaultServerProps.Clear();
                File.ReadAllLines($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{LatestServerVersion}\stock_props.conf").ToList().ForEach(entry => {
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
                string backupPath = serverConfiguration.GetSettingsProp("BackupPath").ToString();
                string serverName = serverConfiguration.GetSettingsProp("ServerName").ToString();
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
                        split[0] = "LogApplicationOutput";
                    }
                    if (split[0] == "LogServersToFile") {
                        split[0] = "LogServerOutput";
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
            if(ServerList.Count == 0) {

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
                Player player = new(GetProp("DefaultGlobalPermLevel").ToString());
                player.Initialize(xuid, username);
                PlayersList.Add(player);
                return player;
            }
            return foundPlayer;
        }

        public List<IPlayer> GetPlayerList() => PlayersList;

        public void SetPlayerList(List<IPlayer> playerList) => PlayersList = playerList;
    }
}
