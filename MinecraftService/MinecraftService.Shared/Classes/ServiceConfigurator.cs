﻿using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.SerializeModels;
using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes {
    public class ServiceConfigurator : ServiceInfo, IServiceConfiguration {
        private readonly IProcessInfo _processInfo;
        public ServiceConfigurator(IProcessInfo processInfo) : base() {
            _processInfo = processInfo;
            if (processInfo != null && processInfo.DeclaredType() != "Client") {
                PlayerManager = new ServicePlayerManager(this);
            }
        }

        public bool InitializeDefaults() {
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

        public Task<(int totalBackups, int totalSize)> CalculateSingleServerBackupTotals(IServerConfiguration serverConfiguration) {
            return Task.Run(() => {
                int TotalServerBackupCount = 0;
                int TotalServerBackupSize = 0;
                string backupPath = serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                string serverName = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerName).ToString();
                DirectoryInfo serverBackupDirInfo = new($"{backupPath}\\{serverName}");
                if (!serverBackupDirInfo.Exists) {
                    serverBackupDirInfo.Create();
                    return (0, 0);
                }
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

        public void SetLatestVersion(MinecraftServerArch serverArch, string version) {
            LatestServerVersion = version;
            try {
                File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[serverArch]), version);
            } catch (Exception) {

            }
        }

        public string GetLatestVersion(MinecraftServerArch serverArch) {
            try {
                LatestServerVersion = File.ReadAllText(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[serverArch]));
            } catch (Exception) {

            }
            return LatestServerVersion;
        }

        public void ProcessUserConfiguration(string[] fileEntries) {
            foreach (string line in fileEntries) {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                    string[] split = line.Split('=');
                    if (split.Length == 1) {
                        split[1] = string.Empty;
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
            if (_processInfo == null || _processInfo.DeclaredType() == null || _processInfo.DeclaredType() == "Client") {
                return;
            }
            File.Delete(GetServiceFilePath(MmsFileNameKeys.ServerConfig_Name, serverConfiguration.GetServerName()));
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
            if (serverConfiguration.GetProp(MmsDependServerPropKeys.PortI4).StringValue == "19132" && serverConfiguration.GetProp(MmsDependServerPropKeys.PortI6).StringValue == "19133") {
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

        public List<Property> GetServerDefaultPropList(MinecraftServerArch serverArch) => MinecraftFileUtilities.CopyPropList(DefaultServerPropsByArch[serverArch]);

        public void SetServerDefaultPropList(MinecraftServerArch serverArch, List<Property> newProps) {
            if (DefaultServerPropsByArch.ContainsKey(serverArch)) {
                DefaultServerPropsByArch[serverArch] = newProps;
                return;
            }
            DefaultServerPropsByArch.Add(serverArch, newProps);
        }

        public byte GetServerIndex(IServerConfiguration server) => (byte)ServerList.IndexOf(server);

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) => PlayerManager.GetOrCreatePlayer(xuid, username);

        public List<IPlayer> GetPlayerList() => PlayerManager.GetPlayerList();

        public void SetPlayerList(List<IPlayer> playerList) => PlayerManager.GetPlayerList();

        public Property GetProp(ServicePropertyKeys keyName) {
            return globals.FirstOrDefault(prop => prop.KeyName == ServicePropertyStrings[keyName]);
        }

        public void SetProp(ServicePropertyKeys keyName, string value) {
            globals.FirstOrDefault(prop => prop.KeyName == ServicePropertyStrings[keyName]).SetValue(value);
        }

        public Property GetProp(MmsDependServerPropKeys key) {
            throw new NotImplementedException();
        }

        public LLServerPluginRegistry GetPluginRegistry() => LLServerPluginRegistry;

        public PluginVersionInfo GetServerPluginInfo(int serverIndex, string pluginFilename) {
            MmsServerPluginDatabase serverBase = LLServerPluginRegistry.ServerPluginList.Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).FirstOrDefault();
            if (serverBase == null) {
                GetPluginRegistry().ServerPluginList.Add(new MmsServerPluginDatabase { MmsServerName = GetServerInfoByIndex(serverIndex).GetServerName(), InstalledPlugins = new() });
            }
            PluginVersionInfo pluginVersionInfo = GetPluginRegistry().ServerPluginList
                .Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Where(y => y.PluginFileName == pluginFilename).FirstOrDefault();
            if (pluginVersionInfo == null) {
                pluginVersionInfo = new PluginVersionInfo() { PluginFileName = pluginFilename };
                LLServerPluginRegistry.ServerPluginList
                .Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Add(pluginVersionInfo);
            }
            return pluginVersionInfo;
        }

        public void SetServerPluginInfo(int serverIndex, PluginVersionInfo info) {
            MmsServerPluginDatabase serverBase = LLServerPluginRegistry.ServerPluginList.Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).FirstOrDefault();
            if (serverBase == null) {
                GetPluginRegistry().ServerPluginList.Add(new MmsServerPluginDatabase { MmsServerName = GetServerInfoByIndex(serverIndex).GetServerName(), InstalledPlugins = new() });
            }
            PluginVersionInfo pluginVersionInfo = GetPluginRegistry().ServerPluginList
                .Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Where(y => y.PluginFileName == info.PluginFileName).FirstOrDefault();
            if (pluginVersionInfo == null) {
                LLServerPluginRegistry.ServerPluginList
                .Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Add(info);
            }
            pluginVersionInfo.LiteLoaderVersion = info.LiteLoaderVersion;
            pluginVersionInfo.BedrockVersion = info.BedrockVersion;
            pluginVersionInfo.PluginFileName = info.PluginFileName;
        }

        public void RemoveServerPluginInfo(int serverIndex, string pluginFilename) {
            MmsServerPluginDatabase serverBase = LLServerPluginRegistry.ServerPluginList.Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).FirstOrDefault();
            if (serverBase == null) {
                return;
            }
            PluginVersionInfo pluginVersionInfo = GetPluginRegistry().ServerPluginList
                .Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Where(y => y.PluginFileName == pluginFilename).FirstOrDefault();
            if (pluginVersionInfo == null) {
                return;
            }
            GetPluginRegistry().ServerPluginList
                .Where(x => x.MmsServerName == GetServerInfoByIndex(serverIndex).GetServerName()).First().InstalledPlugins
                .Remove(pluginVersionInfo);
        }

        public Property GetProp(ServerPropertyKeys key) {
            return null;
        }

        public Property GetSettingsProp(ServerPropertyKeys key) {
            return null;
        }

        public void SetProp(MmsDependServerPropKeys key, string value) {
            throw new NotImplementedException();
        }
    }
}
