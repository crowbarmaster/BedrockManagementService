using BedrockService.Shared.Classes;
using BedrockService.Shared.JsonModels.MinecraftJsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.FileModels.MinecraftFileModels {
    public class PermissionsFileModel : BaseJsonFile {
        public List<PermissionsEntryJsonModel> Contents { get; set; } = new();

        public PermissionsFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<PermissionsEntryJsonModel>>();
        }

        public PermissionsFileModel() { }
    }
}
