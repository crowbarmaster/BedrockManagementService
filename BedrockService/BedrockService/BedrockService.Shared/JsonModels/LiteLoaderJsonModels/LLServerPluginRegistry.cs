using System.Collections.Generic;

namespace BedrockService.Shared.JsonModels.LiteLoaderJsonModels {
    public class LLServerPluginRegistry {
        public List<BmsServerPluginDatabase> ServerPluginList { get; set; } = new();
    }

    public class BmsServerPluginDatabase {
        public string BmsServerName { get; set; }
        public List<PluginVersionInfo> InstalledPlugins { get; set; } = new();
    }

    public class PluginVersionInfo {
        public string PluginFileName { get; set; }
        public string BedrockVersion { get; set; }
        public string LiteLoaderVersion { get; set; }

        public override string ToString() {
            return $"{PluginFileName}";
        }
    }
}
