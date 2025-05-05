using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Server.Updaters;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Server.Configurations {
    public class BedrockConfiguration : ServerInfo, IServerConfiguration {
        private string _servicePath;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Bedrock;
        private readonly ProcessInfo _processInfo;
        private readonly MmsLogger _logger;
        private readonly ServiceConfigurator _serviceConfiguration;

        public BedrockConfiguration(ProcessInfo processInfo, MmsLogger logger, ServiceConfigurator serviceConfiguration) : base() {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _processInfo = processInfo;
        }

        public bool InitializeDefaults() {
            _servicePath = _processInfo.GetDirectory();
            DefaultPropList = _serviceConfiguration.GetServerDefaultPropList(_serverArch);
            ServerPropList = MinecraftFileUtilities.CopyPropList(DefaultPropList);
            ServersPath = new Property(ServicePropertyStrings[ServicePropertyKeys.ServersPath], _serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath).StringValue);
            ServicePropList.Clear();
            ServicePropList.AddRange(MinecraftFileUtilities.CopyPropList(CommonConfigDefaults.PropList));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerName], "Dedicated Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.FileName], "Dedicated Server.conf"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerPath], $@"{ServersPath}\Dedicated Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerExeName], $"MinecraftService.Dedicated Server.exe"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.MinecraftType], "Bedrock"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupPath], $@"{_servicePath}\ServerBackups"));
            PlayerManager = _serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue() ? _serviceConfiguration.PlayerManager : new PlayerManager(GetProp(MmsDependServerPropKeys.ServerName).StringValue, GetProp(MmsDependServerPropKeys.PermLevel).StringValue);
            return true;
        }

        public void ValidateDeployedServer() {
            string serverExePath = $@"{GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\{GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue}";
            if (GetDeployedVersion() == "None" || !File.Exists(serverExePath) || GetDeployedVersion() != GetServerVersion()) {
                _logger.AppendLine("Executable missing, or server is out of date. Replacing server build, Please wait...");
                GetUpdater().ReplaceBuild(this).Wait();
                return;
            }
        }

        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void SetServerVersion(string newVersion) {
            GetSettingsProp(ServerPropertyKeys.ServerVersion).SetValue(newVersion);
        }

        public MinecraftServerArch GetServerArch() => _serverArch;

        public string GetServerVersion() => GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() ? _serviceConfiguration.GetLatestVersion(_serverArch) : GetSettingsProp(ServerPropertyKeys.ServerVersion).StringValue;

        public void ProcessUserConfiguration(List<string[]> fileEntries) {
            foreach (string[] entry in fileEntries) {
                Property SrvProp = ServerPropList.FirstOrDefault(prop => prop != null && prop.KeyName == entry[0]);
                if (SrvProp != null) {
                    SetProp(entry[0], entry[1]);
                }
                Property SvcProp = ServicePropList.FirstOrDefault(prop => prop != null && prop.KeyName == entry[0]);
                if (SvcProp != null) {
                    SetSettingsProp(entry[0], entry[1]);
                }
                switch (entry[0]) {
                    case "server-name":
                        GetSettingsProp(ServerPropertyKeys.ServerName).SetValue(entry[1]);
                        GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue($@"{ServersPath}\{entry[1]}");
                        GetSettingsProp(ServerPropertyKeys.ServerExeName).SetValue($"MinecraftService.{entry[1]}.exe");
                        GetSettingsProp(ServerPropertyKeys.FileName).SetValue($@"{entry[1]}.conf");
                        break;

                    case "AddStartCmd":
                        StartCmds.Add(new StartCmdEntry(entry[1]));
                        break;

                    case "BackupPath":
                        if (entry[1] == "Default")
                            GetSettingsProp(ServerPropertyKeys.BackupPath).SetValue($@"{_servicePath}\ServerBackups");
                        break;
                }
            }
            PlayerManager.LoadPlayerDatabase();
            if (File.Exists(GetServerFilePath(ServerFileNameKeys.DeployedINI, this)) && GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                SetSettingsProp(ServerPropertyKeys.ServerVersion, File.ReadAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, this)));

            }
        }
        

        public void ProcessNewServerConfiguration() {
            Property srvNameProp = ServerPropList.FirstOrDefault(prop => prop != null && prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.ServerName]);
            GetSettingsProp(ServerPropertyKeys.ServerName).SetValue(srvNameProp.StringValue);
            GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue($@"{ServersPath}\{srvNameProp.StringValue}");
            GetSettingsProp(ServerPropertyKeys.ServerExeName).SetValue($"MinecraftService.{srvNameProp.StringValue}.exe");
            GetSettingsProp(ServerPropertyKeys.FileName).SetValue($@"{srvNameProp.StringValue}.conf");
        }

        public bool IsPrimaryServer() {
            return GetProp(MmsDependServerPropKeys.PortI4).StringValue == "19132" ||
            GetProp(MmsDependServerPropKeys.PortI4).StringValue == "19133" ||
            GetProp(MmsDependServerPropKeys.PortI6).StringValue == "19132" ||
            GetProp(MmsDependServerPropKeys.PortI6).StringValue == "19133";
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

        public void SetProp(MmsDependServerPropKeys key, string newValue) {
            try {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[key]);
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

        public Property GetProp(MmsDependServerPropKeys key) {
            try {
                Property foundProp = ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[key]);
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

        public List<Player> GetPlayerList() => PlayerManager.GetPlayerList();

        public void SetPlayerList(List<Player> newList) => PlayerManager.SetPlayerList(newList);

        public Player GetOrCreatePlayer(string xuid, string username = null) => PlayerManager.GetOrCreatePlayer(xuid, username);

        public PlayerManager GetPlayerManager() => PlayerManager;

        public IServerConfiguration GetServerInfo() => this;

        public void SetStatus(ServerStatusModel status) => ServerStatus = status;

        public ServerStatusModel GetStatus() => ServerStatus;

        public int GetRunningPid() => ProcessID;

        public void SetRunningPid(int runningPid) => ProcessID = runningPid;

        public Property GetProp(ServicePropertyKeys key) {
            _logger.AppendLine("GetProp destined for service called on server!");
            return null;
        }

        public string GetDeployedVersion() {
            try {
                return File.ReadAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, this));

            } catch {
                return "None";
            }
        }

        public void SetDeployedVersion(string version) {
            File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, this), version);
        }

        public IUpdater GetUpdater() => _serviceConfiguration.GetUpdater(_serverArch);
    }
}
