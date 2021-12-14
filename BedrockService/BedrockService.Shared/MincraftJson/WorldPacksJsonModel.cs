using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockService.Shared.MincraftJson
{
    public class WorldPacksJsonModel
    {
        public string pack_id { get; set; }
        public List<int> version { get; set; }

        public WorldPacksJsonModel(string id, List<int> ver)
        {
            pack_id = id;
            version = ver;
        }
    }
}
