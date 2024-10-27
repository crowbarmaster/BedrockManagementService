using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Classes.Configurations;
using MinecraftService.Shared.Classes.Updaters;
using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.SerializeModels;
using System.Globalization;
using System.Xml.Linq;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Management {
    public class ConfigManager : IConfigurator {
        private static readonly object _fileLock = new();
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly IServerLogger _logger;
        private readonly FileUtilities FileUtilities;
        private BedrockUpdater _updater;
        private IEnumTypeLookup _typeLookup;
        private List<string> serviceConfigExcludeList = new() { "MinecraftType", "ServerName", "ServerExeName", "FileName", "ServerPath", "ServerVersion" };

        public ConfigManager(IProcessInfo processInfo, ServiceConfigurator serviceConfiguration, IServerLogger logger, FileUtilities fileUtilities, IEnumTypeLookup enumTypeLookup) {
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            FileUtilities = fileUtilities;
            _updater = new(_logger, _serviceConfiguration);
            _typeLookup = enumTypeLookup;
        }

        public Task LoadGlobals() => Task.Run(() => {
            _serviceConfiguration.InitializeDefaults();
            if (File.Exists(GetServiceFilePath(MmsFileNameKeys.ServiceConfig))) {
                try {
                    _serviceConfiguration.ProcessUserConfiguration(File.ReadAllLines(GetServiceFilePath(MmsFileNameKeys.ServiceConfig)));
                    _logger.AppendLine("Loaded Service props.");
                } catch (Exception ex) {
                    _logger.AppendLine($"Error loading Service props: {ex.Message}");
                }
                return;
            }
            _logger.AppendLine("Service.conf was not found. Loaded defaults and saved to file.");
            SaveGlobalFile();
        });

        public Task LoadServerConfigurations() => Task.Run(() => {
            IServerConfiguration serverInfo;
            _serviceConfiguration.GetServerList().Clear();
            string[] files = Directory.GetFiles(GetServiceDirectory(ServiceDirectoryKeys.ServerConfigs), "*.conf");
            foreach (string file in files) {
                FileInfo fileInfo = new(file);
                try {
                    string[] fileEntries = File.ReadAllLines(file);
                    string[] archType = fileEntries[0].Split('=');
                    if (archType[0] != "MinecraftType") {
                        if (!_serviceConfiguration.GetUpdater(MinecraftServerArch.Bedrock).IsInitialized()) {
                            _serviceConfiguration.GetUpdater(MinecraftServerArch.Bedrock).Initialize();
                        }
                        serverInfo = new BedrockConfiguration(_processInfo, _logger, _serviceConfiguration);
                    } else {
                        MinecraftServerArch selectedArch = GetArchFromString(archType[1]);
                        if (!_serviceConfiguration.GetUpdater(selectedArch).IsInitialized()) {
                            _serviceConfiguration.GetUpdater(selectedArch).Initialize();
                        }
                        serverInfo = _typeLookup.PrepareNewServerByArch(selectedArch, _processInfo, _logger, _serviceConfiguration);
                    }
                    if (serverInfo.InitializeDefaults()) {
                        serverInfo.ProcessUserConfiguration(fileEntries);
                        _logger.AppendLine($"Loaded config for server {serverInfo.GetServerName()}.");
                    }
                    _serviceConfiguration.AddNewServerInfo(serverInfo);
                    SaveServerConfiguration(serverInfo);
                } catch (Exception ex) {
                    _logger.AppendLine($"Error loading server configuration file: ${fileInfo.Name}");
                    _logger.AppendLine($"Exception: {ex.Message}");
                    continue;
                }
            }
        });

        public void VerifyServerArchInit(MinecraftServerArch serverArch) {
            if (!_typeLookup.GetUpdaterByArch(serverArch).IsInitialized()) {
                _typeLookup.GetUpdaterByArch(serverArch).Initialize();
            }
        }

        public void SaveGlobalFile() {
            string[] output = new string[_serviceConfiguration.GetAllProps().Count + 3];
            int index = 0;
            output[index++] = "#Globals";
            output[index++] = string.Empty;
            foreach (Property prop in _serviceConfiguration.GetAllProps()) {
                output[index++] = $"{prop.KeyName}={prop}";
            }
            output[index++] = string.Empty;

            File.WriteAllLines(GetServiceFilePath(MmsFileNameKeys.ServiceConfig), output);
        }

        public void SavePlayerDatabase(IServerConfiguration server) {
            server.GetPlayerManager().SavePlayerDatabase();
        }

        public void SaveServerConfiguration(IServerConfiguration server) {
            int index = 0;
            if (server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString() == null) {
                server.GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue(server.GetSettingsProp(ServerPropertyKeys.ServerPath).DefaultValue);
            }
            string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
            string[] output = new string[11 + server.GetSettingsList().Count + server.GetAllProps().Count + server.GetStartCommands().Count];

            output[index++] = $"MinecraftType={MinecraftArchStrings[server.GetServerArch()]}";
            output[index++] = "#Service";
            server.GetSettingsList().ForEach(prop => {
                if (serviceConfigExcludeList.Contains(prop.KeyName)) {
                    if (server.GetServerArch() == MinecraftServerArch.Java) {
                        if (prop.KeyName != ServerPropertyStrings[ServerPropertyKeys.ServerName]) {
                            return;
                        }
                    } else {
                        return;
                    }
                }
                output[index++] = prop.PropFileFormatString();
            });
            output[index++] = string.Empty;

            output[index++] = "#Server";
            server.GetAllProps().ForEach(prop => {
                output[index++] = prop.PropFileFormatString();
            });
            if (!Directory.Exists(serverPath)) {
                Directory.CreateDirectory(serverPath);
            }
            output[index++] = string.Empty;

            output[index++] = "#StartCmds";
            foreach (StartCmdEntry startCmd in server.GetStartCommands()) {
                output[index++] = $"AddStartCmd={startCmd.Command}";
            }
            output[index++] = string.Empty;
            output[index++] = "#Persist - Do not modify";
            output[index++] = $"ServerVersion={server.GetSettingsProp(ServerPropertyKeys.ServerVersion)}";
            output[index++] = string.Empty;

            File.WriteAllLines(GetServiceFilePath(MmsFileNameKeys.ServerConfig_Name, server.GetServerName()), output);
            Task.Delay(500).Wait();
        }

        public void WriteJSONFiles(IServerConfiguration server) {
            string serverVer = server.GetServerVersion();
            System.Version allowStartVersion = System.Version.Parse("1.18.11.01");
            string whitelistFilePath = string.Empty;
            if (server.GetServerVersion() != "None" && System.Version.Parse(serverVer) >= allowStartVersion) {
                whitelistFilePath = GetServerFilePath(ServerFileNameKeys.AllowList, server);
                if (File.Exists(GetServerFilePath(ServerFileNameKeys.WhiteList, server))) {
                    File.Delete(GetServerFilePath(ServerFileNameKeys.WhiteList, server));
                }
            } else {
                whitelistFilePath = GetServerFilePath(ServerFileNameKeys.WhiteList, server);
            }
            string permFilePath = GetServerFilePath(ServerFileNameKeys.PermList, server);
            PermissionsFileModel permissionsFile = new() { FilePath = permFilePath };
            WhitelistFileModel whitelistFile = new() { FilePath = whitelistFilePath };
            if (_serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                _serviceConfiguration.GetPlayerList()
                    .Where(x => x.IsPlayerWhitelisted())
                    .Select(x => (xuid: x.GetPlayerID(), userName: x.GetUsername(), ignoreLimits: x.PlayerIgnoresLimit()))
                    .ToList().ForEach(x => {
                        whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.ignoreLimits, x.xuid, x.userName));
                    });
                _serviceConfiguration.GetPlayerList()
                    .Where(x => !x.IsDefaultRegistration())
                    .Select(x => (xuid: x.GetPlayerID(), permLevel: x.GetPermissionLevel()))
                    .ToList().ForEach(x => {
                        permissionsFile.Contents.Add(new PermissionsEntryJsonModel(x.permLevel, x.xuid));
                    });
            } else {
                server.GetPlayerList()
                    .Where(x => x.IsPlayerWhitelisted())
                    .Select(x => (xuid: x.GetPlayerID(), userName: x.GetUsername(), ignoreLimits: x.PlayerIgnoresLimit()))
                    .ToList().ForEach(x => {
                        whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.ignoreLimits, x.xuid, x.userName));
                    });
                server.GetPlayerList()
                    .Where(x => !x.IsDefaultRegistration())
                    .Select(x => (xuid: x.GetPlayerID(), permLevel: x.GetPermissionLevel()))
                    .ToList().ForEach(x => {
                        permissionsFile.Contents.Add(new PermissionsEntryJsonModel(x.permLevel, x.xuid));
                    });
            }
            permissionsFile.SaveToFile(permissionsFile.Contents);
            whitelistFile.SaveToFile(whitelistFile.Contents);
        }

        private class BackupComparer : IComparer<BackupInfoModel> {
            public int Compare(BackupInfoModel? x, BackupInfoModel? y) {
                if (x != null && y != null) {
                    DateTime xTime = DateTime.ParseExact(x.Filename.Substring(7, 17), "yyyyMMdd_HHmmssff", CultureInfo.InvariantCulture);
                    DateTime yTime = DateTime.ParseExact(y.Filename.Substring(7, 17), "yyyyMMdd_HHmmssff", CultureInfo.InvariantCulture);
                    if (xTime > yTime) {
                        return 1;
                    } else if (yTime > xTime) {
                        return -1;
                    }
                }
                return 0;
            }
        }

        public Task<List<BackupInfoModel>> EnumerateBackupsForServer(byte serverIndex) {
            return Task.Run(() => {
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                _serviceConfiguration.CalculateTotalBackupsAllServers().Wait();
                List<BackupInfoModel> newList = new();
                try {
                    foreach (FileInfo dir in new DirectoryInfo($@"{server.GetSettingsProp(ServerPropertyKeys.BackupPath)}\{server.GetServerName()}").GetFiles()) {
                        newList.Add(new BackupInfoModel(dir));
                    }
                    newList.Sort(new BackupComparer());
                    newList.Reverse();
                } catch (IOException) {
                    return newList;
                }
                return newList;
            });
        }

        public Task RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag) => Task.Run(() => {
            try {
                _logger.AppendLine("Beginning removal of selected options. Please wait!");
                Task.Delay(3000).Wait();
                switch (flag) {
                    case NetworkMessageFlags.RemoveBckPly:
                        if (DeleteAllBackups(serverInfo))
                            _logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeletePlayerFiles(serverInfo))
                            _logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveBckSrv:
                        if (DeleteAllBackups(serverInfo))
                            _logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeleteServerFiles(serverInfo))
                            _logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemovePlySrv:
                        if (DeletePlayerFiles(serverInfo))
                            _logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        if (DeleteServerFiles(serverInfo))
                            _logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveSrv:
                        if (DeleteServerFiles(serverInfo))
                            _logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemovePlayers:
                        if (DeletePlayerFiles(serverInfo))
                            _logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveBackups:
                        if (DeleteAllBackups(serverInfo))
                            _logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveAll:
                        if (DeleteAllBackups(serverInfo))
                            _logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeletePlayerFiles(serverInfo))
                            _logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        if (DeleteServerFiles(serverInfo))
                            _logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.None:
                        break;
                }
                _serviceConfiguration.RemoveServerInfo(serverInfo);
            } catch { }
        });

        public void DeleteBackupForServer(byte serverIndex, string backupName) {
            string serverName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            DirectoryInfo serverBackupDir = new($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.BackupPath)}\{serverName}");
            try {
                foreach (FileInfo file in serverBackupDir.GetFiles())
                    if (file.Name == backupName || backupName == "-RemoveAll-") {
                        file.Delete();
                        _logger.AppendLine($"Deleted backup {file.Name}.");
                    }
                _serviceConfiguration.CalculateTotalBackupsAllServers().Wait();
            } catch (IOException e) {
                _logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
        }

        private void LoadPlayerDatabase(IServerConfiguration server) {
            server.GetPlayerManager().LoadPlayerDatabase();
        }

        private bool DeleteAllBackups(IServerConfiguration serverInfo) {
            try {
                string configBackupPath = serverInfo.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                DirectoryInfo backupDirInfo = new($@"{configBackupPath}\{serverInfo.GetServerName()}");
                foreach (FileInfo dir in backupDirInfo.EnumerateFiles()) {
                    dir.Delete();
                }
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error deleting server backups: {e.Message}");
                return false;
            }
        }

        private bool DeleteServerFiles(IServerConfiguration serverInfo) {
            try {
                string serverPath = serverInfo.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue;
                DirectoryInfo serverDirInfo = new DirectoryInfo(serverPath);
                Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => {
                    _logger.AppendLine($"Deleting server files for server {serverInfo.GetServerName()}. {p.Progress}%");
                });
                FileUtilities.DeleteFilesFromDirectory(serverDirInfo, true, progress).Wait();
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error deleting server files: {e.Message}");
                return false;
            }
        }

        private bool DeletePlayerFiles(IServerConfiguration serverInfo) {
            try {
                DirectoryInfo configDirInfo = new(GetServiceDirectory(ServiceDirectoryKeys.ServerConfigs));
                foreach (DirectoryInfo dir in configDirInfo.GetDirectories()) {
                    if (dir.Name == "KnownPlayers" || dir.Name == "RegisteredPlayers") {
                        foreach (FileInfo file in dir.GetFiles()) {
                            if (file.Name.Contains($"{serverInfo.GetServerName()}")) {
                                file.Delete();
                            }
                        }
                    }
                }
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error deleting server player files: {e.Message}");
                return false;
            }
        }
    }
}

