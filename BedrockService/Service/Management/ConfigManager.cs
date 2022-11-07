using BedrockService.Shared.MinecraftFileModels.JsonModels;
using BedrockService.Shared.SerializeModels;
using BedrockService.Shared.Interfaces;
using System.Globalization;
using System.IO.Compression;
using static BedrockService.Shared.Classes.SharedStringBase;
using BedrockService.Shared.FileModels.MinecraftFileModels;

namespace BedrockService.Service.Management {
    public class ConfigManager : IConfigurator {
        private static readonly object _fileLock = new object();
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly IBedrockLogger _logger;
        private readonly FileUtilities _fileUtilities;
        private List<string> serviceConfigExcludeList = new List<string>() { "ServerName", "ServerExeName", "FileName", "ServerPath", "DeployedVersion" };

        public ConfigManager(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger, FileUtilities fileUtilities) {
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _fileUtilities = fileUtilities;
        }

        public Task LoadGlobals() => Task.Run(() => {
            _serviceConfiguration.InitializeDefaults();
            if (File.Exists(GetServiceFilePath(BmsFileNameKeys.BedrockVersionIni))) {
                _serviceConfiguration.SetLatestBDSVersion(File.ReadAllText(GetServiceFilePath(BmsFileNameKeys.BedrockVersionIni)));
            } else {
                Updater updater = new Updater(_logger, _serviceConfiguration);
                updater.Initialize();
                updater.CheckLatestVersion().Wait();
                _logger.AppendLine("Verifying latest update for baseline properties. Please wait!");
            }
            _serviceConfiguration.ValidateLatestVersion();
            if (File.Exists(GetServiceFilePath(BmsFileNameKeys.ServiceConfig))) {
                _serviceConfiguration.ProcessConfiguration(File.ReadAllLines(GetServiceFilePath(BmsFileNameKeys.ServiceConfig)));
                _logger.AppendLine("Loaded Service props.");
                return;
            }
            _logger.AppendLine("Service.conf was not found. Loaded defaults and saved to file.");
            SaveGlobalFile();
        });

        public Task LoadServerConfigurations() => Task.Run(() => {
            ServerConfigurator serverInfo;
            _serviceConfiguration.GetServerList().Clear();
            string[] files = Directory.GetFiles(GetServiceDirectory(BmsDirectoryKeys.ServerConfigs), "*.conf");
            foreach (string file in files) {
                FileInfo FInfo = new FileInfo(file);
                string[] fileEntries = File.ReadAllLines(file);
                serverInfo = new ServerConfigurator(_processInfo, _logger, _serviceConfiguration);
                if (serverInfo.InitializeDefaults()) {
                    serverInfo.ProcessConfiguration(fileEntries);
                    _logger.AppendLine($"Loaded config for server {serverInfo.GetServerName()}.");
                }
                if (serverInfo.LiteLoaderEnabled) {
                    serverInfo.LiteLoaderConfigProps = MinecraftFileUtilities.LoadLiteLoaderConfigFile(serverInfo);
                    MinecraftFileUtilities.VerifyLiteLoaderCompatableSettings(_processInfo, serverInfo);
                }
                LoadPlayerDatabase(serverInfo);
                _serviceConfiguration.AddNewServerInfo(serverInfo);
            }
            if (_serviceConfiguration.GetServerList().Count == 0) {
                serverInfo = new ServerConfigurator(_processInfo, _logger, _serviceConfiguration);
                if (!serverInfo.InitializeDefaults()) {
                    _logger.AppendLine("Error creating default server!");
                }
                serverInfo.ValidateVersion(_serviceConfiguration.GetLatestBDSVersion());
                SaveServerConfiguration(serverInfo);
                _serviceConfiguration.AddNewServerInfo(serverInfo);
                _logger.AppendLine("Successfully created and saved Default Server.");
            }
        });

