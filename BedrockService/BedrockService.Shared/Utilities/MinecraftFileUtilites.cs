using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Utilities {
    public class MinecraftFileUtilites {

        public static bool UpdateWorldPackFile(string filePath, PackManifestJsonModel manifest) {
            WorldPackFileModel worldPackFile = new WorldPackFileModel(filePath);
            if (worldPackFile.Contents.Where(x => x.pack_id == manifest.header.uuid).Count() > 0) {
                return false;
            }
            worldPackFile.Contents.Add(new WorldPackEntryJsonModel(manifest.header.uuid, manifest.header.version));
            worldPackFile.SaveFile();
            return true;
        }

        public static void WriteServerJsonFiles(IServerConfiguration server) {
            string permFilePath = $@"{server.GetProp("ServerPath")}\permissions.json";
            string whitelistFilePath = $@"{server.GetProp("ServerPath")}\whitelist.json";
            PermissionsFileModel permissionsFile = new() { FilePath = permFilePath };
            WhitelistFileModel whitelistFile = new() { FilePath = whitelistFilePath };
            server.GetPlayerList()
                .Where(x => x.IsPlayerWhitelisted())
                .Select(x => (xuid: x.GetXUID(), userName: x.GetUsername(), ignoreLimits: x.PlayerIgnoresLimit()))
                .ToList().ForEach(x => {
                    whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.ignoreLimits, x.xuid, x.userName));
                });
            server.GetPlayerList()
                .Where(x => !x.IsDefaultRegistration())
                .Select(x => (xuid: x.GetXUID(), permLevel: x.GetPermissionLevel()))
                .ToList().ForEach(x => {
                    permissionsFile.Contents.Add(new PermissionsEntryJsonModel(x.permLevel, x.xuid));
                });
            permissionsFile.SaveToFile(permissionsFile.Contents);
            whitelistFile.SaveToFile(whitelistFile.Contents);
        }

        public static void WriteServerPropsFile(IServerConfiguration server) {
            int index = 0;
            string serverPath = server.GetProp("ServerPath").ToString();
            string[] output = new string[2 + server.GetAllProps().Count];
            output[index++] = "#Server";
            server.GetAllProps().ForEach(prop => {
                output[index++] = $"{prop.KeyName}={prop}";
            });
            if (!Directory.Exists(serverPath)) {
                Directory.CreateDirectory(serverPath);
            }
            File.WriteAllLines($@"{serverPath}\server.properties", output);
        }
    }
}
