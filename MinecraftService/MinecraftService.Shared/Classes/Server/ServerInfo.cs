using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Server
{
    public class ServerInfo
    {
        public MinecraftServerArch ServerArch { get; set; }
        public int ProcessID { get; set; }
        public PlayerManager PlayerManager { get; set; }
        public Property ServersPath { get; set; }
        public int TotalBackupSizeKilobytes { get; set; }
        public int TotalBackupsStored { get; set; }
        public ServerStatusModel ServerStatus { get; set; } = new();
        public List<LogEntry> ServerLogs = new();
        public List<Property> ServicePropList = new();
        public List<Property> ServerPropList = new();
        public List<Property> DefaultPropList = new();
        public List<StartCmdEntry> StartCmds = new();

        public ServerInfo()
        {
        }
    }
}