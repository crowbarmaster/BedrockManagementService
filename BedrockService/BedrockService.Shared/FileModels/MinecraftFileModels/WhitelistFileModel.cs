using BedrockService.Shared.Classes;
using BedrockService.Shared.JsonModels.MinecraftJsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.FileModels.MinecraftFileModels {
    public class WhitelistFileModel : BaseJsonFile {
        public List<WhitelistEntryJsonModel> Contents { get; set; } = new();

        public WhitelistFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<WhitelistEntryJsonModel>>();
        }

        public WhitelistFileModel() { }
    }
}
