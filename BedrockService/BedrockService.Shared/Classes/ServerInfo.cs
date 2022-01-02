using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class ServerInfo : IServerConfiguration {
        public string ServersPath;
        public string DefaultPropFilePath;
        public string ServerName { get; set; }
        public string FileName { get; set; }
        public List<LogEntry> ConsoleBuffer = new List<LogEntry>();
        public Property ServerPath { get; set; }
        public Property ServerExeName { get; set; }
        public List<IPlayer> PlayersList = new List<IPlayer>();
        public List<Property> ServerPropList = new List<Property>();
        private List<Property> _defaultPropList = new List<Property>();
        public List<StartCmdEntry> StartCmds = new List<StartCmdEntry>();

        public ServerInfo(string coreServersPath, List<Property> defaultPropList) {
            _defaultPropList = defaultPropList;
            ServersPath = coreServersPath;
        }

        public bool InitializeDefaults() {
            ServerName = "Dedicated Server";
            FileName = "Dedicated Server.conf";
            ServerPath = new Property("ServerPath", $@"{ServersPath}\{ServerName}");
            ServerExeName = new Property("ServerExeName", $"BedrockService.{ServerName}.exe");
            _defaultPropList.ForEach(p =>  ServerPropList.Add(new Property(p.KeyName, p.DefaultValue)));
            return true;
        }


        public void SetStartCommands(List<StartCmdEntry> newEntries) => StartCmds = newEntries;

        public void ProcessConfiguration(string[] fileEntries) {
            if (fileEntries == null)
                return;
            foreach (string line in fileEntries) {
                if (!line.StartsWith("#") && !string.IsNullOrEmpty(line)) {
                    string[] split = line.Split('=');
                    if (split.Length == 1) {
                        split = new string[] { split[0], "" };
                    }
                    Property SrvProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == split[0]);
                    if (SrvProp != null) {
                        SetProp(split[0], split[1]);
                    }
                    switch (split[0]) {
                        case "server-name":
                            ServerName = split[1];
                            ServerPath.SetValue($@"{ServersPath}\{ServerName}");
                            ServerExeName.SetValue($"BedrockService.{ServerName}.exe");
                            FileName = $@"{ServerName}.conf";
                            break;

                        case "AddStartCmd":
                            StartCmds.Add(new StartCmdEntry(split[1]));
                            break;
                    }
                }
            }
        }

        public bool SetProp(string name, string newValue) {
            try {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == name);
                ServerPropList[ServerPropList.IndexOf(serverProp)].SetValue(newValue);
                return true;
            }
            catch {

            }
            return false;
        }

        public bool SetProp(Property propToSet) {
            try {
                Property serverProp = ServerPropList.First(prop => prop.KeyName == propToSet.KeyName);
                ServerPropList[ServerPropList.IndexOf(serverProp)] = propToSet;
                return true;
            }
            catch {

            }
            return false;
        }

        public Property GetProp(string name) {
            Property foundProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == name);
            if (foundProp == null) {
                switch (name) {
                    case "ServerPath":
                        return ServerPath;
                    case "ServerExeName":
                        return ServerExeName;
                }
            }
            return foundProp;
        }

        public void SetAllInfos() => throw new System.NotImplementedException();

        public void AddStartCommand(string command) {
            StartCmds.Add(new StartCmdEntry(command));
        }

        public bool DeleteStartCommand(string command) {
            StartCmdEntry entry = StartCmds.FirstOrDefault(prop => prop.Command == command);
            return StartCmds.Remove(entry);
        }

        public List<StartCmdEntry> GetStartCommands() => StartCmds;

        public override string ToString() {
            return ServerName;
        }

        public override int GetHashCode() {
            int hashCode = -298215838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ServerName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + EqualityComparer<Property>.Default.GetHashCode(ServerPath);
            return hashCode;
        }

        public override bool Equals(object obj) {
            return obj is ServerInfo info &&
                   ServerName == info.ServerName &&
                   EqualityComparer<Property>.Default.Equals(ServerPath, info.ServerPath);
        }

        public string GetServerName() => ServerName;

        public List<Property> GetAllProps() => ServerPropList;

        public void SetAllProps(List<Property> newPropList) => ServerPropList = newPropList;

        public void AddUpdatePlayer(IPlayer player) {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == player.GetXUID());
            if (foundPlayer == null) {
                PlayersList.Add(player);
                return;
            }
            PlayersList[PlayersList.IndexOf(foundPlayer)] = player;
        }

        public IPlayer GetPlayerByXuid(string xuid) {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                return null;
            }
            return foundPlayer;
        }

        public string GetFileName() {
            return FileName;
        }

        public List<LogEntry> GetLog() => ConsoleBuffer = ConsoleBuffer ?? new List<LogEntry>();

        public void SetLog(List<LogEntry> newLog) => ConsoleBuffer = newLog;

        public List<IPlayer> GetPlayerList() => PlayersList ?? new List<IPlayer>();

        public void SetPlayerList(List<IPlayer> newList) => PlayersList = newList;

        public IServerConfiguration GetServerInfo() => this;
    }

}