﻿using MinecraftService.Shared.JsonModels.Minecraft;
using System.Collections.Generic;

namespace MinecraftService.Shared.FileModels.MinecraftFileModels
{
    public class KnownPacksFileModel : BaseJsonFile {
        public List<KnownPacksJsonModel> Contents { get; set; } = new();

        public KnownPacksFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<KnownPacksJsonModel>>();
        }

        public KnownPacksFileModel() { }

        public void SaveToFile() => SaveToFile(Contents);
    }
}
