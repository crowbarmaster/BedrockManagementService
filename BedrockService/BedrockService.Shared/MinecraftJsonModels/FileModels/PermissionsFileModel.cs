using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftJsonModels.FileModels {
    public class PermissionsFileModel : BaseJsonFile {
        public List<PermissionsEntryJsonModel> Contents { get; set; } = new();

        public PermissionsFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<PermissionsEntryJsonModel>>();
        }

        public PermissionsFileModel() { }
    }
}
