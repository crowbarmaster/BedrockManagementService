using MinecraftService.Shared.JsonModels.Minecraft;
using System.Collections.Generic;

namespace MinecraftService.Shared.FileModels.MinecraftFileModels
{
    public class PermissionsFileModel : BaseJsonFile {
        public List<PermissionsEntryJsonModel> Contents { get; set; } = new();

        public PermissionsFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<PermissionsEntryJsonModel>>();
        }

        public PermissionsFileModel() { }
    }
}
