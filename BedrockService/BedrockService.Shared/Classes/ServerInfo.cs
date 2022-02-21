using BedrockService.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class ServerInfo {
        public Property ServersPath { get; set; }
        public int TotalBackupSizeKilobytes { get; set; }
        public int TotalBackupsStored { get; set; }
        public ServerStatusModel ServerStatus { get; set; } = new();
        public List<LogEntry> ConsoleBuffer = new List<LogEntry>();
        public List<IPlayer> PlayersList = new List<IPlayer>();
        public List<Property> ServicePropList = new List<Property>();
        public List<Property> ServerPropList = new List<Property>();
        public List<Property> _defaultPropList = new List<Property>();
        public List<StartCmdEntry> StartCmds = new List<StartCmdEntry>();

        public ServerInfo() {
        }
    }
}