using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MinecraftService.Shared.JsonModels.MinecraftJsonModels {
    public class LegacyBedrockVersionModel {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("winurl")]
        public string WindowsBinUrl { get; set; }
        [JsonPropertyName("linurl")]
        public string LinuxBinUrl { get; set; }
    }

    public class BedrockVersionHistoryModel {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("winurl")]
        public string WindowsBinUrl { get; set; }
        [JsonPropertyName("linurl")]
        public string LinuxBinUrl { get; set; }
        [JsonPropertyName("proplist")]
        public List<PropInfoEntry> PropList { get; set; } = new();
    }

    public class PropInfoEntry {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
