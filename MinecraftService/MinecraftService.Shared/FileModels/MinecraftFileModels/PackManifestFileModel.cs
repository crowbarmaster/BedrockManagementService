using MinecraftService.Shared.JsonModels.Minecraft;

namespace MinecraftService.Shared.FileModels.MinecraftFileModels
{
    public class PackManifestFileModel : BaseJsonFile {
        public PackManifestJsonModel Contents { get; set; } = new();

        public PackManifestFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<PackManifestJsonModel>();
        }

        public PackManifestFileModel() { }
    }
}
