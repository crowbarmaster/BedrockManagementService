using BedrockService.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Shared.Classes
{
    public class ServerInfo : IServerConfiguration
    {
        public string serversPath;
        public string ServerName { get; set; }
        public string FileName { get; set; }
        public List<string> ConsoleBuffer = new List<string>();
        public Property ServerPath { get; set; }
        public Property ServerExeName { get; set; }
        public List<IPlayer> PlayersList = new List<IPlayer>();
        public List<Property> ServerPropList = new List<Property>();
        public List<StartCmdEntry> StartCmds = new List<StartCmdEntry>();

        public ServerInfo(string[] configEntries, string coreServersPath)
        {
            serversPath = coreServersPath;
            InitializeDefaults();
            ProcessConfiguration(configEntries);
        }

        public void InitializeDefaults()
        {
            ServerName = "Default";
            FileName = "Default.conf";
            ServerPath = new Property("ServerPath", $@"{serversPath}\{ServerName}");
            ServerExeName = new Property("ServerExeName", $"BedrockService.{ServerName}.exe");

            ServerPropList.Clear();
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


        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void ProcessConfiguration(string[] fileEntries)
        {
            if (fileEntries == null)
                return;
            foreach (string line in fileEntries)
            {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                {
                    string[] split = line.Split('=');
                    if (split.Length == 1)
                    {
                        split = new string[] { split[0], "" };
                    }
                    Property SrvProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == split[0]);
                    if (SrvProp != null)
                    {
                        SetProp(split[0], split[1]);
                    }
                    switch (split[0])
                    {
                        case "server-name":
                            ServerName = split[1];
                            ServerPath.SetValue($@"{serversPath}\{split[1]}");
                            ServerExeName.SetValue($"BedrockService.{split[1]}.exe");
                            break;

                        case "AddStartCmd":
                            StartCmds.Add(new StartCmdEntry(split[1]));
                            break;
                    }
                }
            }
        }

        public bool SetProp(string name, string newValue)
        {
            try
            {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == name);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(newValue);
                return true;
            }
            catch
            {

            }
            return false;
        }

        public bool SetProp(Property propToSet)
        {
            try
            {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == propToSet.KeyName);
                ServerPropList[ServerPropList.IndexOf(serverProp)] = propToSet;
                return true;
            }
            catch
            {

            }
            return false;
        }

        public Property GetProp(string name)
        {
            Property foundProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == name);
            if (foundProp == null)
            {
                switch (name)
                {
                    case "ServerPath":
                        return ServerPath;
                    case "ServerExeName":
                        return ServerExeName;
                }
            }
            return foundProp;
        }

        public void SetAllInfos() => throw new System.NotImplementedException();

        public void AddStartCommand(string command)
        {
            StartCmds.Add(new StartCmdEntry(command));
        }

        public bool DeleteStartCommand(string command)
        {
            StartCmdEntry entry = StartCmds.FirstOrDefault(prop => prop.Command == command);
            return StartCmds.Remove(entry);
        }

        public List<StartCmdEntry> GetStartCommands() => StartCmds;

        public override string ToString()
        {
            return ServerName;
        }

        public override int GetHashCode()
        {
            int hashCode = -298215838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ServerName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Property>.Default.GetHashCode(ServerPath);
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is ServerInfo info &&
                   ServerName == info.ServerName &&
                   EqualityComparer<Property>.Default.Equals(ServerPath, info.ServerPath);
        }

        public string GetServerName() => ServerName;

        public List<Property> GetAllProps() => ServerPropList;

        public void SetAllProps(List<Property> newPropList) => ServerPropList = newPropList;

        public void AddUpdatePlayer(IPlayer player)
        {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == player.GetXUID());
            if (foundPlayer == null)
            {
                PlayersList.Add(player);
                return;
            }
            PlayersList[PlayersList.IndexOf(foundPlayer)] = player;
        }

        public IPlayer GetPlayerByXuid(string xuid)
        {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null)
            {
                return null;
            }
            return foundPlayer;
        }

        public string GetFileName()
        {
            return FileName;
        }

        public List<string> GetLog() => ConsoleBuffer = ConsoleBuffer ?? new List<string>();

        public void SetLog(List<string> newLog) => ConsoleBuffer = newLog;

        public List<IPlayer> GetPlayerList() => PlayersList ?? new List<IPlayer>();

        public void SetPlayerList(List<IPlayer> newList) => PlayersList = newList;

        public IServerConfiguration GetServerInfo() => this;
    }

}