using BedrockService.Service.Management.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System.Globalization;
using System.IO.Compression;

namespace BedrockService.Service.Management {
    public class ConfigManager : IConfigurator {
        private readonly string _serverConfigDir;
        private readonly string _globalFile;
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
            _serverConfigDir = $@"{_processInfo.GetDirectory()}\BmsConfig\ServerConfigs";
            _globalFile = $@"{_processInfo.GetDirectory()}\Service.conf";
        }

        public Task LoadGlobals() => Task.Run(() => {
            _serviceConfiguration.InitializeDefaults();
            if (!Directory.Exists(_serverConfigDir))
                Directory.CreateDirectory(_serverConfigDir);
            if (!Directory.Exists($@"{_serverConfigDir}\PlayerRecords"))
                Directory.CreateDirectory($@"{_serverConfigDir}\PlayerRecords");
            if (File.Exists($@"{_serverConfigDir}\..\latest_bedrock_ver.ini")) {
                _serviceConfiguration.SetLatestBDSVersion(File.ReadAllText($@"{_serverConfigDir}\..\latest_bedrock_ver.ini"));
            } else {
                Updater updater = new Updater(_processInfo, _logger, _serviceConfiguration);
                updater.Initialize();
                updater.CheckLatestVersion().Wait();
                _logger.AppendLine("Verifying latest update for baseline properties. Please wait!");
                _serviceConfiguration.ValidateLatestVersion();
            }
            if (File.Exists(_globalFile)) {
                _serviceConfiguration.ProcessConfiguration(File.ReadAllLines(_globalFile));
                _logger.AppendLine("Loaded Service props.");
                return;
            }
            _logger.AppendLine("Service.conf was not found. Loaded defaults and saved to file.");
            SaveGlobalFile();
        });

