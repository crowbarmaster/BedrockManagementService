using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public class ServerConfigurator : ServerInfo, IServerConfiguration {
        private string _servicePath;
        private readonly IProcessInfo _processInfo;
        private readonly IBedrockLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;

        public ServerConfigurator(IProcessInfo processInfo, IBedrockLogger logger, IServiceConfiguration serviceConfiguration) : base() {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _processInfo = processInfo;
            
        }

        public bool InitializeDefaults() {
            _servicePath = _processInfo.GetDirectory();
            _defaultPropList = _serviceConfiguration.GetServerDefaultPropList();
            ServersPath = new Property("ServersPath", _serviceConfiguration.GetProp("ServersPath").StringValue);
            ServicePropList.Clear();
            ServicePropList.Add(new Property("ServerName", "Dedicated Server"));
            ServicePropList.Add(new Property("FileName", "Dedicated Server.conf"));
            ServicePropList.Add(new Property("ServerPath", $@"{ServersPath}\Dedicated Server"));
            ServicePropList.Add(new Property("ServerExeName", $"BedrockService.Dedicated Server.exe"));
            ServicePropList.Add(new Property("ServerAutostartEnabled", "true"));
            ServicePropList.Add(new Property("BackupEnabled", "false"));
            ServicePropList.Add(new Property("BackupPath", $@"{_servicePath}\ServerBackups"));
            ServicePropList.Add(new Property("BackupCron", "0 1 * * *"));
            ServicePropList.Add(new Property("MaxBackupCount", "25"));
            ServicePropList.Add(new Property("AutoBackupsContainPacks", "false"));
            ServicePropList.Add(new Property("IgnoreInactiveBackups", "true"));
            ServicePropList.Add(new Property("CheckUpdates", "true"));
            ServicePropList.Add(new Property("AutoDeployUpdates", "true"));
            ServicePropList.Add(new Property("UpdateCron", "0 2 * * *"));
            ServicePropList.Add(new Property("SelectedServerVersion", "Latest"));
            ServicePropList.Add(new Property("DeployedVersion", "None"));
            return true;
        }

        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void SetServerVersion(string newVersion) => GetSettingsProp("DeployedVersion").SetValue(newVersion);

        public string GetServerVersion() => GetSettingsProp("DeployedVersion").StringValue;

        public string GetSelectedVersion() => GetSettingsProp("SelectedServerVersion").StringValue == "Latest" ? _serviceConfiguration.GetLatestBDSVersion() : GetSettingsProp("SelectedServerVersion").StringValue;

        public void ProcessConfiguration(string[] fileEntries) {
            if (fileEntries == null)
                return;
            foreach (string entry in fileEntries) {
                if (entry.StartsWith("SelectedServerVersion")) {
                    GetSettingsProp("SelectedServerVersion").SetValue(entry.Split('=')[1]);
                }
                if (entry.StartsWith("DeployedVersion")) {
                    GetSettingsProp("DeployedVersion").SetValue(entry.Split('=')[1]);
                }
            }
            if(GetSettingsProp("SelectedServerVersion").StringValue == "Latest") {
                if(GetSettingsProp("DeployedVersion").StringValue == "None") {
                    GetSettingsProp("DeployedVersion").StringValue = _serviceConfiguration.GetLatestBDSVersion();
                }
                ValidateVersion(GetSettingsProp("DeployedVersion").StringValue);
            } else {
                ValidateVersion(GetSettingsProp("SelectedServerVersion").StringValue);
            }
            foreach (string line in fileEntries) {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                    string[] split = line.Split('=');
                    if (split.Length == 1) {
                        split = new string[] { split[0], "" };
                    }
                    Property SrvProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == split[0]);
                    if (SrvProp != null) {
                        SetProp(split[0], split[1]);
                    }
                    Property SvcProp = ServicePropList.FirstOrDefault(prop => prop.KeyName == split[0]);
                    if (SvcProp != null) {
                        SetSettingsProp(split[0], split[1]);
                    }
                    switch (split[0]) {
                        case "server-name":
                            GetSettingsProp("ServerName").SetValue(split[1]);
                            GetSettingsProp("ServerPath").SetValue($@"{ServersPath}\{split[1]}");
                            GetSettingsProp("ServerExeName").SetValue($"BedrockService.{split[1]}.exe");
                            GetSettingsProp("FileName").SetValue($@"{split[1]}.conf");
                            break;

                        case "AddStartCmd":
                            StartCmds.Add(new StartCmdEntry(split[1]));
                            break;

                        case "BackupPath":
                            if (split[1] == "Default")
                                GetSettingsProp("BackupPath").SetValue($@"{_servicePath}\ServerBackups");
                            break;
                    }
                }
            }
        }

        public bool ValidateVersion(string version, bool skipNullCheck = false) {
            if (_processInfo.DeclaredType() != "Client" || (GetSettingsProp("DeployedVersion").StringValue != "None" && !skipNullCheck)) {
                if (!File.Exists($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{version}\stock_packs.json") || !File.Exists($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{version}\stock_props.conf")) {
                    _logger.AppendLine("Core file(s) found missing. Rebuilding!");
                    MinecraftUpdatePackageProcessor packageProcessor = new(_logger, _processInfo, version, $@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{version}");
                    if(!packageProcessor.ExtractBuildToDirectory()){
                        return false;
                    }
                }
                ServerPropList.Clear();
                _defaultPropList.Clear();
                File.ReadAllLines($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\CoreFiles\Build_{version}\stock_props.conf").ToList().ForEach(entry => {
                    string[] splitEntry = entry.Split('=');
                    ServerPropList.Add(new Property(splitEntry[0], splitEntry[1]));
                    _defaultPropList.Add(new Property(splitEntry[0], splitEntry[1]));
                });
            }
            SetSettingsProp("DeployedVersion", version);
            return true;
        }

        public Property GetSettingsProp (string key) {
            return ServicePropList.First(prop => prop.KeyName == key);
        }

        public void SetSettingsProp(string key, string value) {
            if (!string.IsNullOrEmpty(key)) {
                try {
                    Property settingsProp = ServicePropList.First(prop => prop.KeyName == key);
                    ServicePropList[ServicePropList.IndexOf(settingsProp)].SetValue(value);
                } catch (Exception e) {
                    throw new FormatException($"Could not find key {key} in settings property list!", e);
                }
            }
        }

        public void SetProp(string key, string newValue) {
            try {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == key);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(newValue);
            } catch (Exception e) {
                throw new FormatException($"Could not find key {key} in server property list!", e);
            }
        }

        public bool SetProp(Property propToSet) {
            try {
                Property serverProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == propToSet.KeyName);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(propToSet.StringValue);
                return true;
            } catch (Exception e) {
                throw new FormatException($"Could not find key {propToSet.KeyName} in server property list!", e);
            }
        }

        public List<Property> GetSettingsList() => ServicePropList;

        public Property GetProp(string key) {
            try {
            Property foundProp = ServerPropList.First(prop => prop.KeyName == key);
            return foundProp;
            } catch (Exception e) {
                throw new FormatException($"Could not find key {key} in server property list!", e);
            }

        }

        public void SetBackupTotals(int totalBackups, int totalSize) {
            ServerStatus.TotalBackups = totalBackups;
            ServerStatus.TotalSizeOfBackups = totalSize; 
        }

        public void AddStartCommand(string command) {
            StartCmds.Add(new StartCmdEntry(command));
        }

        public bool DeleteStartCommand(string command) {
            StartCmdEntry entry = StartCmds.FirstOrDefault(prop => prop.Command == command);
            return StartCmds.Remove(entry);
        }

        public List<StartCmdEntry> GetStartCommands() => StartCmds;

        public override string ToString() {
            return GetSettingsProp("ServerName").ToString();
        }

        public override int GetHashCode() {
            int hashCode = -298215838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GetSettingsProp("ServerName").StringValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GetSettingsProp("FileName").StringValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<Property>.Default.GetHashCode(GetSettingsProp("ServerPath"));
            return hashCode;
        }

        public override bool Equals(object obj) {
            return obj is ServerInfo info &&
                   ServicePropList[0].StringValue == info.ServicePropList[0].StringValue &&
                   EqualityComparer<Property>.Default.Equals(GetSettingsProp("ServerPath"), info.ServicePropList.First(x => x.KeyName == "ServerPath"));
        }

        public string GetServerName() => GetSettingsProp("ServerName").StringValue;

        public List<Property> GetAllProps() => ServerPropList;

        public void SetAllProps(List<Property> newPropList) {
            newPropList.ForEach(x => {
                SetProp(x);
            });
        }

        public void SetAllSettings(List<Property> settingsList) {
            settingsList.ForEach(x => {
                SetSettingsProp(x.KeyName, x.StringValue);
            });
        }

        public void AddUpdatePlayer(IPlayer player) {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == player.GetXUID());
            if (foundPlayer == null) {
                PlayersList.Add(player);
                return;
            }
            PlayersList[PlayersList.IndexOf(foundPlayer)] = player;
        }

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(GetProp("default-player-permission-level").ToString());
                player.Initialize(xuid, username);
                PlayersList.Add(player);
                return player;
            }
            return foundPlayer;
        }

        public string GetFileName() {
            return GetSettingsProp("FileName").StringValue;
        }

        public List<LogEntry> GetLog() => ConsoleBuffer = ConsoleBuffer ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => ConsoleBuffer = newLog;

        public List<IPlayer> GetPlayerList() => PlayersList ?? new List<IPlayer>();

        public void SetPlayerList(List<IPlayer> newList) => PlayersList = newList;

        public IServerConfiguration GetServerInfo() => this;

        public void SetStatus (ServerStatusModel status) => ServerStatus = status;

        public ServerStatusModel GetStatus() => ServerStatus;
    }
}
