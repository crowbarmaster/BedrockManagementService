using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;

namespace BedrockService.Shared.MinecraftJsonModels.FileModels {
    public class PackManifestFileModel : BaseJsonFile {
        public PackManifestJsonModel Contents { get; set; } = new();

        public PackManifestFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<PackManifestJsonModel>();
        }

        public PackManifestFileModel() { }
    }
}
