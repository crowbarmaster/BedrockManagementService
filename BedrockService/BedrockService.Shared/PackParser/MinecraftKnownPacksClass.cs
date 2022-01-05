using BedrockService.Shared.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BedrockService.Shared.PackParser {
    public class MinecraftKnownPacksClass {
        public KnownPacksFileModel InstalledPacks;
        private readonly KnownPacksFileModel _stockDataModel;

        public MinecraftKnownPacksClass(string serverFile, string stockFile) {
            _stockDataModel = new KnownPacksFileModel(stockFile);
            InstalledPacks = new KnownPacksFileModel(serverFile);
            if (InstalledPacks.Contents[0].file_version != 0) {
                InstalledPacks.Contents.RemoveAt(0); // Strip file version entry.
            }
            List<KnownPacksJsonModel> AddedPacks = InstalledPacks.Contents.Except(_stockDataModel.Contents).ToList();
            InstalledPacks.Contents.Clear();
            InstalledPacks.Contents.AddRange(AddedPacks);
        }

        public void RemovePackFromServer(IServerConfiguration configuration, MinecraftPackContainer pack) {
            string serverPath = configuration.GetProp("ServerPath").ToString();
            string serverFolderName = configuration.GetProp("level-name").ToString();
            string jsonPackPath = null;
            string jsonWorldPackEnablerPath = null;
            if (pack.ManifestType == "WorldPack") {
                Directory.Delete($@"{serverPath}\worlds\{pack.FolderName}", true);
            }
            if (pack.ManifestType == "data") {
                jsonPackPath = $@"{serverPath}\behavior_packs\{pack.FolderName}";
                jsonWorldPackEnablerPath = $@"{serverPath}\worlds\{serverFolderName}\world_behavior_packs.json";
                Directory.Delete(jsonPackPath, true);
            }
            if (pack.ManifestType == "resources") {
                jsonPackPath = $@"{serverPath}\resource_packs\{pack.FolderName}";
                jsonWorldPackEnablerPath = $@"{serverPath}\worlds\{serverFolderName}\world_resource_packs.json";
                Directory.Delete(jsonPackPath, true);
            }
            if (jsonWorldPackEnablerPath != null) {
                WorldPackFileModel worldPackFile = new WorldPackFileModel(jsonWorldPackEnablerPath);
                List<WorldPackEntryJsonModel> worldPacks = worldPackFile.Contents;
                WorldPackEntryJsonModel foundEntry = worldPacks.FirstOrDefault(x => x.pack_id.Equals(pack.JsonManifest.header.uuid));
                if (foundEntry != null) {
                    worldPacks.Remove(foundEntry);
                    worldPackFile.SaveFile();
                }
            }
            MinecraftFileUtilities.RemoveEntryFromKnownPacks($@"{serverPath}\valid_known_packs.json", pack);
        }
    }
}
