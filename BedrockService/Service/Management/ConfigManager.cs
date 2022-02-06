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
        private string _loadedVersion;

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
            if (File.Exists($@"{_serverConfigDir}\..\bedrock_ver.ini"))
                _loadedVersion = File.ReadAllText($@"{_serverConfigDir}\..\bedrock_ver.ini");
            if (File.Exists(_globalFile)) {
                _logger.AppendLine("Loading Globals...");
                _serviceConfiguration.ProcessConfiguration(File.ReadAllLines(_globalFile));
                _serviceConfiguration.SetServerVersion(_loadedVersion);
                return;
            }
            _logger.AppendLine("Globals.conf was not found. Loaded defaults and saved to file.");
            SaveGlobalFile();
        });

        public Task LoadServerConfigurations() => Task.Run(() => {
            ServerConfigurator serverInfo;
            _serviceConfiguration.GetServerList().Clear();
            string[] files = Directory.GetFiles(_serverConfigDir, "*.conf");
            foreach (string file in files) {
                FileInfo FInfo = new FileInfo(file);
                string[] fileEntries = File.ReadAllLines(file);
                serverInfo = new ServerConfigurator(_serviceConfiguration.GetProp("ServersPath").ToString(), _serviceConfiguration.GetServerDefaultPropList());
                if (serverInfo.InitializeDefaults()) {
                    serverInfo.ProcessConfiguration(fileEntries);
                }
                LoadPlayerDatabase(serverInfo);
                _serviceConfiguration.AddNewServerInfo(serverInfo);
            }
            if (_serviceConfiguration.GetServerList().Count == 0) {
                serverInfo = new ServerConfigurator(_serviceConfiguration.GetProp("ServersPath").ToString(), _serviceConfiguration.GetServerDefaultPropList());
                if (!serverInfo.InitializeDefaults()) {
                    _logger.AppendLine("Error creating default server!");
                }
                SaveServerConfiguration(serverInfo);
                _serviceConfiguration.AddNewServerInfo(serverInfo);
            }
        });

        public async Task ReplaceServerBuild(IServerConfiguration server) {
            await Task.Run(() => {
                try {
                    if (!Directory.Exists(server.GetProp("ServerPath").ToString()))
                        Directory.CreateDirectory(server.GetProp("ServerPath").ToString());
                    while (_serviceConfiguration.GetServerVersion() == null || _serviceConfiguration.GetServerVersion() == "None") {
                        Task.Delay(200).Wait();
                    }
                    using (ZipArchive archive = ZipFile.OpenRead($@"{_processInfo.GetDirectory()}\BmsConfig\BDSUpdates\Update_{ _serviceConfiguration.GetServerVersion()}.zip")) {
                        int fileCount = archive.Entries.Count;
                        for (int i = 0; i < fileCount; i++) {
                            if (i % (RoundOff(fileCount) / 6) == 0) {
                                _logger.AppendLine($"Extracting server files for server {server.GetServerName()}, {Math.Round(i / (double)fileCount, 2) * 100}% completed...");
                            }
                            if (!archive.Entries[i].FullName.EndsWith("/")) {
                                string fixedPath = $@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}";
                                if (File.Exists(fixedPath)) {
                                    File.Delete(fixedPath);
                                }
                                archive.Entries[i].ExtractToFile($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}");
                            } else {
                                if (!Directory.Exists($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}")) {
                                    Directory.CreateDirectory($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}");
                                }
                            }
                        }
                        _logger.AppendLine($"Extraction of files for {server.GetServerName()} completed.");
                    }
                    File.Copy(server.GetProp("ServerPath").ToString() + "\\bedrock_server.exe", server.GetProp("ServerPath").ToString() + "\\" + server.GetProp("ServerExeName").ToString(), true);
                } catch (Exception e) {
                    _logger.AppendLine($"ERROR: Got an exception deleting entire directory! {e.Message}");
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
            string serverPath = server.GetProp("ServerPath").ToString();
            string[] output = new string[8 + server.GetAllProps().Count + server.GetStartCommands().Count];

            output[index++] = "#Service";
            output[index++] = $"{server.GetProp("ServerAutostartEnabled").KeyName}={server.GetProp("ServerAutostartEnabled").Value}";
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

            File.WriteAllLines($@"{_serverConfigDir}\{server.GetFileName()}", output);
            if (server.GetProp("ServerPath").ToString() == null)
                server.GetProp("ServerPath").SetValue(server.GetProp("ServerPath").DefaultValue);
            if (!Directory.Exists(server.GetProp("ServerPath").ToString())) {
                Directory.CreateDirectory(server.GetProp("ServerPath").ToString());
            }
            File.WriteAllLines($@"{server.GetProp("ServerPath")}\server.properties", output);

        }

        public void WriteJSONFiles(IServerConfiguration server) {
            string permFilePath = $@"{server.GetProp("ServerPath")}\permissions.json";
            string whitelistFilePath = $@"{server.GetProp("ServerPath")}\whitelist.json";
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

        public List<Property> EnumerateBackupsForServer(byte serverIndex) {
            string serverName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            List<Property> newList = new List<Property>();
            try {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{serverName}").GetDirectories()) {
                    string[] splitName = dir.Name.Split('_');
                    newList.Add(new Property(dir.Name, DateTime.ParseExact(splitName[1], "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture).ToString("G")));
                }
            } catch (IOException) {
                return newList;
            }
            return newList;
        }

        public void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag) {
            try {
                File.Delete($@"{_serverConfigDir}\{serverInfo.GetFileName()}");
                switch (flag) {
                    case NetworkMessageFlags.RemoveBckPly:
                        if (DeleteBackups(serverInfo))
                            _logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeletePlayerFiles(serverInfo))
                            _logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveBckSrv:
                        if (DeleteBackups(serverInfo))
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
                        if (DeleteBackups(serverInfo))
                            _logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveAll:
                        if (DeleteBackups(serverInfo))
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

        public void DeleteBackupsForServer(byte serverIndex, List<string> list) {
            string serverName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            try {
                foreach (string deleteDir in list)
                    foreach (DirectoryInfo dir in new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{serverName}").GetDirectories())
                        if (dir.Name == deleteDir) {
                            new FileUtilities(_processInfo).DeleteFilesRecursively(new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{serverName}\{deleteDir}"), true);
                            _logger.AppendLine($"Deleted backup {deleteDir}.");
                        }
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

        private bool DeleteBackups(IServerConfiguration serverInfo) {
            try {
                string configBackupPath = "";
                DirectoryInfo backupDirInfo = new DirectoryInfo($@"{configBackupPath}\{serverInfo.GetServerName()}");
                DirectoryInfo configBackupDirInfo = new DirectoryInfo($@"{_serverConfigDir}\Backups");
                foreach (DirectoryInfo dir in backupDirInfo.GetDirectories()) {
                    if (dir.Name.Contains($"{serverInfo.GetServerName()}")) {
                        dir.Delete(true);
                    }
                }
                foreach (FileInfo file in configBackupDirInfo.GetFiles()) {
                    if (file.Name.Contains($"{serverInfo.GetServerName()}_")) {
                        file.Delete();
                    }
                }
                return true;
            } catch { return false; }
        }

        private int RoundOff(int i) {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        private bool DeleteServerFiles(IServerConfiguration serverInfo) {
            try {
                new FileUtilities(_processInfo).DeleteFilesRecursively(new DirectoryInfo(serverInfo.GetProp("ServerPath").ToString()), false);
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

