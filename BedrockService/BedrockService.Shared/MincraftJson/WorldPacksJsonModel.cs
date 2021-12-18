using System.Collections.Generic;

namespace BedrockService.Shared.MincraftJson {
    public class WorldPacksJsonModel {
        public string pack_id { get; set; }
        public List<int> version { get; set; }

        public WorldPacksJsonModel(string id, List<int> ver) {
            pack_id = id;
            version = ver;
        }
    }
}
