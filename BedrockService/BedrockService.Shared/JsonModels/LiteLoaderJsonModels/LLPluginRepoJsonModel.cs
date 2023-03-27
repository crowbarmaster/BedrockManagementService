using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.JsonModels.LiteLoaderJsonModels {

    public class LLPluginRepoJsonModel {
        public List<PluginEntry> PluginRepo { get; set; } = new();

        public class PluginEntry {
            public string Name { get; set; }
            public string Description { get; set; }
            public string PluginVersion { get; set; }
            public int ProtoVersion { get; set; }
            public string ExtendedInfo { get; set; }
            public int[] ProtoBlacklist { get; set; }
            public string[] ProtectedFiles { get; set; }
            public string RepoURL { get; set; }
        }
    }

    public class PluginManifest {
        public string Name { get; set; }
        public string DllName { get; set; }
        public string Version { get; set; }
        public string LLVersion { get; set; }
        public string Description { get; set; }
        public string ExtendedInfo { get; set; }
        public string ProtectedFiles { get; set; }
    }
}
