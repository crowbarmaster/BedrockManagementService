using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftFileModels.JsonModels;

namespace BedrockService.Shared.MinecraftFileModels.FileAccessModels {
    public class PackManifestFileModel : BaseJsonFile {
        public PackManifestJsonModel Contents { get; set; } = new();

        public PackManifestFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<PackManifestJsonModel>();
        }

        public PackManifestFileModel() { }
    }
}
