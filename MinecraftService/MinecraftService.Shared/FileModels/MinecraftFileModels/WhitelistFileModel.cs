using MinecraftService.Shared.JsonModels.Minecraft;
using System.Collections.Generic;

namespace MinecraftService.Shared.FileModels.MinecraftFileModels
{
    public class WhitelistFileModel : BaseJsonFile {
        public List<WhitelistEntryJsonModel> Contents { get; set; } = new();

        public WhitelistFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<WhitelistEntryJsonModel>>();
        }

        public WhitelistFileModel() { }
    }
}
