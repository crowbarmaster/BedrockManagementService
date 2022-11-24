using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System.Collections.Generic;

namespace BedrockService.Shared.Classes {
    public class ServiceInfo {
        public IPlayerManager PlayerManager;
        public string LatestServerVersion { get; set; }
        public List<LogEntry> serviceLog = new();
        public List<IServerConfiguration> ServerList = new();
        public List<IPlayer> PlayersList = new();
        public List<Property> globals = new();
        public List<Property> DefaultServerProps = new();
        public LLServerPluginRegistry LLServerPluginRegistry = new();
        public int TotalBackupsServiceWide { get; set; }
        public int TotalBackupSizeServiceWideMegabytes { get; set; }
    }
}