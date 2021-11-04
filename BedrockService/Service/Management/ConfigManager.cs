using BedrockService.Service.Server;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BedrockService.Service.Management
{
    public class ConfigManager : IConfigurator
    {
        private readonly string _configDir;
        private readonly string _globalFile;
        private string _loadedVersion;
        private static readonly object _fileLock = new object();
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly ILogger _logger;

        public ConfigManager(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, ILogger logger)
        {
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _configDir = $@"{_processInfo.GetDirectory()}\Server\Configs";
            _globalFile = $@"{_processInfo.GetDirectory()}\Service\Globals.conf";
        }

        public async Task LoadAllConfigurations()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(_configDir))
                    Directory.CreateDirectory(_configDir);
                if (!Directory.Exists($@"{_configDir}\KnownPlayers\Backups"))
                    Directory.CreateDirectory($@"{_configDir}\KnownPlayers\Backups");
                if (!Directory.Exists($@"{_configDir}\RegisteredPlayers\Backups"))
                    Directory.CreateDirectory($@"{_configDir}\RegisteredPlayers\Backups");
                if (!Directory.Exists($@"{_configDir}\Backups"))
                    Directory.CreateDirectory($@"{_configDir}\Backups");
                if (File.Exists($@"{_configDir}\..\bedrock_ver.ini"))
                    _loadedVersion = File.ReadAllText($@"{_configDir}\..\bedrock_ver.ini");
                ServerInfo serverInfo;
                LoadGlobals();
                _serviceConfiguration.GetServerList().Clear();
                string[] files = Directory.GetFiles(_configDir, "*.conf");
                foreach (string file in files)
                {
                    FileInfo FInfo = new FileInfo(file);
                    string[] fileEntries = File.ReadAllLines(file);
                    serverInfo = new ServerInfo(fileEntries, _serviceConfiguration.GetProp("ServersPath").ToString());
                    LoadPlayerDatabase(serverInfo);
                    LoadRegisteredPlayers(serverInfo);
                    _serviceConfiguration.AddNewServerInfo(serverInfo);
                }
                if (_serviceConfiguration.GetServerList().Count == 0)
                {
                    serverInfo = new ServerInfo(null, _serviceConfiguration.GetProp("ServersPath").ToString());
                    serverInfo.InitializeDefaults();
                    SaveServerProps(serverInfo, true);
                    _serviceConfiguration.AddNewServerInfo(serverInfo);
                }
            });
        }

        public void SaveGlobalFile()
        {
            string[] output = new string[_serviceConfiguration.GetAllProps().Count + 3];
            int index = 0;
            output[index++] = "#Globals";
            output[index++] = string.Empty;
            foreach (Property prop in _serviceConfiguration.GetAllProps())
            {
                output[index++] = $"{prop.KeyName}={prop}";
            }
            output[index++] = string.Empty;

            File.WriteAllLines(_globalFile, output);
        }

        public void LoadRegisteredPlayers(IServerConfiguration server)
        {
            string serverName = server.GetServerName();
            string filePath = $@"{_configDir}\RegisteredPlayers\{serverName}.preg";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return;
            }
            foreach (string entry in File.ReadLines(filePath))
            {
                if (entry.StartsWith("#") || string.IsNullOrWhiteSpace(entry))
                    continue;
                string[] split = entry.Split(',');
                _logger.AppendLine($"Server \"{server.GetServerName()}\" Loaded registered player: {split[1]}");
                IPlayer playerFound = server.GetPlayerByXuid(split[0]);
                if (playerFound == null)
                {
                    server.AddUpdatePlayer(new Player(split[0], split[1], DateTime.Now.Ticks.ToString(), "0", "0", split[3].ToLower() == "true", split[2], split[4].ToLower() == "true"));
                    continue;
                }
                string[] playerTimes = playerFound.GetTimes();
                server.AddUpdatePlayer(new Player(split[0], split[1], playerTimes[0], playerTimes[1], playerTimes[2], split[3].ToLower() == "true", split[2], split[4].ToLower() == "true"));
            }
        }

        private void LoadGlobals()
        {
            if (File.Exists(_globalFile))
            {
                _logger.AppendLine("Loading Globals...");
                _serviceConfiguration.ProcessConfiguration(File.ReadAllLines(_globalFile));
                _serviceConfiguration.SetServerVersion(_loadedVersion);
                return;
            }
            _serviceConfiguration.InitializeDefaults();
            _logger.AppendLine("Globals.conf was not found. Loaded defaults and saved to file.");
            SaveGlobalFile();
        }

        private int RoundOff(int i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        public async Task ReplaceServerBuild(IServerConfiguration server)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(server.GetProp("ServerPath").ToString()))
                        Directory.CreateDirectory(server.GetProp("ServerPath").ToString());
                    while (_serviceConfiguration.GetServerVersion() == null || _serviceConfiguration.GetServerVersion() == "None")
                    {
                        Thread.Sleep(150);
                    }
                    using (ZipArchive archive = ZipFile.OpenRead($@"{_processInfo.GetDirectory()}\Server\MCSFiles\Update_{ _serviceConfiguration.GetServerVersion()}.zip"))
                    {
                        int fileCount = archive.Entries.Count;
                        for(int i=0; i<fileCount; i++)
                        {
                            if(i%(RoundOff(fileCount)/6) == 0)
                            {
                                _logger.AppendLine($"Extracting server files for server {server.GetServerName()}, {Math.Round((double)i / (double)fileCount, 2) * 100}% completed...");
                            }
                            if (!archive.Entries[i].FullName.EndsWith("/"))
                            {
                                if (File.Exists($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}"))
                                {
                                    File.Delete($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}");
                                }
                                archive.Entries[i].ExtractToFile($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}");
                            }
                            else
                            {
                                if (!Directory.Exists($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}"))
                                {
                                    Directory.CreateDirectory($@"{server.GetProp("ServerPath")}\{archive.Entries[i].FullName.Replace('/', '\\')}");
                                }
                            }
                        }
                        _logger.AppendLine($"Extraction of files for {server.GetServerName()} completed.");
                    }
                    File.Copy(server.GetProp("ServerPath").ToString() + "\\bedrock_server.exe", server.GetProp("ServerPath").ToString() + "\\" + server.GetProp("ServerExeName").ToString(), true);
                }
                catch (Exception e)
                {
                    _logger.AppendLine($"ERROR: Got an exception deleting entire directory! {e.Message}");
                }
            });
        }

        public void LoadPlayerDatabase(IServerConfiguration server)
        {
            string filePath = $@"{_configDir}\KnownPlayers\{server.GetServerName()}.playerdb";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return;
            }
            foreach (string entry in File.ReadLines(filePath))
            {
                if (entry.StartsWith("#") || string.IsNullOrWhiteSpace(entry))
                    continue;
                string[] split = entry.Split(',');
                _logger.AppendLine($"Server \"{server.GetServerName()}\" loaded known player: {split[1]}");
                IPlayer playerFound = server.GetPlayerByXuid(split[0]);
                if (playerFound == null)
                {
                    server.AddUpdatePlayer(new Player(split[0], split[1], split[2], split[3], split[4], false, server.GetProp("default-player-permission-level").ToString(), false));
                    continue;
                }
                string[] playerTimes = playerFound.GetTimes();
                server.AddUpdatePlayer(new Player(split[0], split[1], playerTimes[0], playerTimes[1], playerTimes[2], playerFound.IsPlayerWhitelisted(), playerFound.GetPermissionLevel(), playerFound.PlayerIgnoresLimit()));
            }
        }

        public void SaveKnownPlayerDatabase(IServerConfiguration server)
        {
            lock (_fileLock)
            {
                string filePath = $@"{_configDir}\KnownPlayers\{server.GetServerName()}.playerdb";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{_configDir}\KnownPlayers\Backups\{server.GetServerName()}_{DateTime.Now:mmddyyhhmmssff}.dbbak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                foreach (Player entry in server.GetPlayerList())
                {
                    writer.WriteLine(entry.ToString("Known"));
                }
                writer.Flush();
                writer.Close();
            }
            lock (_fileLock)
            {
                string filePath = $@"{_configDir}\RegisteredPlayers\{server.GetServerName()}.preg";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{_configDir}\RegisteredPlayers\Backups\{server.GetServerName()}_{DateTime.Now:mmddyyhhmmssff}.bak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                writer.WriteLine("# Registered player list");
                writer.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
                writer.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
                writer.WriteLine("");
                foreach (IPlayer player in server.GetPlayerList())
                {
                    if (!player.IsDefaultRegistration())
                        writer.WriteLine(player.ToString("Registered"));
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void WriteJSONFiles(IServerConfiguration server)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[\n");

            foreach (IPlayer player in server.GetPlayerList())
            {
                string[] playerReg = player.GetRegistration();
                if (!player.IsDefaultRegistration() && playerReg[0] == "True")
                {
                    sb.Append("\t{\n");
                    sb.Append($"\t\t\"ignoresPlayerLimit\": {playerReg[0].ToLower()},\n");
                    sb.Append($"\t\t\"name\": \"{player.GetUsername()}\",\n");
                    sb.Append($"\t\t\"xuid\": \"{player.GetXUID()}\"\n");
                    sb.Append("\t},\n");
                }
            }
            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("\n]");
            File.WriteAllText($@"{server.GetProp("ServerPath")}\whitelist.json", sb.ToString());
            sb = new StringBuilder();
            sb.Append("[\n");

            foreach (Player player in server.GetPlayerList())
            {
                string[] playerReg = player.GetRegistration();
                if (!player.IsDefaultRegistration() && playerReg[0] == "False")
                {
                    sb.Append("\t{\n");
                    sb.Append($"\t\t\"permission\": \"{playerReg[1]}\",\n");
                    sb.Append($"\t\t\"xuid\": \"{player.GetXUID()}\"\n");
                    sb.Append("\t},\n");
                }
            }
            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("\n]");
            File.WriteAllText($@"{server.GetProp("ServerPath")}\permissions.json", sb.ToString());
        }

        public void SaveServerProps(IServerConfiguration server, bool SaveServerInfo)
        {
            int index = 0;
            string[] output = new string[5 + server.GetAllProps().Count + server.GetStartCommands().Count];
            output[index++] = "#Server";
            foreach (Property prop in server.GetAllProps())
            {
                output[index++] = $"{prop.KeyName}={prop}";
            }
            if (!SaveServerInfo)
            {
                if (!Directory.Exists(server.GetProp("ServerPath").ToString()))
                {
                    Directory.CreateDirectory(server.GetProp("ServerPath").ToString());
                }
                File.WriteAllLines($@"{server.GetProp("ServerPath")}\server.properties", output);
            }
            else
            {
                output[index++] = string.Empty;
                output[index++] = "#StartCmds";

                foreach (StartCmdEntry startCmd in server.GetStartCommands())
                {
                    output[index++] = $"AddStartCmd={startCmd.Command}";
                }
                output[index++] = string.Empty;

                File.WriteAllLines($@"{_configDir}\{server.GetFileName()}", output);
                if (server.GetProp("ServerPath").ToString() == null)
                    server.GetProp("ServerPath").SetValue(server.GetProp("ServerPath").DefaultValue);
                if (!Directory.Exists(server.GetProp("ServerPath").ToString()))
                {
                    Directory.CreateDirectory(server.GetProp("ServerPath").ToString());
                }
                File.WriteAllLines($@"{server.GetProp("ServerPath")}\server.properties", output);
            }
        }

        public void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag)
        {
            try
            {
                File.Delete($@"{_configDir}\{serverInfo.GetFileName()}");
                switch (flag)
                {
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

            }
            catch { }
        }

        public List<Property> EnumerateBackupsForServer(byte serverIndex)
        {
            string serverName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            List<Property> newList = new List<Property>();
            try
            {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{serverName}").GetDirectories())
                {
                    string[] splitName = dir.Name.Split('_');
                    newList.Add(new Property(dir.Name, new DateTime(long.Parse(splitName[1])).ToString("G")));
                }
            }
            catch (IOException)
            {
                return newList;
            }
            return newList;
        }

        public void DeleteBackupsForServer(byte serverIndex, List<string> list)
        {
            string serverName = _serviceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            try
            {
                foreach (string deleteDir in list)
                    foreach (DirectoryInfo dir in new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{serverName}").GetDirectories())
                        if (dir.Name == deleteDir)
                        {
                            new FileUtils(_processInfo.GetDirectory()).DeleteFilesRecursively(new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{serverName}\{deleteDir}"), true);
                            _logger.AppendLine($"Deleted backup {deleteDir}.");
                        }
            }
            catch (IOException e)
            {
                _logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
        }

        private bool DeleteBackups(IServerConfiguration serverInfo)
        {
            try
            {
                string configBackupPath = "";
                DirectoryInfo backupDirInfo = new DirectoryInfo($@"{configBackupPath}\{serverInfo.GetServerName()}");
                DirectoryInfo configBackupDirInfo = new DirectoryInfo($@"{_configDir}\Backups");
                foreach (DirectoryInfo dir in backupDirInfo.GetDirectories())
                {
                    if (dir.Name.Contains($"{serverInfo.GetServerName()}"))
                    {
                        dir.Delete(true);
                    }
                }
                foreach (FileInfo file in configBackupDirInfo.GetFiles())
                {
                    if (file.Name.Contains($"{serverInfo.GetServerName()}_"))
                    {
                        file.Delete();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        private bool DeleteServerFiles(IServerConfiguration serverInfo)
        {
            try
            {
                new FileUtils(_processInfo.GetDirectory()).DeleteFilesRecursively(new DirectoryInfo(serverInfo.GetProp("ServerPath").ToString()), false);
                return true;
            }
            catch { return false; }
        }

        private bool DeletePlayerFiles(IServerConfiguration serverInfo)
        {
            try
            {
                DirectoryInfo configDirInfo = new DirectoryInfo(_configDir);
                foreach (DirectoryInfo dir in configDirInfo.GetDirectories())
                {
                    if (dir.Name == "KnownPlayers" || dir.Name == "RegisteredPlayers")
                    {
                        foreach (FileInfo file in dir.GetFiles())
                        {
                            if (file.Name.Contains($"{serverInfo.GetServerName()}"))
                            {
                                file.Delete();
                            }
                        }
                    }
                }
                return true;
            }
            catch { return false; }
        }

        public Task LoadConfiguration(IConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public Task SaveConfiguration(IConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}

