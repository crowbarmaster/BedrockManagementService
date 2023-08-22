using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System.Collections.Generic;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes {
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