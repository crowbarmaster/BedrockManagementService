using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes {
    public class ServiceInfo {
        public IPlayerManager PlayerManager { get; set; }
        public string LatestServerVersion { get; set; }
        public List<LogEntry> serviceLog = new();
        public List<IServerConfiguration> ServerList = new();
        public List<Property> globals = new();
        public Dictionary<MinecraftServerArch, List<Property>> DefaultServerPropsByArch = new();
        public LLServerPluginRegistry LLServerPluginRegistry = new();
        public int TotalBackupsServiceWide { get; set; }
        public int TotalBackupSizeServiceWideMegabytes { get; set; }
    }
}