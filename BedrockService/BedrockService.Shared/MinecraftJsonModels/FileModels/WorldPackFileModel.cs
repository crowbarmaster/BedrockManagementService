using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftJsonModels.FileModels {
    public class WorldPackFileModel : BaseJsonFile {
        public List<WorldKnownPackEntryJsonModel> Contents { get; set; } = new();

        public WorldPackFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<WorldKnownPackEntryJsonModel>>();
        }

        public WorldPackFileModel() { }
    }
}
