using System;
using System.Collections.Generic;

namespace BedrockService.Shared.MinecraftJsonModels.JsonModels {
    public class WorldKnownPackEntryJsonModel {
        public string pack_id { get; set; }
        public List<int> version { get; set; }

        public WorldKnownPackEntryJsonModel(string id, List<int> ver) {
            pack_id = id;
            version = ver;
        }

        public override bool Equals(object obj) {
            return obj is WorldKnownPackEntryJsonModel model &&
                   pack_id == model.pack_id;
        }

        public override int GetHashCode() {
            return HashCode.Combine(pack_id);
        }
    }
}
