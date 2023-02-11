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
            public int[] ProtoBlacklist { get; set; }
            public string ExtendedInfo { get; set; }
            public string RepoURL { get; set; }
        }
    }
}
