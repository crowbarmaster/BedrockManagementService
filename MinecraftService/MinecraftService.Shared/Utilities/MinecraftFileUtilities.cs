using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.PackParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Utilities
{
    public class MinecraftFileUtilities {
        public static bool UpdateWorldPackFile(string filePath, PackManifestJsonModel manifest) {
            WorldPackFileModel worldPackFile = new(filePath);
            var foundpack = worldPackFile.Contents.Where(x => x.pack_id == manifest.header.uuid).ToList();
            if (foundpack.Count() == 1) {
                foundpack[0] = new WorldPackEntryJsonModel(manifest.header.uuid, manifest.header.version);
            }
            worldPackFile.Contents.Add(new WorldPackEntryJsonModel(manifest.header.uuid, manifest.header.version));
            worldPackFile.SaveFile();
            return true;
        }

        public static Task RemoveEntryFromWorldPackFile(string filePath, WorldPackEntryJsonModel manifest) {
            return Task.Run(() => {
                WorldPackFileModel worldPackFile = new(filePath);
                if (worldPackFile.Contents.Where(x => x.pack_id == manifest.pack_id).Count() > 0) {
                }
                WorldPackEntryJsonModel cannedEntry = worldPackFile.Contents.Where(x => x.pack_id == manifest.pack_id).First();
                worldPackFile.Contents.Remove(cannedEntry);
                worldPackFile.SaveFile();
            });
        }

        public static Task ValidateFixWorldPackFiles(string serverPath, string levelName) {
            return Task.Run(() => {
                MinecraftPackParser packParser = new();
                try {
                    packParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.ServerWorldDir_LevelName, serverPath, levelName), 0);
                    List<WorldPackEntryJsonModel> resourceJsonList = JsonConvert.DeserializeObject<List<WorldPackEntryJsonModel>>(File.ReadAllText(GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, serverPath, levelName)));
                    List<WorldPackEntryJsonModel> behaviorJsonList = JsonConvert.DeserializeObject<List<WorldPackEntryJsonModel>>(File.ReadAllText(GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, serverPath, levelName)));
                    foreach (WorldPackEntryJsonModel entry in resourceJsonList) {
                        if (packParser.FoundPacks.Count(x => x.JsonManifest.header.uuid == entry.pack_id) == 0) {
                            RemoveEntryFromWorldPackFile(GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, serverPath, levelName), entry).Wait();
                        }
                    }
                    foreach (WorldPackEntryJsonModel entry in behaviorJsonList) {
                        if (packParser.FoundPacks.Count(x => x.JsonManifest.header.uuid == entry.pack_id) == 0) {
                            RemoveEntryFromWorldPackFile(GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, serverPath, levelName), entry).Wait();
                        }
                    }
                } catch { }
            });
        }

        public static bool UpdateKnownPackFile(string filePath, MinecraftPackContainer contentToAdd) {
            KnownPacksFileModel fileModel = new(filePath);
            if (fileModel.Contents.Where(x => x.uuid == contentToAdd.JsonManifest.header.uuid).Count() > 0) {
                return false;
            }
            fileModel.Contents.Add(new KnownPacksJsonModel(contentToAdd));
            fileModel.SaveToFile();
            return true;
        }

        public static bool RemoveEntryFromKnownPacks(string filePath, MinecraftPackContainer contentToRemove) {
            KnownPacksFileModel fileModel = new(filePath);
            KnownPacksJsonModel modelToRemove = fileModel.Contents.Where(x => x.uuid == contentToRemove.JsonManifest.header.uuid).FirstOrDefault();
            if (modelToRemove == null) {
                return false;
            }
            fileModel.Contents.Remove(modelToRemove);
            fileModel.SaveToFile();
            return true;
        }

        public static void WriteServerJsonFiles(IServerConfiguration server) {
            string permFilePath = $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\permissions.json";
            string whitelistFilePath = $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\whitelist.json";
            PermissionsFileModel permissionsFile = new() { FilePath = permFilePath };
            WhitelistFileModel whitelistFile = new() { FilePath = whitelistFilePath };
            server.GetPlayerList()
                .Where(x => x.IsPlayerWhitelisted())
                .ToList().ForEach(x => {
                    whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.PlayerIgnoresLimit(), x.GetPlayerID(), x.GetUsername()));
                });
            server.GetPlayerList()
                .Where(x => !x.IsDefaultRegistration())
                .ToList().ForEach(x => {
                    permissionsFile.Contents.Add(new PermissionsEntryJsonModel(x.GetPermissionLevel(), x.GetPlayerID()));
                });
            permissionsFile.SaveToFile(permissionsFile.Contents);
            whitelistFile.SaveToFile(whitelistFile.Contents);
        }

        public static void WriteServerPropsFile(IServerConfiguration server) {
            int index = 0;
            string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
            string[] output = new string[2 + server.GetAllProps().Count];
            output[index++] = "#Server";
            server.GetAllProps().ForEach(prop => {
                output[index++] = $"{prop.KeyName}={prop}";
            });
            if (!Directory.Exists(serverPath)) {
                Directory.CreateDirectory(serverPath);
            }
            File.WriteAllLines(GetServerFilePath(ServerFileNameKeys.ServerProps, server), output);
        }

        public static List<Property> CopyPropList(List<Property> souceList) {
            List<Property> result = [];
            foreach (Property prop in souceList) {
                result.Add(new Property(prop));
            }
            return result;
        }

        public static List<string[]> FilterLinesFromPropFile(string filePath, char delim = '=') {
            FileUtilities.CreateInexistentFile(filePath);
            return File.ReadAllLines(filePath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(delim))
                    .ToList();
        }

        public static List<Property> CreatePropListFromFile(string filePath, char delim = '=') {
            List<string[]> result = FilterLinesFromPropFile(filePath, delim);
            List<Property> output = [];
            result.ForEach(x => {
                if (x.Length < 2) {
                    string[] replace = new string[2];
                    replace[0] = x[0];
                    x = replace;
                }
                output.Add(new(x[0], x[1]));
            });
            return output;
        }

        public static void CleanBedrockDirectory(IServerConfiguration server) {
            DirectoryInfo bedrockDir = new DirectoryInfo(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
            Progress<ProgressModel> nullProgress = new();
            FileUtilities.DeleteFilesFromDirectory($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\resource_packs", true, nullProgress);
            FileUtilities.DeleteFilesFromDirectory($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\behavior_packs", true, nullProgress);
            foreach (FileInfo file in bedrockDir.EnumerateFiles()) {
                if (file.Extension.Equals(".exe") || file.Extension.Equals(".dll")) {
                    File.Delete(file.FullName);
                }
                if ((file.Name + file.Extension) == GetServerFileName(ServerFileNameKeys.DeployedINI)) {
                    file.Delete();
                }
            }
        }

        public static void CleanJavaServerDirectory(IServerConfiguration server) {
            Progress<ProgressModel> nullProgress = new();
            DirectoryInfo workingDir = new DirectoryInfo(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
            foreach (DirectoryInfo dir in workingDir.EnumerateDirectories()) {
                if (dir.Name != "worlds") {
                    FileUtilities.DeleteFilesFromDirectory(dir.FullName, true, nullProgress);
                }
            }
            foreach (FileInfo file in workingDir.EnumerateFiles()) {
                if (file.Name != GetServerFileName(ServerFileNameKeys.DeployedINI)) {
                    file.Delete();
                }
            }
        }

        public static void WriteJavaEulaFile(IServerConfiguration server) {
            File.WriteAllText(GetServerFilePath(ServerFileNameKeys.JavaEula, server), "eula=true\r\n");
        }
    }
}
