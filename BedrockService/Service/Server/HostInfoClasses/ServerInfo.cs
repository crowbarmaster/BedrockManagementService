using BedrockService.Service.Server.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Service.Server.HostInfoClasses
{
    public class ServerInfo
    {
        public bool Primary { get; set; }
        public string ServerName { get; set; }
        public string ServerVersion { get; set; }
        public string FileName { get; set; }
        public ServerLogger ConsoleBuffer { get; set; }
        public Property ServerPath { get; set; }
        public Property ServerExeName { get; set; }
        public Property BackupPath { get; set; }
        public Property MaxBackupCount { get; set; }
        public Property AdvancedBackup { get; set; }
        public Property LogToFileEnabled { get; set; }
        public List<Player> KnownPlayers = new List<Player>();
        public List<Property> ServerPropList = new List<Property>();
        public List<StartCmdEntry> StartCmds = new List<StartCmdEntry>();

        public enum PermissionLevels
        {
            Visitor,
            Member,
            Operator
        }

        public void InitDefaults()
        {
            ServerName = "Default Server";
            FileName = "Default.conf";
            ServerPath = new Property("ServerPath", @"C:\Program Files (x86)\Minecraft Bedrock Server Launcher\Servers\Server");
            ServerExeName = new Property("ServerExeName", "bedrock_server.exe");
            BackupPath = new Property("BackupPath", "Default");
            MaxBackupCount = new Property("MaxBackupCount", "10");
            AdvancedBackup = new Property("AdvancedBackup", "false");
            LogToFileEnabled = new Property("LogToFile", "false");

            ServerPropList.Add(new Property("server-name", "Default"));
            ServerPropList.Add(new Property("gamemode", "creative"));
            ServerPropList.Add(new Property("difficulty", "easy"));
            ServerPropList.Add(new Property("allow-cheats", "false"));
            ServerPropList.Add(new Property("max-players", "10"));
            ServerPropList.Add(new Property("online-mode", "true"));
            ServerPropList.Add(new Property("white-list", "false"));
            ServerPropList.Add(new Property("server-port", "19132"));
            ServerPropList.Add(new Property("server-portv6", "19133"));
            ServerPropList.Add(new Property("view-distance", "32"));
            ServerPropList.Add(new Property("tick-distance", "4"));
            ServerPropList.Add(new Property("player-idle-timeout", "30"));
            ServerPropList.Add(new Property("max-threads", "8"));
            ServerPropList.Add(new Property("level-name", "Bedrock Level"));
            ServerPropList.Add(new Property("level-seed", ""));
            ServerPropList.Add(new Property("default-player-permission-level", "member"));
            ServerPropList.Add(new Property("texturepack-required", "false"));
            ServerPropList.Add(new Property("content-log-file-enabled", "false"));
            ServerPropList.Add(new Property("compression-threshold", "1"));
            ServerPropList.Add(new Property("server-authoritative-movement", "server-auth"));
            ServerPropList.Add(new Property("player-movement-score-threshold", "20"));
            ServerPropList.Add(new Property("player-movement-distance-threshold", "0.3"));
            ServerPropList.Add(new Property("player-movement-duration-threshold-in-ms", "500"));
            ServerPropList.Add(new Property("correct-player-movement", "false"));

        }

        public void SetServerProp(string name, string newValue)
        {
            Property serverProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == name);
            ServerPropList[ServerPropList.IndexOf(serverProp)].Value = newValue;
        }

        public void SetServerPropDefault(string name)
        {
            Property serverProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == name);
            ServerPropList[ServerPropList.IndexOf(serverProp)].Value = serverProp.DefaultValue;
        }

        public Property GetServerProp(string name)
        {
            return ServerPropList.FirstOrDefault(prop => prop.KeyName == name);
        }

        public void AddStartCmdEntry(string command)
        {
            StartCmds.Add(new StartCmdEntry(command));
        }

        public bool DeleteStartCmdEntry(string command)
        {
            StartCmdEntry entry = StartCmds.FirstOrDefault(prop => prop.Command == command);
            return StartCmds.Remove(entry);
        }

        public override string ToString()
        {
            return ServerName;
        }

    }

}