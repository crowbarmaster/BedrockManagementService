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

namespace MinecraftService.Shared.Classes.Server.Configurations
{
    public class JavaConfiguration : ServerInfo, IServerConfiguration
    {
        private string _servicePath;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Java;
        private readonly ProcessInfo _processInfo;
        private readonly MmsLogger _logger;
        private readonly ServiceConfigurator _serviceConfiguration;

        public JavaConfiguration(ProcessInfo processInfo, MmsLogger logger, ServiceConfigurator serviceConfiguration) : base()
        {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _processInfo = processInfo;
        }

        public bool InitializeDefaults()
        {
            _servicePath = _processInfo.GetDirectory();
            DefaultPropList = _serviceConfiguration.GetServerDefaultPropList(_serverArch);
            ServerPropList = MinecraftFileUtilities.CopyPropList(DefaultPropList);
            ServersPath = new Property(ServicePropertyStrings[ServicePropertyKeys.ServersPath], _serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath).StringValue);
            ServicePropList.Clear();
            ServicePropList.AddRange(MinecraftFileUtilities.CopyPropList(CommonConfigDefaults.PropList));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerName], "Java Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.FileName], "Java Server.conf"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerPath], $@"{ServersPath}\Java Server"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.ServerExeName], $"MinecraftService.Java Server.jar"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.MinecraftType], "Java"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.BackupPath], $@"{_servicePath}\ServerBackups"));
            ServicePropList.Add(new Property(ServerPropertyStrings[ServerPropertyKeys.JavaArgs], "-Xmx1024M -Xms1024M -XX:+UnlockExperimentalVMOptions -XX:+AlwaysPreTouch -XX:+UseStringDeduplication -Dfml.ignorePatchDiscrepancies=true -Dfml.ignoreInvalidMinecraftCertificates=true -XX:-OmitStackTraceInFastThrow -XX:+OptimizeStringConcat -Dfml.readTimeout=180 -XX:+UseLargePages"));
            PlayerManager = new PlayerManager(GetProp(MmsDependServerPropKeys.ServerName).StringValue, GetProp(MmsDependServerPropKeys.PermLevel).StringValue);
            return true;
        }

        public void ValidateDeployedServer()
        {
            string serverExePath = $@"{GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\{GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue}";
            if (GetDeployedVersion() == "None" || !File.Exists(serverExePath) || GetDeployedVersion() != GetServerVersion())
            {
                _logger.AppendLine("Executable missing, or server is out of date. Replacing server build, Please wait...");
                GetUpdater().ReplaceBuild(this).Wait();
                return;
            }
        }

        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void SetServerVersion(string newVersion)
        {
            GetSettingsProp(ServerPropertyKeys.ServerVersion).SetValue(newVersion);
        }

        public MinecraftServerArch GetServerArch() => _serverArch;

        public string GetServerVersion() => GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() ? _serviceConfiguration.GetLatestVersion(_serverArch) : GetSettingsProp(ServerPropertyKeys.ServerVersion).StringValue;

        public void ProcessUserConfiguration(string[] fileEntries)
        {
            if (fileEntries == null)
                return;

            foreach (string line in fileEntries)
            {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                {
                    string[] split = line.Split('=');
                    if (split.Length == 1)
                    {
                        split = new string[] { split[0], "" };
                    }
                    Property SrvProp = ServerPropList.FirstOrDefault(prop => prop != null && prop.KeyName == split[0]);
                    if (SrvProp != null)
                    {
                        SetProp(split[0], split[1]);
                    }
                    Property SvcProp = ServicePropList.FirstOrDefault(prop => prop != null && prop.KeyName == split[0]);
                    if (SvcProp != null)
                    {
                        SetSettingsProp(split[0], split[1]);
                    }
                    switch (split[0])
                    {
                        case "ServerName":
                            GetSettingsProp(ServerPropertyKeys.ServerName).SetValue(split[1]);
                            GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue($@"{ServersPath}\{split[1]}");
                            GetSettingsProp(ServerPropertyKeys.ServerExeName).SetValue($"MinecraftService.{split[1]}.jar");
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
            if (File.Exists(GetServerFilePath(ServerFileNameKeys.DeployedINI, this)) && GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue())
            {
                SetSettingsProp(ServerPropertyKeys.ServerVersion, File.ReadAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, this)));
            }
        }

        public void ProcessNewServerConfiguration()
        {
            Property srvNameProp = ServicePropList.FirstOrDefault(prop => prop != null && prop.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerName]);
            GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue($@"{ServersPath}\{srvNameProp.StringValue}");
            GetSettingsProp(ServerPropertyKeys.ServerExeName).SetValue($"MinecraftService.{srvNameProp.StringValue}.jar");
            GetSettingsProp(ServerPropertyKeys.FileName).SetValue($@"{srvNameProp.StringValue}.conf");
        }

        public bool IsPrimaryServer()
        {
            return GetProp(MmsDependServerPropKeys.PortI4).StringValue == "19132";
        }

        public void UpdateServerProps(string version)
        {
            DefaultPropList.Clear();
            DefaultPropList = _serviceConfiguration.GetUpdater(_serverArch).GetVersionPropList(version);
            List<Property> newList = new List<Property>();
            ServerPropList.ForEach(prop =>
            {
                if (DefaultPropList.Where(x => x.KeyName == prop.KeyName).Count() > 0)
                {
                    newList.Add(prop);
                }
            });
            ServerPropList = newList;
            SetSettingsProp(ServerPropertyKeys.ServerVersion, version);
        }

        public Property GetSettingsProp(ServerPropertyKeys key)
        {
            string returnedValue = ServerPropertyStrings[key];
            return ServicePropList.First(prop => prop.KeyName == returnedValue);
        }

        public void SetSettingsProp(ServerPropertyKeys key, string value)
        {
            try
            {
                Property settingsProp = ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[key]);
                ServicePropList[ServicePropList.IndexOf(settingsProp)].SetValue(value);
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {key} in settings property list!", e);
            }
        }

        public void SetSettingsProp(string key, string value)
        {
            try
            {
                Property settingsProp = ServicePropList.First(prop => prop.KeyName == key);
                ServicePropList[ServicePropList.IndexOf(settingsProp)].SetValue(value);
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {key} in settings property list!", e);
            }
        }

        public void SetProp(string key, string newValue)
        {
            try
            {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == key);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(newValue);
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {key} in server property list!", e);
            }
        }


        public void SetProp(MmsDependServerPropKeys key, string newValue)
        {
            try
            {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[key]);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(newValue);
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {key} in server property list!", e);
            }
        }

        public bool SetProp(Property propToSet)
        {
            try
            {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == propToSet.KeyName);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(propToSet.StringValue);
                return true;
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {propToSet.KeyName} in server property list!", e);
            }
        }

        public List<Property> GetSettingsList() => ServicePropList;

        public Property GetProp(string key)
        {
            try
            {
                Property foundProp = ServerPropList.First(prop => prop.KeyName == key);
                return foundProp;
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {key} in server property list!", e);
            }

        }

        public Property GetProp(MmsDependServerPropKeys key)
        {
            try
            {
                Property foundProp = ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[key]);
                return foundProp;
            }
            catch (Exception e)
            {
                throw new FormatException($"Could not find key {key} in server property list!", e);
            }

        }

        public void SetBackupTotals(int totalBackups, int totalSize)
        {
            ServerStatus.TotalBackups = totalBackups;
            ServerStatus.TotalSizeOfBackups = totalSize;
        }

        public void AddStartCommand(string command)
        {
            StartCmds.Add(new StartCmdEntry(command));
        }

        public bool DeleteStartCommand(string command)
        {
            StartCmdEntry entry = StartCmds.FirstOrDefault(prop => prop.Command == command);
            return StartCmds.Remove(entry);
        }

        public List<StartCmdEntry> GetStartCommands() => StartCmds;

        public override string ToString()
        {
            return GetSettingsProp(ServerPropertyKeys.ServerName).ToString();
        }

        public override int GetHashCode()
        {
            int hashCode = -298215838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GetSettingsProp(ServerPropertyKeys.ServerName).StringValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GetSettingsProp(ServerPropertyKeys.FileName).StringValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<Property>.Default.GetHashCode(GetSettingsProp(ServerPropertyKeys.ServerPath));
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is ServerInfo info &&
                   ServicePropList[0].StringValue == info.ServicePropList[0].StringValue &&
                   EqualityComparer<Property>.Default.Equals(GetSettingsProp(ServerPropertyKeys.ServerPath), info.ServicePropList.First(x => x.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerPath]));
        }

        public string GetServerName() => GetSettingsProp(ServerPropertyKeys.ServerName).StringValue;

        public List<Property> GetAllProps() => ServerPropList;

        public void SetAllProps(List<Property> newPropList)
        {
            newPropList.ForEach(x =>
            {
                SetProp(x);
            });
        }

        public void SetAllSettings(List<Property> settingsList)
        {
            settingsList.ForEach(x =>
            {
                SetSettingsProp(x.KeyName, x.StringValue);
            });
        }

        public string GetConfigFileName()
        {
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

        public Property GetProp(ServicePropertyKeys key)
        {
            _logger.AppendLine("GetProp destined for service called on server!");
            return null;
        }

        public string GetDeployedVersion()
        {
            try
            {
                return File.ReadAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, this));

            }
            catch
            {
                return "None";
            }
        }

        public void SetDeployedVersion(string version)
        {
            File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, this), version);
        }

        public IUpdater GetUpdater() => _serviceConfiguration.GetUpdater(_serverArch);
    }
}
