using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftFileModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftFileModels.FileAccessModels {
    public class WhitelistFileModel : BaseJsonFile {
        public List<WhitelistEntryJsonModel> Contents { get; set; } = new();

        public WhitelistFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<WhitelistEntryJsonModel>>();
        }

        public WhitelistFileModel() { }
    }
}