        public async Task ReplaceServerBuild(IServerConfiguration server, string buildVersion) {
            await Task.Run(() => {
                try {
                    string serverVersion = string.Empty;
                    string liteVersion = string.Empty;
                    if (server.GetSettingsProp(ServerPropertyKeys.LiteLoaderEnabled).GetBoolValue()) {
                        string[] LLVersion = _serviceConfiguration.GetProp(ServicePropertyKeys.LatestLiteLoaderVersion).ToString().Split('|');
                        serverVersion = LLVersion[0];
                        liteVersion = LLVersion[1];
                        buildVersion = serverVersion;
                    }
                    if (!Directory.Exists(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString()))
                        Directory.CreateDirectory(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
                    string filePath = GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, buildVersion);
                    if (!File.Exists(filePath)) {
                        throw new FileNotFoundException($"Service could not locate file \"Update_{buildVersion}.zip\" and version was not found on Mojang servers!");
                    }
                    Progress<double> progress = new(percent =>
                    {
                        _logger.AppendLine($"Extracting Bedrock files for server {server.GetServerName()}, {percent}% completed...");
                    });
                    _fileUtilities.ExtractZipToDirectory(filePath, server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), progress).Wait();

                    FileInfo originalExeInfo = new(GetServerFilePath(BdsFileNameKeys.VanillaBedrock, server));
                    FileInfo bmsExeInfo = new(GetServerFilePath(BdsFileNameKeys.BmsServer_Name, server, server.GetServerName()));
                    try {
                        if (server.GetSettingsProp(ServerPropertyKeys.LiteLoaderEnabled).GetBoolValue()) {
                            progress = new(percent =>
                            {
                                _logger.AppendLine($"Extracting LiteLoader files for server {server.GetServerName()}, {percent}% completed...");
                            });
                            _fileUtilities.ExtractZipToDirectory(GetServiceFilePath(BmsFileNameKeys.LLUpdatePackage_Ver, liteVersion), GetServerDirectory(BdsDirectoryKeys.ServerRoot, server), progress).Wait();
                            LiteLoaderPECore.BuildLLExe(server, liteVersion);
                            server.SetSettingsProp(ServerPropertyKeys.SelectedServerVersion, buildVersion);
                            MinecraftFileUtilities.CreateDefaultLoaderConfigFile(server);
                        } else {
                            File.Copy(originalExeInfo.FullName, bmsExeInfo.FullName, true);
                        }
                    } catch (IOException e) {
                        if (e.Message.Contains("because it is being used by another process.")) {
                            List<Process> procList = Process.GetProcessesByName(bmsExeInfo.Name[..^bmsExeInfo.Extension.Length]).ToList();
                            procList.ForEach(p => {
                                p.Kill();
                                Task.Delay(1000).Wait();
                            });
                            File.Copy(GetServerFilePath(BdsFileNameKeys.VanillaBedrock, server), GetServerFilePath(BdsFileNameKeys.BmsServer_Name, server, server.GetServerName()), true);
                        }
                    }
                    _logger.AppendLine($"Extraction of files for {server.GetServerName()} completed.");
                    server.GetSettingsProp(ServerPropertyKeys.DeployedVersion).SetValue(buildVersion);
                    SaveServerConfiguration(server);
                } catch (InvalidDataException) {
                    throw new FileNotFoundException($"Build file \"Update_{buildVersion}.zip\" found corrupt. Service cannot proceed!!");
                }
            });
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

            File.WriteAllLines(GetServiceFilePath(BmsFileNameKeys.ServiceConfig), output);
        }

