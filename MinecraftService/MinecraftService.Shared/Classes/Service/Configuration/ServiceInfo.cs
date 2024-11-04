using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Service.Configuration
{
    public class ServiceInfo
    {
        public PlayerManager PlayerManager { get; set; }
        public List<LogEntry> serviceLog = [];
        public List<IServerConfiguration> ServerList = [];
        public List<Property> globals = [];
        public Dictionary<MinecraftServerArch, List<Property>> DefaultServerPropsByArch = [];
        public int TotalBackupsServiceWide { get; set; }
        public int TotalBackupSizeServiceWideMegabytes { get; set; }
    }
}