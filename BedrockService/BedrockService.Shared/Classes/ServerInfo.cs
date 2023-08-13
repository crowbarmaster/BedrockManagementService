using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System.Collections.Generic;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes {
    public class ServerInfo {
        public MinecraftServerArch ServerArch { get; set; }
        public int ProcessID { get; set; }
        public IPlayerManager PlayerManager { get; set; }
        public Property ServersPath { get; set; }
        public int TotalBackupSizeKilobytes { get; set; }
        public int TotalBackupsStored { get; set; }
        public ServerStatusModel ServerStatus { get; set; } = new();
        public List<LogEntry> ServerLogs = new();
        public LiteLoaderConfigNodeModel LiteLoaderConfigProps { get; set; }
        public List<Property> ServicePropList = new();
        public List<Property> ServerPropList = new();
        public List<Property> DefaultPropList = new();
        public List<StartCmdEntry> StartCmds = new();

        public ServerInfo() {
        }
    }
}