        public void SavePlayerDatabase(IServerConfiguration server) {
            string dbPath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName());
            string regPath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName());
            List<IPlayer> playerList = server.GetPlayerList();
            if (_serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                dbPath = GetServiceFilePath(BmsFileNameKeys.GlobalPlayerTelem);
                regPath = GetServiceFilePath(BmsFileNameKeys.GlobalPlayerRegistry);
                playerList = _serviceConfiguration.GetPlayerList();
            }
            lock (_fileLock) {
                TextWriter writer = new StreamWriter(dbPath);
                foreach (Player entry in playerList) {
                    writer.WriteLine(entry.ToString("Known"));
                }
                writer.Flush();
                writer.Close();
            }
            lock (_fileLock) {
                TextWriter writer = new StreamWriter(regPath);
                writer.WriteLine("# Registered player list");
                writer.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
                writer.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
                writer.WriteLine("");
                foreach (IPlayer player in playerList) {
                    if (!player.IsDefaultRegistration())
                        writer.WriteLine(player.ToString("Registered"));
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void SaveServerConfiguration(IServerConfiguration server) {
            int index = 0;
            string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
            string[] output = new string[10 + server.GetSettingsList().Count + server.GetAllProps().Count + server.GetStartCommands().Count];

            output[index++] = "#Service";
            server.GetSettingsList().ForEach(prop => {
                if (serviceConfigExcludeList.Contains(prop.KeyName)) {
                    return;
                }
                output[index++] = $"{prop.KeyName}={prop}";
            });
            output[index++] = string.Empty;

            output[index++] = "#Server";
            server.GetAllProps().ForEach(prop => {
                output[index++] = $"{prop.KeyName}={prop}";
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
            output[index++] = $"DeployedVersion={server.GetServerVersion()}";
            output[index++] = string.Empty;

            File.WriteAllLines(GetServiceFilePath(BmsFileNameKeys.ServiceConfig), output);
            if (server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString() == null)
                server.GetSettingsProp(ServerPropertyKeys.ServerPath).SetValue(server.GetSettingsProp(ServerPropertyKeys.ServerPath).DefaultValue);
            if (!Directory.Exists(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString())) {
                Directory.CreateDirectory(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
            }
        }

        public void WriteJSONFiles(IServerConfiguration server) {
            Version allowStartVersion = Version.Parse("1.18.11.01");
            string whitelistFilePath = string.Empty;
            if (server.GetServerVersion() != "None" && Version.Parse(server.GetServerVersion()) >= allowStartVersion) {
                whitelistFilePath = GetServerFilePath(BdsFileNameKeys.AllowList, server);
                if (File.Exists(GetServerFilePath(BdsFileNameKeys.WhiteList, server))) {
                    File.Delete(GetServerFilePath(BdsFileNameKeys.WhiteList, server));
                }
            } else {
                whitelistFilePath = GetServerFilePath(BdsFileNameKeys.WhiteList, server);
            }
            string permFilePath = GetServerFilePath(BdsFileNameKeys.PermList, server);
            PermissionsFileModel permissionsFile = new() { FilePath = permFilePath };
            WhitelistFileModel whitelistFile = new() { FilePath = whitelistFilePath };
            if (_serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                _serviceConfiguration.GetPlayerList()
                    .Where(x => x.IsPlayerWhitelisted())
                    .Select(x => (xuid: x.GetXUID(), userName: x.GetUsername(), ignoreLimits: x.PlayerIgnoresLimit()))
                    .ToList().ForEach(x => {
                        whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.ignoreLimits, x.xuid, x.userName));
                    });
                _serviceConfiguration.GetPlayerList()
                    .Where(x => !x.IsDefaultRegistration())
                    .Select(x => (xuid: x.GetXUID(), permLevel: x.GetPermissionLevel()))
                    .ToList().ForEach(x => {
                        permissionsFile.Contents.Add(new PermissionsEntryJsonModel(x.permLevel, x.xuid));
                    });
            } else {
                server.GetPlayerList()
                    .Where(x => x.IsPlayerWhitelisted())
                    .Select(x => (xuid: x.GetXUID(), userName: x.GetUsername(), ignoreLimits: x.PlayerIgnoresLimit()))
                    .ToList().ForEach(x => {
                        whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.ignoreLimits, x.xuid, x.userName));
                    });
                server.GetPlayerList()
                    .Where(x => !x.IsDefaultRegistration())
                    .Select(x => (xuid: x.GetXUID(), permLevel: x.GetPermissionLevel()))
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
                List<BackupInfoModel> newList = new List<BackupInfoModel>();
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

        public void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag) {
            try {
                _logger.AppendLine("Beginning removal of selected options. This may take up to five minutes, depending on selections and server features. Please wait!");
                File.Delete(GetServiceFilePath(BmsFileNameKeys.ServiceConfig));
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
        }

        public void DeleteBackupForServer(byte serverIndex, string backupName) {
            string serverName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            DirectoryInfo serverBackupDir = new DirectoryInfo($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.BackupPath)}\{serverName}");
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
            if (_serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                string dbPath = GetServiceFilePath(BmsFileNameKeys.GlobalPlayerTelem);
                string regPath = GetServiceFilePath(BmsFileNameKeys.GlobalPlayerRegistry);
                _fileUtilities.CreateInexistantFile(regPath);
                _fileUtilities.CreateInexistantFile(dbPath);
                List<string[]> playerDbEntries = File.ReadAllLines(dbPath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
                List<string[]> playerRegEntries = File.ReadAllLines(regPath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
                playerDbEntries.ForEach(x => {
                    _serviceConfiguration.GetOrCreatePlayer(x[0]).UpdatePlayerFromDbStrings(x);
                });
                playerRegEntries.ForEach(x => {
                    _serviceConfiguration.GetOrCreatePlayer(x[0]).UpdatePlayerFromRegStrings(x);
                });

            } else {
                string serverName = server.GetServerName();
                string dbPath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName());
                string regPath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName());
                _fileUtilities.CreateInexistantFile(regPath);
                _fileUtilities.CreateInexistantFile(dbPath);
                List<string[]> playerDbEntries = File.ReadAllLines(dbPath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
                List<string[]> playerRegEntries = File.ReadAllLines(regPath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
                playerDbEntries.ForEach(x => {
                    server.GetOrCreatePlayer(x[0]).UpdatePlayerFromDbStrings(x);
                });
                playerRegEntries.ForEach(x => {
                    server.GetOrCreatePlayer(x[0]).UpdatePlayerFromRegStrings(x);
                });
            }
        }

        private bool DeleteAllBackups(IServerConfiguration serverInfo) {
            try {
                string configBackupPath = serverInfo.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                DirectoryInfo backupDirInfo = new DirectoryInfo($@"{configBackupPath}\{serverInfo.GetServerName()}");
                foreach (FileInfo dir in backupDirInfo.EnumerateFiles()) {
                    dir.Delete();
                }
                return true;
            } catch { return false; }
        }

        private static bool DeleteServerFiles(IServerConfiguration serverInfo) {
            try {
                new FileUtilities().DeleteFilesFromDirectory(new DirectoryInfo(serverInfo.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString()), false).Wait();
                return true;
            } catch { return false; }
        }

        private static bool DeletePlayerFiles(IServerConfiguration serverInfo) {
            try {
                DirectoryInfo configDirInfo = new DirectoryInfo(GetServiceDirectory(BmsDirectoryKeys.ServerConfigs));
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
            } catch { return false; }
        }
    }
}

