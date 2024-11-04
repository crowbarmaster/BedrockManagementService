using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.PackParser
{
    public class MinecraftKnownPacksClass {
        public WorldPackFileModel InstalledResourcePacks;
        public WorldPackFileModel InstalledBehaviorPacks;
        public KnownPacksFileModel InstalledPacks;

        public MinecraftKnownPacksClass(string validPacksFile, string worldDirectory) {
            if (!File.Exists($@"{worldDirectory}\world_behavior_packs.json")) {
                File.Create($@"{worldDirectory}\world_behavior_packs.json").Close();
            }
            if (!File.Exists($@"{worldDirectory}\world_resource_packs.json")) {
                File.Create($@"{worldDirectory}\world_resource_packs.json").Close();
            }
            InstalledBehaviorPacks = new WorldPackFileModel($@"{worldDirectory}\world_behavior_packs.json");
            InstalledResourcePacks = new WorldPackFileModel($@"{worldDirectory}\world_resource_packs.json");
            InstalledPacks = new KnownPacksFileModel(validPacksFile);
            if (InstalledPacks.Contents[0].file_version != 0) {
                InstalledPacks.Contents.RemoveAt(0); // Strip file version entry.
            }
            List<KnownPacksJsonModel> AddedPacks = new();
            InstalledBehaviorPacks.Contents.ForEach((x) => {
                AddedPacks.AddRange(InstalledPacks.Contents.Where(y => y.uuid == x.pack_id).ToList());
            });
            InstalledResourcePacks.Contents.ForEach((x) => {
                AddedPacks.AddRange(InstalledPacks.Contents.Where(y => y.uuid == x.pack_id).ToList());
            });
            InstalledPacks.Contents.Clear();
            InstalledPacks.Contents.AddRange(AddedPacks);
        }

        public void RemovePackFromServer(IServerConfiguration configuration, MinecraftPackContainer pack) {
            string serverPath = configuration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
            string serverFolderName = configuration.GetProp(MmsDependServerPropKeys.LevelName).ToString();
            string jsonPackPath = null;
            string jsonWorldPackEnablerPath = null;
            if (pack.ManifestType == "WorldPack") {
                Directory.Delete($@"{serverPath}\worlds\{pack.FolderName}", true);
            }
            if (pack.ManifestType == "data") {
                jsonPackPath = $@"{serverPath}\development_behavior_packs\{pack.FolderName}";
                jsonWorldPackEnablerPath = $@"{serverPath}\worlds\{serverFolderName}\world_behavior_packs.json";
                Directory.Delete(jsonPackPath, true);
            }
            if (pack.ManifestType == "resources") {
                jsonPackPath = $@"{serverPath}\development_resource_packs\{pack.FolderName}";
                jsonWorldPackEnablerPath = $@"{serverPath}\worlds\{serverFolderName}\world_resource_packs.json";
                Directory.Delete(jsonPackPath, true);
            }
            if (jsonWorldPackEnablerPath != null) {
                WorldPackFileModel worldPackFile = new(jsonWorldPackEnablerPath);
                List<WorldPackEntryJsonModel> worldPacks = worldPackFile.Contents;
                WorldPackEntryJsonModel foundEntry = worldPacks.FirstOrDefault(x => x.pack_id.Equals(pack.JsonManifest.header.uuid));
                if (foundEntry != null) {
                    worldPacks.Remove(foundEntry);
                    worldPackFile.SaveFile();
                }
            }
        }
    }
}
