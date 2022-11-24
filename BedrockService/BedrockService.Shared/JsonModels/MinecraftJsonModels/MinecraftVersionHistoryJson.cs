using System.Text.Json.Serialization;

namespace BedrockService.Shared.JsonModels.MinecraftJsonModels {
    public class MinecraftVersionHistoryJson {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("bin-win")]
        public string WindowsBinUrl { get; set; }
        [JsonPropertyName("bin-lin")]
        public string LinuxBinUrl { get; set; }
    }
}
