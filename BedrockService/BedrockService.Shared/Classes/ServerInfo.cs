using BedrockService.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class ServerInfo {
        public string ServersPath;
        public string DefaultPropFilePath;
        public string ServerName { get; set; }
        public string FileName { get; set; }
        public List<LogEntry> ConsoleBuffer = new List<LogEntry>();
        public Property ServerPath { get; set; }
        public Property ServerExeName { get; set; }
        public List<IPlayer> PlayersList = new List<IPlayer>();
        public List<Property> ServerPropList = new List<Property>();
        public List<Property> _defaultPropList = new List<Property>();
        public List<StartCmdEntry> StartCmds = new List<StartCmdEntry>();

        public ServerInfo(string coreServersPath, List<Property> defaultPropList) {
            _defaultPropList = defaultPropList;
            ServersPath = coreServersPath;
        }
    }
}