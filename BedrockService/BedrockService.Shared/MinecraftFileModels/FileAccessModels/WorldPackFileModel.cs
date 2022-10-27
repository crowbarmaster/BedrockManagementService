using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftFileModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftFileModels.FileAccessModels {
    public class WorldPackFileModel : BaseJsonFile {
        public List<WorldPackEntryJsonModel> Contents { get; set; } = new();

        public WorldPackFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<WorldPackEntryJsonModel>>();
            if (Contents == null) {
                Contents = new();
            }
        }

        public WorldPackFileModel() { }

        public void SaveFile() => SaveToFile(Contents);
    }
}
