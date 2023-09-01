using System.Text.Json.Serialization;

namespace MinecraftService.Shared.JsonModels.MinecraftJsonModels {
    public class BedrockVersionHistoryJson {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("winurl")]
        public string WindowsBinUrl { get; set; }
        [JsonPropertyName("linurl")]
        public string LinuxBinUrl { get; set; }
    }
}
