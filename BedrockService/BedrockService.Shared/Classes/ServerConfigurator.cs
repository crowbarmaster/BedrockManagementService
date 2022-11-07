﻿using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BedrockService.Shared.Classes.SharedStringBase;

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
            ServersPath = new Property(ServicePropertyStrings[ServicePropertyKeys.ServersPath], _serviceConfiguration.GetProp(ServicePropertyStrings[ServicePropertyKeys.ServersPath]).StringValue);
            ServicePropList.Clear();
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerName], "Dedicated Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.FileName], "Dedicated Server.conf"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerPath], $@"{ ServersPath }\Dedicated Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerExeName], $"BedrockService.Dedicated Server.exe"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.LiteLoaderEnabled], "false"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerAutostartEnabled], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupEnabled], "false"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupPath], $@"{ _servicePath }\ServerBackups"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupCron], "0 1 * **"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.MaxBackupCount], "25"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.AutoBackupsContainPacks], "false"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.IgnoreInactiveBackups], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.CheckUpdates], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.AutoDeployUpdates], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.UpdateCron], "0 2 * **"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.SelectedServerVersion], "Latest"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.DeployedVersion], "None"));
            return true;
        }

        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void SetServerVersion(string newVersion) => GetSettingsProp(ServerPropertyKeys.DeployedVersion).SetValue(newVersion);

        public string GetServerVersion() => GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue;

        public string GetSelectedVersion() => GetSettingsProp(ServerPropertyKeys.SelectedServerVersion).StringValue;

        public void ProcessConfiguration(string[] fileEntries) {
            if (fileEntries == null)
                return;
            foreach (string entry in fileEntries) {
                if (entry.StartsWith(ServerPropertyStrings[ServerPropertyKeys.SelectedServerVersion])) {
                    GetSettingsProp(ServerPropertyKeys.SelectedServerVersion).SetValue(entry.Split('=')[1]);
                }
                if (entry.StartsWith(ServerPropertyStrings[ServerPropertyKeys.DeployedVersion])) {
                    GetSettingsProp(ServerPropertyKeys.DeployedVersion).SetValue(entry.Split('=')[1]);
                }
            }
            if(GetSettingsProp(ServerPropertyKeys.SelectedServerVersion).StringValue == "Latest") {
                if(GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue == "None") {
                    GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue = _serviceConfiguration.GetLatestBDSVersion();
                }
                ValidateVersion(GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue);
            } else {
                ValidateVersion(GetSettingsProp(ServerPropertyKeys.SelectedServerVersion).StringValue);
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
                            GetSettingsProp(ServerPropertyKeys.ServerName).SetValue(split[1]);
                            GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue($@"{ServersPath}\{split[1]}");
                            GetSettingsProp(ServerPropertyKeys.ServerExeName).SetValue($"BedrockService.{split[1]}.exe");
                            GetSettingsProp(ServerPropertyKeys.FileName).SetValue($@"{split[1]}.conf");
                            break;

                        case "AddStartCmd":
                            StartCmds.Add(new StartCmdEntry(split[1]));
                            break;

                        case "BackupPath":
                            if (split[1] == "Default")
                                GetSettingsProp(ServerPropertyKeys.BackupPath).SetValue($@"{_servicePath}\ServerBackups");
                            break;

                        case "LiteLoaderEnabled":
                            LiteLoaderEnabled = bool.Parse(split[1]);
                            break;
                    }
                }
            }
        }

        public bool ValidateVersion(string version, bool skipNullCheck = false) {
            if (version.Contains("(ProtocolVersion")) {
                version = version.Substring(0, version.IndexOf('('));
            }
            if (_processInfo.DeclaredType() != "Client" || (GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue != "None" && !skipNullCheck)) {
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.StockProps, version))) {
                    _logger.AppendLine("Core file(s) found missing. Rebuilding!");
                    MinecraftUpdatePackageProcessor packageProcessor = new(_logger, version, GetServiceDirectory(BmsDirectoryKeys.CoreFileBuild_Ver, version));
                    if(!packageProcessor.ExtractCoreFiles()){
                        return false;
                    }
                }
                ServerPropList.Clear();
                _defaultPropList.Clear();
                File.ReadAllLines(GetServiceFilePath(BmsFileNameKeys.StockProps, version)).ToList().ForEach(entry => {
                    string[] splitEntry = entry.Split('=');
                    ServerPropList.Add(new Property(splitEntry[0], splitEntry[1]));
                    _defaultPropList.Add(new Property(splitEntry[0], splitEntry[1]));
                });
            }
            SetSettingsProp(ServerPropertyKeys.DeployedVersion, version);
            return true;
        }

        public Property GetSettingsProp (ServerPropertyKeys key) {
            string returnedValue = ServerPropertyStrings[key];
            return ServicePropList.First(prop => prop.KeyName == returnedValue);
        }

        public void SetSettingsProp(ServerPropertyKeys key, string value) {
            try {
                Property settingsProp = ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[key]);
                ServicePropList[ServicePropList.IndexOf(settingsProp)].SetValue(value);
            } catch (Exception e) {
                throw new FormatException($"Could not find key {key} in settings property list!", e);
            }
        }

        public void SetSettingsProp(string key, string value) {
            try {
                Property settingsProp = ServicePropList.First(prop => prop.KeyName == key);
                ServicePropList[ServicePropList.IndexOf(settingsProp)].SetValue(value);
            } catch (Exception e) {
                throw new FormatException($"Could not find key {key} in settings property list!", e);
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

        public void SetProp(ServerPropertyKeys key, string newValue) {
            try {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == ServerPropertyStrings[key]);
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

        public Property GetProp(BmsDependServerPropKeys key) {
            try {
            Property foundProp = ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[key]);
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
            return GetSettingsProp(ServerPropertyKeys.ServerName).ToString();
        }

        public override int GetHashCode() {
            int hashCode = -298215838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GetSettingsProp(ServerPropertyKeys.ServerName).StringValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GetSettingsProp(ServerPropertyKeys.FileName).StringValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<Property>.Default.GetHashCode(GetSettingsProp(ServerPropertyKeys.ServerPath));
            return hashCode;
        }

        public override bool Equals(object obj) {
            return obj is ServerInfo info &&
                   ServicePropList[0].StringValue == info.ServicePropList[0].StringValue &&
                   EqualityComparer<Property>.Default.Equals(GetSettingsProp(ServerPropertyKeys.ServerPath), info.ServicePropList.First(x => x.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerPath]));
        }

        public string GetServerName() => GetSettingsProp(ServerPropertyKeys.ServerName).StringValue;

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

        public string GetConfigFileName() {
            return GetSettingsProp(ServerPropertyKeys.FileName).StringValue;
        }

        public List<LogEntry> GetLog() => ConsoleBuffer = ConsoleBuffer ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => ConsoleBuffer = newLog;

        public List<IPlayer> GetPlayerList() => PlayersList ?? new List<IPlayer>();

        public void SetPlayerList(List<IPlayer> newList) => PlayersList = newList;

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = GetPlayerList().FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(GetProp(BmsDependServerPropKeys.PermLevel).ToString());
                player.Initialize(xuid, username);
                PlayersList.Add(player);
                return player;
            }
            return foundPlayer;
        }

        public IServerConfiguration GetServerInfo() => this;

        public void SetStatus (ServerStatusModel status) => ServerStatus = status;

        public ServerStatusModel GetStatus() => ServerStatus;

        public void SetLiteLoaderStatus(bool statusToSet) => LiteLoaderEnabled = statusToSet;

        public bool GetLiteLoaderStatus() => LiteLoaderEnabled;

        public LiteLoaderConfigNodeModel GetLiteLoaderConfig() => LiteLoaderConfigProps;

        public void SetLiteLoaderConfig(LiteLoaderConfigNodeModel config) => LiteLoaderConfigProps = config;
    }
}
