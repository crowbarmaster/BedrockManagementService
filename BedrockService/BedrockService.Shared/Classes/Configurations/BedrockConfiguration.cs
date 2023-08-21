using BedrockService.Shared.Classes.Updaters;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using BedrockService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes.Configurations {
    public class BedrockConfiguration : ServerInfo, IServerConfiguration {
        private string _servicePath;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Bedrock;
        private readonly IProcessInfo _processInfo;
        private readonly IServerLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IUpdater _updater;

        public BedrockConfiguration(IProcessInfo processInfo, IServerLogger logger, IServiceConfiguration serviceConfiguration) : base() {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _processInfo = processInfo;
            _updater = new BedrockUpdater(logger, _serviceConfiguration, this);
        }

        public bool InitializeDefaults() {
            _servicePath = _processInfo.GetDirectory();
            DefaultPropList = _serviceConfiguration.GetServerDefaultPropList(_serverArch);
            ServerPropList = MinecraftFileUtilities.CopyPropList(DefaultPropList);
            ServersPath = new Property(ServicePropertyStrings[ServicePropertyKeys.ServersPath], _serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath).StringValue);
            ServicePropList.Clear();
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerName], "Dedicated Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.FileName], "Dedicated Server.conf"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerPath], $@"{ServersPath}\Dedicated Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerExeName], $"BedrockService.Dedicated Server.exe"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.MinecraftType], "Bedrock"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerAutostartEnabled], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupEnabled], "false"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupPath], $@"{_servicePath}\ServerBackups"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupCron], "0 1 * * *"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.MaxBackupCount], "25"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.AutoBackupsContainPacks], "false"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.IgnoreInactiveBackups], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.CheckUpdates], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.AutoDeployUpdates], "true"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.UpdateCron], "0 2 * * *"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerVersion], "None"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.UseBetaVersions], "false"));
            PlayerManager = new BedrockPlayerManager(this);
            return true;
        }

        public void ValidateDeployedServer() {
            string serverExePath = $@"{GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\{GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue}";
            if (GetDeployedVersion() == "None" || !File.Exists(serverExePath) || GetDeployedVersion() != GetServerVersion()) {
                _logger.AppendLine("Executable missing, or server is out of date. Replacing server build, Please wait...");
                GetUpdater().ReplaceServerBuild().Wait();
                return;
            }
        }

        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void SetServerVersion(string newVersion) {
            GetSettingsProp(ServerPropertyKeys.ServerVersion).SetValue(newVersion);
        }

        public MinecraftServerArch GetServerArch() => _serverArch;

        public string GetServerVersion() => GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() ? _serviceConfiguration.GetLatestVersion(_serverArch) : GetSettingsProp(ServerPropertyKeys.ServerVersion).StringValue;

        public void ProcessUserConfiguration(string[] fileEntries) {
            if (fileEntries == null)
                return;

            foreach (string line in fileEntries) {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                    string[] split = line.Split('=');
                    if (split.Length == 1) {
                        split = new string[] { split[0], "" };
                    }
                    Property SrvProp = ServerPropList.FirstOrDefault(prop => prop != null && prop.KeyName == split[0]);
                    if (SrvProp != null) {
                        SetProp(split[0], split[1]);
                    }
                    Property SvcProp = ServicePropList.FirstOrDefault(prop => prop != null && prop.KeyName == split[0]);
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
                    }
                }
            }
            PlayerManager.LoadPlayerDatabase();
            if (File.Exists(GetServerFilePath(BdsFileNameKeys.DeployedINI, this)) && GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                SetSettingsProp(ServerPropertyKeys.ServerVersion, File.ReadAllText(GetServerFilePath(BdsFileNameKeys.DeployedINI, this)));
            }
        }

        public void ProcessNewServerConfiguration() {
            Property srvNameProp = ServerPropList.FirstOrDefault(prop => prop != null && prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.ServerName]);
            GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue($@"{ServersPath}\{srvNameProp.StringValue}");
            GetSettingsProp(ServerPropertyKeys.ServerExeName).SetValue($"BedrockService.{srvNameProp.StringValue}.exe");
            GetSettingsProp(ServerPropertyKeys.FileName).SetValue($@"{srvNameProp.StringValue}.conf");
        }

        public bool IsPrimaryServer() {
            return GetProp(BmsDependServerPropKeys.PortI4).StringValue == "19132" ||
            GetProp(BmsDependServerPropKeys.PortI4).StringValue == "19133" ||
            GetProp(BmsDependServerPropKeys.PortI6).StringValue == "19132" ||
            GetProp(BmsDependServerPropKeys.PortI6).StringValue == "19133";
        }

        public void UpdateServerProps(string version) {
            DefaultPropList.Clear();
            DefaultPropList = MinecraftFileUtilities.GetDefaultPropListFromFile(GetServiceFilePath(BmsFileNameKeys.BedrockStockProps_Ver, version));
            List<Property> newList = new List<Property>();
            ServerPropList.ForEach(prop => {
                if (DefaultPropList.Where(x => x.KeyName == prop.KeyName).Count() > 0) {
                    newList.Add(prop);
                }
            });
            ServerPropList = newList;
            SetSettingsProp(ServerPropertyKeys.ServerVersion, version);
        }

        public Property GetSettingsProp(ServerPropertyKeys key) {
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
                Property serverProp = ServerPropList.First(prop => prop.KeyName == propToSet.KeyName);
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

        public List<LogEntry> GetLog() => ServerLogs ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => ServerLogs = newLog;

        public List<IPlayer> GetPlayerList() => PlayerManager.GetPlayerList();

        public void SetPlayerList(List<IPlayer> newList) => PlayerManager.SetPlayerList(newList);

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) => PlayerManager.GetOrCreatePlayer(xuid, username);

        public IPlayerManager GetPlayerManager() => PlayerManager;

        public IServerConfiguration GetServerInfo() => this;

        public void SetStatus(ServerStatusModel status) => ServerStatus = status;

        public ServerStatusModel GetStatus() => ServerStatus;

        public LiteLoaderConfigNodeModel GetLiteLoaderConfig() => LiteLoaderConfigProps;

        public void SetLiteLoaderConfig(LiteLoaderConfigNodeModel config) => LiteLoaderConfigProps = config;

        public int GetRunningPid() => ProcessID;

        public void SetRunningPid(int runningPid) => ProcessID = runningPid;

        public Property GetProp(ServicePropertyKeys key) {
            _logger.AppendLine("GetProp destined for service called on server!");
            return null;
        }

        public string GetDeployedVersion() {
            try {
                return File.ReadAllText(GetServerFilePath(BdsFileNameKeys.DeployedINI, this));

            } catch {
                return "None";
            }
        }

        public void SetDeployedVersion(string version) {
            File.WriteAllText(GetServerFilePath(BdsFileNameKeys.DeployedINI, this), version);
        }

        public IUpdater GetUpdater() => _updater;
    }
}
