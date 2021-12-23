using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftJsonModels.FileModels {
    public class WhitelistFileModel : BaseJsonFile {
        public List<WhitelistEntryJsonModel> Contents { get; set; } = new();

        public WhitelistFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<WhitelistEntryJsonModel>>();
        }

        public WhitelistFileModel() { }
    }
}
