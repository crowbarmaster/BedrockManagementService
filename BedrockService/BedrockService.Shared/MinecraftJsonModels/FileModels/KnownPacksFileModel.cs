using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;

namespace BedrockService.Shared.MinecraftJsonModels.FileModels {
    public class KnownPacksFileModel : BaseJsonFile {
        public List<KnownPacksJsonModel> Contents { get; set; } = new();

        public KnownPacksFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<List<KnownPacksJsonModel>>();
        }

        public KnownPacksFileModel() { }
    }
}
