using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Server.Configurations;
using MinecraftService.Shared.Classes.Server.Updaters;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Service.Configuration
{
    public class ServiceConfigurator : ServiceInfo
    {
        private readonly ProcessInfo _processInfo;
        private readonly UpdaterContainer _updaterContainer;
        public ServiceConfigurator(ProcessInfo processInfo, UpdaterContainer updaters) : base()
        {
            _processInfo = processInfo;
            _updaterContainer = updaters;
        }

        public bool InitializeDefaults()
        {
            globals.Clear();
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.ServersPath], @"C:\MinecraftService"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.AcceptedMojangLic], "false"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.ClientPort], "19134"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.LogServerOutput], "true"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.LogApplicationOutput], "true"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.TimestampLogEntries], "true"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.GlobalizedPlayerDatabase], "false"));
            globals.Add(new Property(ServicePropertyStrings[ServicePropertyKeys.DefaultGlobalPermLevel], "member"));
            return true;
        }

        public Task CalculateTotalBackupsAllServers()
        {
            return Task.Run(() =>
            {
                TotalBackupsServiceWide = 0;
                TotalBackupSizeServiceWideMegabytes = 0;
                ServerList.ForEach(x =>
                {
                    var results = CalculateSingleServerBackupTotals(x).Result;
                    TotalBackupsServiceWide += results.totalBackups;
                    TotalBackupSizeServiceWideMegabytes += results.totalSize;
                });
            });
        }

        private static Task<(int totalBackups, int totalSize)> CalculateSingleServerBackupTotals(IServerConfiguration serverConfiguration)
        {
            return Task.Run(() =>
            {
                int TotalServerBackupCount = 0;
                int TotalServerBackupSize = 0;
                string backupPath = serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                string serverName = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerName).ToString();
                DirectoryInfo serverBackupDirInfo = new($"{backupPath}\\{serverName}");
                if (!serverBackupDirInfo.Exists)
                {
                    serverBackupDirInfo.Create();
                    return (0, 0);
                }
                try
                {
                    IEnumerable<FileInfo> backupFileList = serverBackupDirInfo.GetFiles();
                    foreach (FileInfo backupFile in backupFileList)
                    {
                        TotalServerBackupCount++;
                        TotalServerBackupSize += (int)(backupFile.Length / 1000);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                }
                serverConfiguration.SetBackupTotals(TotalServerBackupCount, TotalServerBackupSize);
                return (TotalServerBackupCount, TotalServerBackupSize);
            });
        }

        public void DeleteBackupForServer(byte serverIndex, string backupName) {
            string serverName = GetServerInfoByIndex(serverIndex).GetServerName();
            DirectoryInfo serverBackupDir = new($@"{GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.BackupPath)}\{serverName}");
            try {
                foreach (FileInfo file in serverBackupDir.GetFiles())
                    if (file.Name == backupName || backupName == "-RemoveAll-") {
                        file.Delete();
                    }
                CalculateTotalBackupsAllServers().Wait();
            } catch (IOException e) {
                throw new Exception($"Error deleting selected backups! {e.Message}");
            }
        }

        public (int totalBackups, int totalSize) GetServiceBackupInfo() => (TotalBackupsServiceWide, TotalBackupSizeServiceWideMegabytes);

        public string GetLatestVersion(MinecraftServerArch serverArch, bool isBeta = false)
        {
            if (!_updaterContainer.GetUpdaterTable().TryGetValue(serverArch, out IUpdater value)) return string.Empty;
            if (serverArch == MinecraftServerArch.Bedrock)
            {
                return value.GetSimpleVersionList().Last().Version;
            }
            return value.GetSimpleVersionList().Last(x => x.IsBeta == isBeta).Version;
        }

        public void ProcessUserConfiguration(string[] fileEntries)
        {
            foreach (string line in fileEntries)
            {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                {
                    string[] split = line.Split('=');
                    if (split.Length == 1)
                    {
                        split[1] = string.Empty;
                    }
                    SetProp(split[0], split[1]);
                }
            }
            if (_processInfo != null && _processInfo.DeclaredType() != "Client")
            {
                PlayerManager = new PlayerManager("Service", GetProp(ServicePropertyKeys.DefaultGlobalPermLevel).StringValue);
            }
        }

        public bool SetProp(string name, string newValue)
        {
            try
            {
                Property GlobalToEdit = globals.First(glob => glob.KeyName == name);
                globals[globals.IndexOf(GlobalToEdit)].SetValue(newValue);
                return true;
            }
            catch
            {
                // handle soon.
                return false;
            }
        }

        public bool SetProp(Property propToSet)
        {
            try
            {
                Property GlobalToEdit = globals.First(glob => glob.KeyName == propToSet.KeyName);
                globals[globals.IndexOf(GlobalToEdit)].SetValue(propToSet.StringValue);
            }
            catch
            {
                // handle soon.
                return false;
            }
            return true;
        }

        public Property GetProp(string name)
        {
            return globals.FirstOrDefault(prop => prop.KeyName == name);
        }

        public List<Property> GetAllProps() => globals;

        public void SetAllProps(List<Property> props)
        {
            foreach (Property prop in props)
            {
                SetProp(prop.KeyName, prop.StringValue);
            }
        }

        public IServerConfiguration GetServerInfoByName(string serverName)
        {
            return ServerList.FirstOrDefault(info => info.GetServerName() == serverName);
        }

        public IServerConfiguration GetServerInfoByIndex(int index)
        {
            return ServerList[index];
        }

        public void RemoveServerInfo(IServerConfiguration serverConfiguration)
        {
            ServerList.Remove(serverConfiguration.GetServerInfo());
            if (_processInfo == null || _processInfo.DeclaredType() == null || _processInfo.DeclaredType() == "Client")
            {
                return;
            }
            File.Delete(GetServiceFilePath(MmsFileNameKeys.ServerConfig_Name, serverConfiguration.GetServerName()));
        }

        public void SetServerInfo(IServerConfiguration newInfo)
        {
            ServerList[ServerList.IndexOf(newInfo.GetServerInfo())] = newInfo.GetServerInfo();
        }

        public List<IServerConfiguration> GetServerList() => ServerList;

        public void SetAllServerInfos(List<IServerConfiguration> newInfos)
        {
            ServerList = newInfos;
        }

        public void AddNewServerInfo(IServerConfiguration serverConfiguration)
        {
            if (serverConfiguration.GetProp(MmsDependServerPropKeys.PortI4).StringValue == "19132" && (serverConfiguration.GetServerArch() == MinecraftServerArch.Java || serverConfiguration.GetProp(MmsDependServerPropKeys.PortI6).StringValue == "19133"))
            {
                ServerList.Insert(0, serverConfiguration);
                return;
            }
            ServerList.Add(serverConfiguration);
        }

        public void RemoveServerInfoByIndex(int serverIndex)
        {
            ServerList.RemoveAt(serverIndex);
        }

        public List<LogEntry> GetLog() => serviceLog ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => serviceLog = newLog;

        public List<Property> GetServerDefaultPropList(MinecraftServerArch serverArch) => MinecraftFileUtilities.CopyPropList(DefaultServerPropsByArch[serverArch]);

        public void SetServerDefaultPropList(MinecraftServerArch serverArch, List<Property> newProps)
        {
            if (DefaultServerPropsByArch.ContainsKey(serverArch))
            {
                DefaultServerPropsByArch[serverArch] = newProps;
                return;
            }
            DefaultServerPropsByArch.Add(serverArch, newProps);
        }

        public byte GetServerIndex(IServerConfiguration server) => (byte)ServerList.IndexOf(server);

        public Player GetOrCreatePlayer(string xuid, string username = null) => PlayerManager.GetOrCreatePlayer(xuid, username);

        public List<Player> GetPlayerList() => PlayerManager.GetPlayerList();

        public void SetPlayerList(List<Player> playerList) => PlayerManager.GetPlayerList();

        public Property GetProp(ServicePropertyKeys keyName)
        {
            return globals.FirstOrDefault(prop => prop.KeyName == ServicePropertyStrings[keyName]);
        }

        public void SetProp(ServicePropertyKeys keyName, string value)
        {
            globals.FirstOrDefault(prop => prop.KeyName == ServicePropertyStrings[keyName]).SetValue(value);
        }

        public Property GetProp(MmsDependServerPropKeys key)
        {
            throw new NotImplementedException();
        }

        public Property GetProp(ServerPropertyKeys key)
        {
            return null;
        }

        public Property GetSettingsProp(ServerPropertyKeys key)
        {
            return null;
        }

        public void SetProp(MmsDependServerPropKeys key, string value)
        {
            throw new NotImplementedException();
        }

        public IUpdater GetUpdater(MinecraftServerArch serverArch)
        {
            return _updaterContainer.GetUpdaterByArch(serverArch);
        }

        public Dictionary<MinecraftServerArch, IUpdater> GetUpdaterTable()
        {
            return _updaterContainer.GetUpdaterTable();
        }

        public static IServerConfiguration PrepareNewServerConfig(string archName, ProcessInfo processInfo, MmsLogger logger, ServiceConfigurator service) => PrepareNewServerConfig(GetArchFromString(archName), processInfo, logger, service);

        public static IServerConfiguration PrepareNewServerConfig(MinecraftServerArch archType, ProcessInfo processInfo, MmsLogger logger, ServiceConfigurator service)
        {
            return archType switch
            {
                MinecraftServerArch.Bedrock => new BedrockConfiguration(processInfo, logger, service),
                MinecraftServerArch.Java => new JavaConfiguration(processInfo, logger, service),
                _ => null,
            };
        }

    }
}
