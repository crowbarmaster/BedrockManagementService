using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
            globalFile = $@"{configDir}\Globals.conf";
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
            string globFileResult = null;
            string serversPath = null;
            foreach (string file in files)
            {
                if (file.EndsWith("Globals.conf"))
                {
                    globFileResult = file;
                    InstanceProvider.GetServiceLogger().AppendLine("Loading Globals...");
                    InstanceProvider.GetHostInfo().SetGlobalsDefault();
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                        {
                            string[] split = line.Split('=');
                            if (split.Length == 1)
                            {
                                split[1] = "";
                            }
                            if(InstanceProvider.GetHostInfo().SetGlobalProperty(split[0], split[1]))
                            {
                                if(split[0] == "ServersPath")
                                {
                                    serversPath = split[1];
                                }
                            }
                            else
                            {
                                InstanceProvider.GetServiceLogger().AppendLine($"Error! Key \"{split[0]}\" was not found! Check configs!");
                            }


                        }
                    }
                }
            }
            if (globFileResult == null)
            {
                InstanceProvider.GetHostInfo().SetGlobalsDefault();
                InstanceProvider.GetServiceLogger().AppendLine("Globals.conf was not found. Loaded defaults and saved to file.");
                SaveGlobalFile();
            }
            InstanceProvider.GetHostInfo().ClearServerInfos();
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
                    serverInfo.ServerVersion = loadedVersion;
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
                                    InstanceProvider.GetServiceLogger().AppendLine($"Loading ServerInfo for server {split[1]}...");
                                    serverInfo.ServerName = split[1];
                                    serverInfo.ServerPath.Value = $@"{serversPath}\{split[1]}";
                                    serverInfo.ServerExeName.Value = $"BDS_{split[1]}.exe";
                                    LoadPlayerDatabase(serverInfo);
                                    LoadRegisteredPlayers(serverInfo);
                                    break;

                                case "AddStartCmd":
                                    serverInfo.StartCmds.Add(new StartCmdEntry(split[1]));
                                    break;
                            }
                        }
                    }
                    InstanceProvider.GetHostInfo().GetServerInfos().Add(serverInfo);
                }
                if (InstanceProvider.GetHostInfo().GetServerInfos().Count == 0)
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
            string[] output = new string[InstanceProvider.GetHostInfo().GetGlobals().Count + 3];
            int index = 0;
            output[index++] = "#Globals";
            output[index++] = string.Empty;
            foreach (Property prop in InstanceProvider.GetHostInfo().GetGlobals())
            {
                output[index++] = $"{prop.KeyName}={prop.Value}";
            }
            output[index++] = string.Empty;

            File.WriteAllLines(globalFile, output);
        }

        public ServerInfo LoadRegisteredPlayers(ServerInfo server)
        {
            string filePath = $@"{configDir}\{server.ServerName}.preg";
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
                InstanceProvider.GetServiceLogger().AppendLine($"Server \"{server.ServerName}\" Loaded registered player: {split[1]}");
                Player playerFound = server.KnownPlayers.FirstOrDefault(ply => ply.XUID == split[0]);
                if (playerFound == null)
                {
                    server.KnownPlayers.Add(new Player(split[0], split[1], DateTime.Now.Ticks.ToString(), "0", "0", split[3].ToLower() == "true", split[2], split[4].ToLower() == "true", true));
                    continue;
                }
                InstanceProvider.GetPlayerManager().UpdatePlayerFromCfg(split[0], split[1], split[2], split[3], split[4], server);
            }
            return server;
        }

        public ServerInfo LoadPlayerDatabase(ServerInfo server)
        {
            string filePath = $@"{configDir}\{server.ServerName}.playerdb";
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
                InstanceProvider.GetServiceLogger().AppendLine($"Server \"{server.ServerName}\" Loaded known player: {split[1]}");
                server.KnownPlayers.Add(new Player(split[0], split[1], split[2], split[3], split[4], server.GetServerProp("default-player-permission-level").Value));
            }
            return server;
        }

        public void SaveKnownPlayerDatabase(ServerInfo server)
        {
            lock (FileLock)
            {
                string filePath = $@"{configDir}\{server.ServerName}.playerdb";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{configDir}\Backups\{server.ServerName}_{DateTime.Now.ToString("mmddyyhhmmssff")}.dbbak", true);
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
                string filePath = $@"{configDir}\{server.ServerName}.preg";
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $@"{configDir}\Backups\{server.ServerName}_{DateTime.Now.ToString("mmddyyhhmmssff")}.bak", true);
                }
                TextWriter writer = new StreamWriter(filePath);
                writer.WriteLine("# Registered player list");
                writer.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
                writer.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
                writer.WriteLine("");
                foreach (Player entry in server.KnownPlayers)
                {
                    if(entry.Whitelisted || entry.PermissionLevel != server.GetServerProp("default-player-permission-level").Value)
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

            File.WriteAllText($@"{server.ServerPath.Value}\whitelist.json", sb.ToString());

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
            File.WriteAllText($@"{server.ServerPath.Value}\permissions.json", sb.ToString());
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
                File.WriteAllLines($@"{server.ServerPath.Value}\server.properties", output);
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
                if (server.ServerPath.Value == null)
                    server.ServerPath.Value = server.ServerPath.DefaultValue;
                if (!Directory.Exists(server.ServerPath.Value))
                {
                    Directory.CreateDirectory(server.ServerPath.Value);
                }
                File.WriteAllLines($@"{server.ServerPath.Value}\server.properties", output);
            }
        }
    }
}

