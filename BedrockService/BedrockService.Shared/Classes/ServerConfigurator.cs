﻿using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public class ServerConfigurator : ServerInfo, IServerConfiguration {
        public ServerConfigurator(string coreServersPath, List<Property> defaultPropList) : base (coreServersPath, defaultPropList) {
            _defaultPropList = defaultPropList;
            ServersPath = coreServersPath;
        }

        public bool InitializeDefaults() {
            ServerName = "Dedicated Server";
            FileName = "Dedicated Server.conf";
            ServerPath = new Property("ServerPath", $@"{ServersPath}\{ServerName}");
            ServerExeName = new Property("ServerExeName", $"BedrockService.{ServerName}.exe");
            ServerAutostartEnabled = new Property("ServerAutostartEnabled", "true");
            ServerPropList.Clear();
            _defaultPropList.ForEach(p => ServerPropList.Add(new Property(p.KeyName, p.DefaultValue)));
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
                        case "ServerAutostartEnabled":
                            bool result = true;
                            if(bool.TryParse(split[1], out result)) {
                                ServerAutostartEnabled.SetValue(result.ToString());
                            }
                            break;  
                            
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
            } catch {

            }
            return false;
        }

        public bool SetProp(Property propToSet) {
            try {
                Property serverProp = ServerPropList.FirstOrDefault(prop => prop.KeyName == propToSet.KeyName);
                if(serverProp != null) {
                    ServerPropList[ServerPropList.IndexOf(serverProp)] = propToSet;
                } else {
                    if(propToSet.KeyName == "ServerAutostartEnabled") {
                        ServerAutostartEnabled.Value = propToSet.Value;
                    }
                }
                return true;
            } catch { }
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
                    case "ServerAutostartEnabled":
                        return ServerAutostartEnabled;
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

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = PlayersList.FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(GetProp("default-player-permission-level").ToString());
                player.Initialize(xuid, username);
                PlayersList.Add(player);
                return player;
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

        public void SetStatus (ServerStatusModel status) => ServerStatus = status;

        public ServerStatusModel GetStatus() => ServerStatus;
    }
}
