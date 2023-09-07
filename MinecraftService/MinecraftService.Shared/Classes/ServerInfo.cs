using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes {
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