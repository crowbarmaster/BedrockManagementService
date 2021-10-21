using BedrockService.Service.Server;
using BedrockService.Shared.Utilities;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BedrockService.Shared.Classes;

namespace BedrockService.Service.Management
{
    public class ConfigManager : IConfigurator
    {
        private string configDir = "";
        private string globalFile;
        private string loadedVersion;
        private static readonly object FileLock = new object();
        private readonly IServiceConfiguration ServiceConfiguration;
        private readonly IProcessInfo ProcessInfo;
        private readonly ILogger Logger;

        public ConfigManager(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, ILogger logger)
        {
            ProcessInfo = processInfo;
            ServiceConfiguration = serviceConfiguration;
            Logger = logger;
            configDir = $@"{ProcessInfo.GetDirectory()}\Server\Configs";
            globalFile = $@"{ProcessInfo.GetDirectory()}\Service\Globals.conf";
        }

        public async Task LoadAllConfigurations()
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);
                if (!Directory.Exists($@"{configDir}\KnownPlayers\Backups"))
                    Directory.CreateDirectory($@"{configDir}\KnownPlayers\Backups");
                if (!Directory.Exists($@"{configDir}\RegisteredPlayers\Backups"))
                    Directory.CreateDirectory($@"{configDir}\RegisteredPlayers\Backups");
                if (!Directory.Exists($@"{configDir}\Backups"))
                    Directory.CreateDirectory($@"{configDir}\Backups");
                if (File.Exists($@"{configDir}\..\bedrock_ver.ini"))
                    loadedVersion = File.ReadAllText($@"{configDir}\..\bedrock_ver.ini");
                bool loading = true;
                ServerInfo serverInfo;
                while (loading)
                {
                    ServiceConfiguration.ProcessConfiguration(File.ReadAllLines(globalFile));
                    string[] files = Directory.GetFiles(configDir, "*.conf");
                    foreach (string file in files)
                    {
                        FileInfo FInfo = new FileInfo(file);
                        if (FInfo.Name == "Globals.conf")
                            continue;
                        string[] fileEntries = File.ReadAllLines(file);
                        serverInfo = new ServerInfo(fileEntries);
                        ServiceConfiguration.AddNewServerInfo(serverInfo);
                    }
                    if (ServiceConfiguration.GetAllServerInfos().Count != 0)
                    {
                        break;
                    }

                    serverInfo = new ServerInfo(null);
                    serverInfo.InitializeDefaults();
                    SaveServerProps(serverInfo, true);
                }
                return;
            });
        }

        public void SaveGlobalFile()
        {
            string[] output = new string[ServiceConfiguration.GetAllProps().Count + 3];
            int index = 0;
            output[index++] = "#Globals";
            output[index++] = string.Empty;
            foreach (Property prop in ServiceConfiguration.GetAllProps())
            {
                output[index++] = $"{prop.KeyName}={prop}";
            }
            output[index++] = string.Empty;

            File.WriteAllLines(globalFile, output);
        }

        public void LoadRegisteredPlayers(IBedrockServer server)
        {
            string serverName = server.GetServerName();
            string filePath = $@"{configDir}\RegisteredPlayers\{serverName}.preg";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return;
            }
            server.GetPlayerManager().ProcessConfiguration(File.ReadAllLines($@"{configDir}\RegisteredPlayers\{serverName}.preg"));
        }

        private void LoadGlobals()
        {
            ServiceConfiguration.InitializeDefaults();
            if (File.Exists(globalFile))
            {
                Logger.AppendLine("Loading Globals...");
                string[] fileEntries = File.ReadAllLines(globalFile);
                IServiceConfiguration serviceConfig = ServiceConfiguration;
                serviceConfig.ProcessConfiguration(fileEntries);
                return;
            }
            ServiceConfiguration.InitializeDefaults();
            Logger.AppendLine("Globals.conf was not found. Loaded defaults and saved to file.");
            SaveGlobalFile();
        }

        public async Task ReplaceServerBuild(IServerConfiguration server)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(server.GetProp("ServerPath").ToString()))
                        Directory.CreateDirectory(server.GetProp("ServerPath").ToString());
                    else if (File.Exists($@"{ProcessInfo.GetDirectory()}\Server\MCSFiles\stock_filelist.ini"))
                    {
                        new FileUtils(ProcessInfo.GetDirectory()).DeleteFilelist(File.ReadAllLines($@"{ProcessInfo.GetDirectory()}\Server\MCSFiles\stock_filelist.ini"), server.GetProp("ServerPath").ToString());
                    }
                    else
                    {
                        new FileUtils(ProcessInfo.GetDirectory()).DeleteFilesRecursively(new DirectoryInfo(server.GetProp("ServerPath").ToString()), false);
                    }
                    while (ServiceConfiguration.GetServerVersion() == null || ServiceConfiguration.GetServerVersion() == "None")
                    {
                        Thread.Sleep(150);
                    }
                    ZipFile.ExtractToDirectory($@"{ProcessInfo.GetDirectory()}\Server\MCSFiles\Update_{ServiceConfiguration.GetServerVersion()}.zip", ProcessInfo.GetDirectory());
                    File.Copy(server.GetProp("ServerPath").ToString() + "\\bedrock_server.exe", server.GetProp("ServerPath").ToString() + "\\" + server.GetProp("ServerExeName").ToString(), true);
                }
                catch (Exception e)
                {
                    Logger.AppendLine($"ERROR: Got an exception deleting entire directory! {e.Message}");
                }
            });
        }

        public void LoadPlayerDatabase(IServerConfiguration server)
        {
            string filePath = $@"{configDir}\KnownPlayers\{server.GetServerName()}.playerdb";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return;
            }
        }

        public void SaveKnownPlayerDatabase(IServerConfiguration server)
        {
            lock (FileLock)
            {
                string filePath = $@"{configDir}\KnownPlayers\{server.GetServerName()}.playerdb";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{configDir}\KnownPlayers\Backups\{server.GetServerName()}_{DateTime.Now:mmddyyhhmmssff}.dbbak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                foreach (Player entry in server.GetPlayerList())
                {
                    writer.WriteLine(entry.ToString("K"));
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void SaveRegisteredPlayers(IServerConfiguration server)
        {
            lock (FileLock)
            {
                string filePath = $@"{configDir}\RegisteredPlayers\{server.GetServerName()}.preg";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{configDir}\RegisteredPlayers\Backups\{server.GetServerName()}_{DateTime.Now:mmddyyhhmmssff}.bak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                writer.WriteLine("# Registered player list");
                writer.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
                writer.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
                writer.WriteLine("");
                foreach (IPlayer player in server.GetPlayerList())
                {
                    if (!player.IsDefaultRegistration())
                        writer.WriteLine(player.ToString("R"));
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

                File.WriteAllLines($@"{configDir}\{server.GetFileName()}", output);
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
                File.Delete($@"{configDir}\{serverInfo.GetFileName()}");
                switch (flag)
                {
                    case NetworkMessageFlags.RemoveBckPly:
                        if (DeleteBackups(serverInfo))
                            Logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeletePlayerFiles(serverInfo))
                            Logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveBckSrv:
                        if (DeleteBackups(serverInfo))
                            Logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeleteServerFiles(serverInfo))
                            Logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemovePlySrv:
                        if (DeletePlayerFiles(serverInfo))
                            Logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        if (DeleteServerFiles(serverInfo))
                            Logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveSrv:
                        if (DeleteServerFiles(serverInfo))
                            Logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemovePlayers:
                        if (DeletePlayerFiles(serverInfo))
                            Logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveBackups:
                        if (DeleteBackups(serverInfo))
                            Logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.RemoveAll:
                        if (DeleteBackups(serverInfo))
                            Logger.AppendLine($"Deleted Backups for server {serverInfo.GetServerName()}");
                        if (DeletePlayerFiles(serverInfo))
                            Logger.AppendLine($"Deleted Player files for server {serverInfo.GetServerName()}");
                        if (DeleteServerFiles(serverInfo))
                            Logger.AppendLine($"Deleted server directory for server {serverInfo.GetServerName()}");
                        break;
                    case NetworkMessageFlags.None:
                        break;
                }
                ServiceConfiguration.RemoveServerInfo(serverInfo);

            }
            catch { }
        }

        public List<Property> EnumerateBackupsForServer(byte serverIndex)
        {
            string serverName = ServiceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            List<Property> newList = new List<Property>();
            try
            {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{ServiceConfiguration.GetProp("BackupPath")}\{serverName}").GetDirectories())
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
            string serverName = ServiceConfiguration.GetServerInfoByIndex(serverIndex).GetServerName();
            try
            {
                foreach (string deleteDir in list)
                    foreach (DirectoryInfo dir in new DirectoryInfo($@"{ServiceConfiguration.GetProp("BackupPath")}\{serverName}").GetDirectories())
                        if (dir.Name == deleteDir)
                        {
                            new FileUtils(ProcessInfo.GetDirectory()).DeleteFilesRecursively(new DirectoryInfo($@"{ServiceConfiguration.GetProp("BackupPath")}\{serverName}\{deleteDir}"), true);
                            Logger.AppendLine($"Deleted backup {deleteDir}.");
                        }
            }
            catch (IOException e)
            {
                Logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
        }

        public bool RollbackToBackup(byte serverIndex, string folderName)
        {
            return false;
            /*
            IServerConfiguration server = ServiceConfiguration.GetServerInfoByIndex(serverIndex);
            IBedrockServer brs = BedrockService.GetBedrockServerByIndex(serverIndex);
            brs.SetServerStatus(BedrockServer.ServerStatus.Stopping);
            while (brs.GetServerStatus() != BedrockServer.ServerStatus.Stopped)
            {
                Thread.Sleep(100);
            }
            try
            {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{ServiceConfiguration.GetProp("BackupPath")}\{server.GetServerName()}").GetDirectories())
                    if (dir.Name == folderName)
                    {
                        new FileUtils(ProcessInfo.GetDirectory()).DeleteFilesRecursively(new DirectoryInfo($@"{server.GetProp("ServerPath")}\worlds"), false);
                        Logger.AppendLine($"Deleted world folder contents.");
                        foreach (DirectoryInfo worldDir in new DirectoryInfo($@"{ServiceConfiguration.GetProp("BackupPath")}\{server.GetServerName()}\{folderName}").GetDirectories())
                        {
                            new FileUtils(ProcessInfo.GetDirectory()).CopyFilesRecursively(worldDir, new DirectoryInfo($@"{server.GetProp("ServerPath")}\worlds\{worldDir.Name}"));
                            Logger.AppendLine($@"Copied {worldDir.Name} to path {server.GetProp("ServerPath")}\worlds");
                        }
                        brs.SetServerStatus(BedrockServer.ServerStatus.Starting);
                        return true;
                    }
            }
            catch (IOException e)
            {
                Logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
            */
        }

        private bool DeleteBackups(IServerConfiguration serverInfo)
        {
            try
            {
                string configBackupPath = "";
                DirectoryInfo backupDirInfo = new DirectoryInfo($@"{configBackupPath}\{serverInfo.GetServerName()}");
                DirectoryInfo configBackupDirInfo = new DirectoryInfo($@"{configDir}\Backups");
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
                new FileUtils(ProcessInfo.GetDirectory()).DeleteFilesRecursively(new DirectoryInfo(serverInfo.GetProp("ServerPath").ToString()), false);
                return true;
            }
            catch { return false; }
        }

        private bool DeletePlayerFiles(IServerConfiguration serverInfo)
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

