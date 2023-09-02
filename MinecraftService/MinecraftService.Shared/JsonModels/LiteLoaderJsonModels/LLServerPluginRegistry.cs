using System.Collections.Generic;

namespace MinecraftService.Shared.JsonModels.LiteLoaderJsonModels {
    public class LLServerPluginRegistry {
        public List<MmsServerPluginDatabase> ServerPluginList { get; set; } = new();
    }

    public class MmsServerPluginDatabase {
        public string MmsServerName { get; set; }
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