        public Task LoadServerConfigurations() => Task.Run(() => {
            ServerConfigurator serverInfo;
            _serviceConfiguration.GetServerList().Clear();
            string[] files = Directory.GetFiles(_serverConfigDir, "*.conf");
            foreach (string file in files) {
                FileInfo FInfo = new FileInfo(file);
                string[] fileEntries = File.ReadAllLines(file);
                serverInfo = new ServerConfigurator(_processInfo, _logger, _serviceConfiguration);
                if (serverInfo.InitializeDefaults()) {
                    serverInfo.ProcessConfiguration(fileEntries);
                    _logger.AppendLine($"Loaded config for server {serverInfo.GetServerName()}.");
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
                    if (!Directory.Exists(server.GetSettingsProp("ServerPath").ToString()))
                        Directory.CreateDirectory(server.GetSettingsProp("ServerPath").ToString());
                    string filePath = $@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\BuildArchives\Update_{buildVersion}.zip";
                    if (!File.Exists(filePath)) {
                        throw new FileNotFoundException($"Service could not locate file \"Update_{buildVersion}.zip\" and version was not found on Mojang servers!");
                    }
                    using ZipArchive archive = ZipFile.OpenRead($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\BuildArchives\Update_{buildVersion}.zip");
                    int fileCount = archive.Entries.Count;
                    for (int i = 0; i < fileCount; i++) {
                        if (i % (RoundOff(fileCount) / 6) == 0) {
                            _logger.AppendLine($"Extracting server files for server {server.GetServerName()}, {Math.Round(i / (double)fileCount, 2) * 100}% completed...");
                        }
                        if (!archive.Entries[i].FullName.EndsWith("/")) {
                            Task.Run(() => {
                                string fixedPath = $@"{server.GetSettingsProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}";
                                if (File.Exists(fixedPath)) {
                                    File.Delete(fixedPath);
                                }

                            }).Wait();
                            Task.Run(() => archive.Entries[i].ExtractToFile($@"{server.GetSettingsProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}")).Wait();
                        } else {
                            if (!Directory.Exists($@"{server.GetSettingsProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}")) {
                                Directory.CreateDirectory($@"{server.GetSettingsProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}");
                            }
                        }
                    }
                        FileInfo originalExeInfo = new($"{server.GetSettingsProp("ServerPath")}\\bedrock_server.exe");
                        FileInfo bmsExeInfo = new($"{server.GetSettingsProp("ServerPath")}\\{server.GetSettingsProp("ServerExeName")}");
                    try {
                        File.Copy(originalExeInfo.FullName, bmsExeInfo.FullName, true);
                    } catch (IOException e) {
                        if (e.Message.Contains("because it is being used by another process.")) {
                            List<Process> procList = Process.GetProcessesByName(bmsExeInfo.Name.Substring(0, bmsExeInfo.Name.Length - bmsExeInfo.Extension.Length)).ToList();
                            procList.ForEach(p => {
                                p.Kill();
                                Task.Delay(1000).Wait();
                            });
                            File.Copy($"{server.GetSettingsProp("ServerPath")}\\bedrock_server.exe", $"{server.GetSettingsProp("ServerPath")}\\{server.GetSettingsProp("ServerExeName")}", true);
                        }
                    }
                    _logger.AppendLine($"Extraction of files for {server.GetServerName()} completed.");
                    server.GetSettingsProp("DeployedVersion").SetValue(buildVersion);
                } catch (InvalidDataException e) {
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

            File.WriteAllLines(_globalFile, output);
        }

        public void SavePlayerDatabase(IServerConfiguration server) {
            lock (_fileLock) {
                string filePath = $@"{_serverConfigDir}\PlayerRecords\{server.GetServerName()}.playerdb";
                TextWriter writer = new StreamWriter(filePath);
                foreach (Player entry in server.GetPlayerList()) {
                    writer.WriteLine(entry.ToString("Known"));
                }
                writer.Flush();
                writer.Close();
            }
            lock (_fileLock) {
                string filePath = $@"{_serverConfigDir}\PlayerRecords\{server.GetServerName()}.preg";
                TextWriter writer = new StreamWriter(filePath);
                writer.WriteLine("# Registered player list");
                writer.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
                writer.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
                writer.WriteLine("");
                foreach (IPlayer player in server.GetPlayerList()) {
                    if (!player.IsDefaultRegistration())
                        writer.WriteLine(player.ToString("Registered"));
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void SaveServerConfiguration(IServerConfiguration server) {
            int index = 0;
            string serverPath = server.GetSettingsProp("ServerPath").ToString();
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

            File.WriteAllLines($@"{_serverConfigDir}\{server.GetFileName()}", output);
            if (server.GetSettingsProp("ServerPath").ToString() == null)
                server.GetSettingsProp("ServerPath").SetValue(server.GetSettingsProp("ServerPath").DefaultValue);
            if (!Directory.Exists(server.GetSettingsProp("ServerPath").ToString())) {
                Directory.CreateDirectory(server.GetSettingsProp("ServerPath").ToString());
            }
        }

        public void WriteJSONFiles(IServerConfiguration server) {
            Version allowStartVersion = Version.Parse("1.18.11.01");
            string whitelistFilePath = null;
            if (server.GetServerVersion() != "None" && Version.Parse(server.GetServerVersion()) >= allowStartVersion) {
                whitelistFilePath = $@"{server.GetSettingsProp("ServerPath")}\allowlist.json";
                if (File.Exists($@"{server.GetSettingsProp("ServerPath")}\whitelist.json")) {
                    File.Delete($@"{server.GetSettingsProp("ServerPath")}\whitelist.json");
                }
            } else {
                whitelistFilePath = $@"{server.GetSettingsProp("ServerPath")}\whitelist.json";
            }
            string permFilePath = $@"{server.GetSettingsProp("ServerPath")}\permissions.json";
            PermissionsFileModel permissionsFile = new() { FilePath = permFilePath };
            WhitelistFileModel whitelistFile = new() { FilePath = whitelistFilePath };
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
            permissionsFile.SaveToFile(permissionsFile.Contents);
            whitelistFile.SaveToFile(whitelistFile.Contents);
        }

        private class BackupComparer : IComparer<BackupInfoModel> {
            public int Compare(BackupInfoModel x, BackupInfoModel y) {
                DateTime xTime = DateTime.ParseExact(x.Filename.Substring(7, 17), "yyyyMMdd_HHmmssff", CultureInfo.InvariantCulture);
                DateTime yTime = DateTime.ParseExact(y.Filename.Substring(7, 17), "yyyyMMdd_HHmmssff", CultureInfo.InvariantCulture);
                if (xTime > yTime) {
                    return 1;
                } else if (yTime > xTime) {
                    return -1;
                } else {
                    return 0;
                }
            }
        }
        

        public Task<List<BackupInfoModel>> EnumerateBackupsForServer(byte serverIndex) {
            return Task.Run(() => {
                IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
                _serviceConfiguration.CalculateTotalBackupsAllServers().Wait();
                List<BackupInfoModel> newList = new List<BackupInfoModel>();
                try {
                    foreach (FileInfo dir in new DirectoryInfo($@"{server.GetSettingsProp("BackupPath")}\{server.GetServerName()}").GetFiles()) {
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
                File.Delete($@"{_serverConfigDir}\{serverInfo.GetFileName()}");
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
            DirectoryInfo serverBackupDir = new DirectoryInfo($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp("BackupPath")}\{serverName}");
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
            string serverName = server.GetServerName();
            string dbPath = $@"{_serverConfigDir}\PlayerRecords\{serverName}.playerdb";
            string regPath = $@"{_serverConfigDir}\PlayerRecords\{serverName}.preg";
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

        private bool DeleteAllBackups(IServerConfiguration serverInfo) {
            try {
                string configBackupPath = serverInfo.GetSettingsProp("BackupPath").ToString();
                DirectoryInfo backupDirInfo = new DirectoryInfo($@"{configBackupPath}\{serverInfo.GetServerName()}");
                foreach (FileInfo dir in backupDirInfo.EnumerateFiles()) {
                    dir.Delete();
                }
                return true;
            } catch { return false; }
        }

        private int RoundOff(int i) {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        private bool DeleteServerFiles(IServerConfiguration serverInfo) {
            try {
                new FileUtilities(_processInfo).DeleteFilesFromDirectory(new DirectoryInfo(serverInfo.GetSettingsProp("ServerPath").ToString()), false).Wait();
                return true;
            } catch { return false; }
        }

        private bool DeletePlayerFiles(IServerConfiguration serverInfo) {
            try {
                DirectoryInfo configDirInfo = new DirectoryInfo(_serverConfigDir);
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

