using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftJsonModels.FileModels {
    public class KnownPacksFileModel : BaseJsonFile {
        public List<KnownPacksJsonModel> Contents { get; set; } = new();

        public KnownPacksFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<KnownPacksJsonModel>>();
        }

        public KnownPacksFileModel() { }

        public void SaveToFile() => SaveToFile(Contents);
    }
}
