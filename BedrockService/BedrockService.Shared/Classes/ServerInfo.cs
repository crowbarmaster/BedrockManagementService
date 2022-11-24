using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System.Collections.Generic;

namespace BedrockService.Shared.Classes {
    public class ServerInfo {
        public ServerPlayerManager PlayerManager { get; set; }
        public Property ServersPath { get; set; }
        public int TotalBackupSizeKilobytes { get; set; }
        public int TotalBackupsStored { get; set; }
        public bool LiteLoaderEnabled { get; set; }
        public ServerStatusModel ServerStatus { get; set; } = new();
        public List<LogEntry> ConsoleBuffer = new();
        public List<IPlayer> PlayersList = new();
        public LiteLoaderConfigNodeModel LiteLoaderConfigProps { get; set; }
        public List<Property> ServicePropList = new();
        public List<Property> ServerPropList = new();
        public List<Property> _defaultPropList = new();
        public List<StartCmdEntry> StartCmds = new();

        public ServerInfo() {
        }
    }
}