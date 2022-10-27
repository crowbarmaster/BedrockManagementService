using BedrockService.Shared.Classes;
using BedrockService.Shared.MinecraftFileModels.JsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftFileModels.FileAccessModels {
    public class PermissionsFileModel : BaseJsonFile {
        public List<PermissionsEntryJsonModel> Contents { get; set; } = new();

        public PermissionsFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<PermissionsEntryJsonModel>>();
        }

        public PermissionsFileModel() { }
    }
}
