using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockService.Shared.PackParser
{
    public class MinecraftKnownPacksClass
    {
        public List<KnownPack> KnownPacks = new List<KnownPack>();
        private List<KnownPack> stockPacks = new List<KnownPack>();
        public class KnownPack
        {
            public int file_version { get; set; }
            public string file_system { get; set; }
            public bool? from_disk { get; set; }
            public List<string> hashes { get; set; }
            public string path { get; set; }
            public string uuid { get; set; }
            public string version { get; set; }

            public override string ToString()
            {
                return path;
            }
        }

        public MinecraftKnownPacksClass(string serverFile, string stockFile)
        {
            KnownPacks = ParseJsonArray(serverFile);
            stockPacks = ParseJsonArray(stockFile);
            KnownPacks.RemoveAt(0); // Strip file version entry.
            foreach (KnownPack pack in stockPacks)
            {
                KnownPack packToRemove = new KnownPack();
                if (pack.uuid == null)
                    continue;
                packToRemove = KnownPacks.First(p => p.uuid != null && p.uuid == pack.uuid);
                KnownPacks.Remove(packToRemove);
            }
        }

        public void RemovePackFromServer(string serverPath, MinecraftPackContainer pack)
        {
            if (pack.ManifestType == "WorldPack")
                Directory.Delete($@"{serverPath}\worlds\{pack.FolderName}", true);
            if (pack.ManifestType == "data")
                Directory.Delete($@"{serverPath}\behavior_packs\{pack.FolderName}", true);
            if (pack.ManifestType == "resources")
                Directory.Delete($@"{serverPath}\resource_packs\{ pack.FolderName}", true);
            KnownPacks.Remove(KnownPacks.First(p => p.uuid != null || p.uuid == pack.JsonManifest.header.uuid));
            List<KnownPack> packsToWrite = new List<KnownPack>(stockPacks);
            if (KnownPacks.Count > 0)
                packsToWrite.AddRange(KnownPacks);
            File.WriteAllText($@"{serverPath}\valid_known_packs.json", JArray.FromObject(packsToWrite).ToString());
        }

        private List<KnownPack> ParseJsonArray(string jsonFile)
        {
            JArray packList = JArray.Parse(File.ReadAllText(jsonFile));
            List<KnownPack> parsedPackInfos = new List<KnownPack>();
            foreach (JToken token in packList)
                parsedPackInfos.Add(token.ToObject<KnownPack>());
            return parsedPackInfos;
        }
    }
}
