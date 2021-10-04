using BedrockService.Service.Networking;
using BedrockService.Service.Server;
using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BedrockService.Service.Management
{
    public class ConfigManager
    {
        private string configDir = $@"{Program.ServiceDirectory}\Server\Configs"; // Get Executable directory for the root.
        private string globalFile;
        private string loadedVersion;
        private static readonly object FileLock = new object();

        public ConfigManager()
        {
            globalFile = $@"{Program.ServiceDirectory}\Service\Globals.conf";
            if (!Directory.Exists($@"{configDir}\KnownPlayers\Backups"))
                Directory.CreateDirectory($@"{configDir}\KnownPlayers\Backups");
            if (!Directory.Exists($@"{configDir}\RegisteredPlayers\Backups"))
                Directory.CreateDirectory($@"{configDir}\RegisteredPlayers\Backups");
            if (!Directory.Exists($@"{configDir}\Backups"))
                Directory.CreateDirectory($@"{configDir}\Backups");
        }

        public bool LoadConfigs()
        {
            bool loading = true;
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            if (File.Exists($@"{configDir}\..\bedrock_ver.ini"))
                loadedVersion = File.ReadAllText($@"{configDir}\..\bedrock_ver.ini");

            string[] files = Directory.GetFiles(configDir, "*.conf");
            string serversPath = null;
            if (!File.Exists(globalFile))
            {
                InstanceProvider.HostInfo.SetGlobalsDefault();
                InstanceProvider.ServiceLogger.AppendLine("Globals.conf was not found. Loaded defaults and saved to file.");
                SaveGlobalFile();
            }
            else
            {
                InstanceProvider.ServiceLogger.AppendLine("Loading Globals...");
                InstanceProvider.HostInfo.SetGlobalsDefault();
                string[] lines = File.ReadAllLines(globalFile);
                foreach (string line in lines)
                {
                    if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                    {
                        string[] split = line.Split('=');
                        if (split.Length == 1)
                        {
                            split[1] = "";
                        }
                        if (split[0] == "BackupPath")
                        {
                            if (split[1] == "Default")
                                split[1] = $@"{Program.ServiceDirectory}\Server\Backups";
                        }
                        if (InstanceProvider.HostInfo.SetGlobalProperty(split[0], split[1]))
                        {
                            if (split[0] == "ServersPath")
                            {
                                serversPath = split[1];
                            }
                        }
                        else
                        {
                            InstanceProvider.ServiceLogger.AppendLine($"Error! Key \"{split[0]}\" was not found! Check configs!");
                        }


                    }
                }
            }
            InstanceProvider.HostInfo.ClearServerInfos();
            ServerInfo serverInfo = new ServerInfo();
            serverInfo.InitDefaults(serversPath);
            while (loading)
            {
                foreach (string file in files)
                {
                    FileInfo FInfo = new FileInfo(file);
                    if (FInfo.Name == "Globals.conf")
                        continue;
                    serverInfo = new ServerInfo();
                    serverInfo.InitDefaults(serversPath);
                    serverInfo.FileName = FInfo.Name;
                    InstanceProvider.HostInfo.ServerVersion = loadedVersion;
                    string[] linesArray = File.ReadAllLines(file);
                    foreach (string line in linesArray)
                    {
                        if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                        {
                            string[] split = line.Split('=');
                            if (split.Length == 1)
                            {
                                split = new string[] { split[0], "" };
                            }
                            Property SrvProp = serverInfo.ServerPropList.FirstOrDefault(prop => prop.KeyName == split[0]);
                            if (SrvProp != null)
                            {
                                serverInfo.SetServerProp(split[0], split[1]);
                            }
                            switch (split[0])
                            {
                                case "server-name":
                                    InstanceProvider.ServiceLogger.AppendLine($"Loading ServerInfo for server {split[1]}...");
                                    serverInfo.ServerName = split[1];
                                    serverInfo.ServerPath.Value = $@"{serversPath}\{split[1]}";
                                    serverInfo.ServerExeName.Value = $"BedrockService.{split[1]}.exe";
                                    LoadPlayerDatabase(serverInfo);
                                    LoadRegisteredPlayers(serverInfo);
                                    break;

                                case "AddStartCmd":
                                    serverInfo.StartCmds.Add(new StartCmdEntry(split[1]));
                                    break;
                            }
                        }
                    }
                    InstanceProvider.HostInfo.GetServerInfos().Add(serverInfo);
                }
                if (InstanceProvider.HostInfo.GetServerInfos().Count == 0)
                {
                    SaveServerProps(serverInfo, true);
                    files = Directory.GetFiles(configDir, "*.conf");
                }
                else
                {
                    loading = false;
                }
            }
            return true;
        }

        public void SaveGlobalFile()
        {
            string[] output = new string[InstanceProvider.HostInfo.GetGlobals().Count + 3];
            int index = 0;
            output[index++] = "#Globals";
            output[index++] = string.Empty;
            foreach (Property prop in InstanceProvider.HostInfo.GetGlobals())
            {
                output[index++] = $"{prop.KeyName}={prop.Value}";
            }
            output[index++] = string.Empty;

            File.WriteAllLines(globalFile, output);
        }

        public ServerInfo LoadRegisteredPlayers(ServerInfo server)
        {
            string filePath = $@"{configDir}\RegisteredPlayers\{server.ServerName}.preg";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return server;
            }
            foreach (string entry in File.ReadAllLines(filePath))
            {
                if (entry.StartsWith("#") || string.IsNullOrWhiteSpace(entry))
                    continue;
                string[] split = entry.Split(',');
                InstanceProvider.ServiceLogger.AppendLine($"Server \"{server.ServerName}\" Loaded registered player: {split[1]}");
                Player playerFound = server.KnownPlayers.FirstOrDefault(ply => ply.XUID == split[0]);
                if (playerFound == null)
                {
                    server.KnownPlayers.Add(new Player(split[0], split[1], DateTime.Now.Ticks.ToString(), "0", "0", split[3].ToLower() == "true", split[2], split[4].ToLower() == "true", true));
                    continue;
                }
                InstanceProvider.PlayerManager.UpdatePlayerFromCfg(split[0], split[1], split[2], split[3], split[4], server);
            }
            return server;
        }

        public ServerInfo LoadPlayerDatabase(ServerInfo server)
        {
            string filePath = $@"{configDir}\KnownPlayers\{server.ServerName}.playerdb";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return server;
            }
            foreach (string entry in File.ReadAllLines(filePath))
            {
                if (entry.StartsWith("#") || string.IsNullOrWhiteSpace(entry))
                    continue;
                string[] split = entry.Split(',');
                InstanceProvider.ServiceLogger.AppendLine($"Server \"{server.ServerName}\" Loaded known player: {split[1]}");
                server.KnownPlayers.Add(new Player(split[0], split[1], split[2], split[3], split[4], server.GetServerProp("default-player-permission-level").Value));
            }
            return server;
        }

        public void SaveKnownPlayerDatabase(ServerInfo server)
        {
            lock (FileLock)
            {
                string filePath = $@"{configDir}\KnownPlayers\{server.ServerName}.playerdb";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{configDir}\KnownPlayers\Backups\{server.ServerName}_{DateTime.Now.ToString("mmddyyhhmmssff")}.dbbak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                foreach (Player entry in server.KnownPlayers)
                {
                    writer.WriteLine($"{entry.XUID},{entry.Username},{entry.FirstConnectedTime},{entry.LastConnectedTime},{entry.LastDisconnectTime}");
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void SaveRegisteredPlayers(ServerInfo server)
        {
            lock (FileLock)
            {
                string filePath = $@"{configDir}\RegisteredPlayers\{server.ServerName}.preg";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{configDir}\RegisteredPlayers\Backups\{server.ServerName}_{DateTime.Now.ToString("mmddyyhhmmssff")}.bak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                writer.WriteLine("# Registered player list");
                writer.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
                writer.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
                writer.WriteLine("");
                foreach (Player entry in server.KnownPlayers)
                {
                    if (entry.Whitelisted || entry.PermissionLevel != server.GetServerProp("default-player-permission-level").Value)
                        writer.WriteLine($"{entry.XUID},{entry.Username},{entry.PermissionLevel},{entry.Whitelisted},{entry.IgnorePlayerLimits}");
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void WriteJSONFiles(ServerInfo server)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[\n");

            foreach (Player player in server.KnownPlayers)
            {
                if (player.FromConfig && player.Whitelisted)
                {
                    sb.Append("\t{\n");
                    sb.Append($"\t\t\"ignoresPlayerLimit\": {player.IgnorePlayerLimits.ToString().ToLower()},\n");
                    sb.Append($"\t\t\"name\": \"{player.Username}\",\n");
                    sb.Append($"\t\t\"xuid\": \"{player.XUID}\"\n");
                    sb.Append("\t},\n");
                }
            }
            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("\n]");

            File.WriteAllText($@"{server.ServerPath}\whitelist.json", sb.ToString());

            sb = new StringBuilder();
            sb.Append("[\n");

            foreach (Player player in server.KnownPlayers)
            {
                if (player.FromConfig && server.GetServerProp("default-player-permission-level").Value != player.PermissionLevel)
                {
                    sb.Append("\t{\n");
                    sb.Append($"\t\t\"permission\": \"{player.PermissionLevel}\",\n");
                    sb.Append($"\t\t\"xuid\": \"{player.XUID}\"\n");
                    sb.Append("\t},\n");
                }
            }

            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("\n]");
            File.WriteAllText($@"{server.ServerPath}\permissions.json", sb.ToString());
        }

        public void SaveServerProps(ServerInfo server, bool SaveServerInfo)
        {
            int index = 0;
            string[] output = new string[5 + server.ServerPropList.Count + server.StartCmds.Count];
            output[index++] = "#Server";
            foreach (Property prop in server.ServerPropList)
            {
                output[index++] = $"{prop.KeyName}={prop.Value}";
            }
            if (!SaveServerInfo)
            {
                if (!Directory.Exists(server.ServerPath.Value))
                {
                    Directory.CreateDirectory(server.ServerPath.Value);
                }
                File.WriteAllLines($@"{server.ServerPath}\server.properties", output);
            }
            else
            {
                output[index++] = string.Empty;
                output[index++] = "#StartCmds";

                foreach (StartCmdEntry startCmd in server.StartCmds)
                {
                    output[index++] = $"AddStartCmd={startCmd.Command}";
                }
                output[index++] = string.Empty;

                File.WriteAllLines($@"{configDir}\{server.FileName}", output);
                if (server.ServerPath == null)
                    server.ServerPath.Value = server.ServerPath.DefaultValue;
                if (!Directory.Exists(server.ServerPath.Value))
                {
                    Directory.CreateDirectory(server.ServerPath.Value);
                }
                File.WriteAllLines($@"{server.ServerPath}\server.properties", output);
            }
        }

        public void RemoveServerConfigs(ServerInfo serverInfo, NetworkMessageFlags flag)
        {
            try
            {
                File.Delete($@"{configDir}\{serverInfo.FileName}");
                switch (flag)
                {
                    case NetworkMessageFlags.RemoveBckPly:
                        if (DeleteBackups(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Backups for server {serverInfo.ServerName}");
                        if (DeletePlayerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Player files for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.RemoveBckSrv:
                        if (DeleteBackups(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Backups for server {serverInfo.ServerName}");
                        if (DeleteServerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted server directory for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.RemovePlySrv:
                        if (DeletePlayerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Player files for server {serverInfo.ServerName}");
                        if (DeleteServerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted server directory for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.RemoveSrv:
                        if (DeleteServerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted server directory for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.RemovePlayers:
                        if (DeletePlayerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Player files for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.RemoveBackups:
                        if (DeleteBackups(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Backups for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.RemoveAll:
                        if (DeleteBackups(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Backups for server {serverInfo.ServerName}");
                        if (DeletePlayerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted Player files for server {serverInfo.ServerName}");
                        if (DeleteServerFiles(serverInfo))
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted server directory for server {serverInfo.ServerName}");
                        break;
                    case NetworkMessageFlags.None:
                        break;
                }
                BedrockServer removeMe = InstanceProvider.BedrockService.bedrockServers.First(brs => brs.serverInfo == serverInfo);
                InstanceProvider.BedrockService.bedrockServers.Remove(removeMe);

            }
            catch { }
        }

        public List<Property> EnumerateBackupsForServer(byte serverIndex)
        {
            string serverName = InstanceProvider.GetServerInfoByIndex(serverIndex).ServerName;
            List<Property> newList = new List<Property>();
            try
            {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{InstanceProvider.HostInfo.GetGlobalValue("BackupPath")}\{serverName}").GetDirectories())
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
            string serverName = InstanceProvider.GetServerInfoByIndex(serverIndex).ServerName;
            try
            {
                foreach (string deleteDir in list)
                    foreach (DirectoryInfo dir in new DirectoryInfo($@"{InstanceProvider.HostInfo.GetGlobalValue("BackupPath")}\{serverName}").GetDirectories())
                        if (dir.Name == deleteDir)
                        {
                            Directory.Delete($@"{InstanceProvider.HostInfo.GetGlobalValue("BackupPath")}\{serverName}\{deleteDir}", true);
                            InstanceProvider.ServiceLogger.AppendLine($"Deleted backup {deleteDir}.");
                        }
            }
            catch (IOException e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
        }

        public bool RollbackToBackup(byte serverIndex, string folderName)
        {
            ServerInfo server = InstanceProvider.GetServerInfoByIndex(serverIndex);
            BedrockServer brs = InstanceProvider.GetBedrockServerByIndex(serverIndex);
            brs.CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
            while (brs.CurrentServerStatus != BedrockServer.ServerStatus.Stopped)
            {
                Thread.Sleep(100);
            }
            try
            {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{InstanceProvider.HostInfo.GetGlobalValue("BackupPath")}\{server.ServerName}").GetDirectories())
                    if (dir.Name == folderName)
                    {
                        DeleteFilesRecursively(new DirectoryInfo($@"{server.ServerPath.Value}\worlds"));
                        InstanceProvider.ServiceLogger.AppendLine($"Deleted world folder contents.");
                        foreach (DirectoryInfo worldDir in new DirectoryInfo($@"{InstanceProvider.HostInfo.GetGlobalValue("BackupPath")}\{server.ServerName}\{folderName}").GetDirectories())
                        {
                            CopyFilesRecursively(worldDir, new DirectoryInfo($@"{server.ServerPath.Value}\worlds\{worldDir.Name}"));
                            InstanceProvider.ServiceLogger.AppendLine($@"Copied {worldDir.Name} to path {server.ServerPath.Value}\worlds");
                        }
                        brs.CurrentServerStatus = BedrockServer.ServerStatus.Starting;
                        return true;
                    }
            }
            catch (IOException e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
        }


        private void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        private void DeleteFilesRecursively(DirectoryInfo source)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                DeleteFilesRecursively(dir);
            foreach (FileInfo file in source.GetFiles())
                file.Delete();
            foreach (DirectoryInfo emptyDir in source.GetDirectories())
                emptyDir.Delete(true);
        }

        private bool DeleteBackups(ServerInfo serverInfo)
        {
            try
            {

                string configBackupPath = InstanceProvider.HostInfo.GetGlobalValue("BackupPath");
                DirectoryInfo backupDirInfo = new DirectoryInfo($@"{configBackupPath}\{serverInfo.ServerName}");
                DirectoryInfo configDirInfo = new DirectoryInfo($@"{configDir}\Backups");
                foreach (DirectoryInfo dir in backupDirInfo.GetDirectories())
                {
                    if (dir.Name.Contains($"{serverInfo.ServerName}"))
                    {
                        dir.Delete(true);
                    }
                }
                foreach (FileInfo file in configDirInfo.GetFiles())
                {
                    if (file.Name.Contains($"{serverInfo.ServerName}_"))
                    {
                        file.Delete();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        private bool DeleteServerFiles(ServerInfo serverInfo)
        {
            try
            {
                DirectoryInfo serverDirInfo = new DirectoryInfo(serverInfo.ServerPath.Value);
                serverDirInfo.Delete(true);
                return true;
            }
            catch { return false; }
        }

        private bool DeletePlayerFiles(ServerInfo serverInfo)
        {
            try
            {
                DirectoryInfo configDirInfo = new DirectoryInfo(configDir);
                foreach (DirectoryInfo dir in configDirInfo.GetDirectories())
                {
                    if (dir.Name == "KnownPlayers" || dir.Name == "RegisteredPlayers")
                    {
                        foreach (FileInfo file in dir.GetFiles())
                        {
                            if (file.Name.Contains($"{serverInfo.ServerName}"))
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
    }
}

