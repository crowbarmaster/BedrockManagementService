using BedrockService.Shared.FileModels.LiteLoaderFileModels.FileAccessModels;
using BedrockService.Shared.FileModels.MinecraftFileModels;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.JsonModels.MinecraftJsonModels;
using BedrockService.Shared.PackParser;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Utilities {
    public class MinecraftFileUtilities {

        public static bool UpdateWorldPackFile(string filePath, PackManifestJsonModel manifest) {
            WorldPackFileModel worldPackFile = new(filePath);
            if (worldPackFile.Contents.Where(x => x.pack_id == manifest.header.uuid).Count() > 0) {
                return false;
            }
            worldPackFile.Contents.Add(new WorldPackEntryJsonModel(manifest.header.uuid, manifest.header.version));
            worldPackFile.SaveFile();
            return true;
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
                    whitelistFile.Contents.Add(new WhitelistEntryJsonModel(x.PlayerIgnoresLimit(), x.GetXUID(), x.GetUsername()));
                });
            server.GetPlayerList()
                .Where(x => !x.IsDefaultRegistration())
                .ToList().ForEach(x => {
                    permissionsFile.Contents.Add(new PermissionsEntryJsonModel(x.GetPermissionLevel(), x.GetXUID()));
                });
            permissionsFile.SaveToFile(permissionsFile.Contents);
            whitelistFile.SaveToFile(whitelistFile.Contents);
        }

        public static void CreateDefaultLoaderConfigFile(IServerConfiguration server) {
            string configFilePath = GetServerFilePath(BdsFileNameKeys.LLConfig, server);
            FileInfo fileInfo = new(configFilePath);
            if (!fileInfo.Directory.Exists) {
                fileInfo.Directory.Create();
            }
            LiteLoaderFileModel configFile = new() { FilePath = configFilePath };
            LiteLoaderConfigJsonModel configLayout = new() {
                ColorLog = false,
                DebugMode = false,
                Language = "system",
                LogLevel = 4,
                Modules = new() {
                    AddonsHelper = new() { autoInstallPath = "plugins/AddonsHelper", enabled = true },
                    AntiGive = new() { command = "kick {player}", enabled = true },
                    CheckRunningBDS = new() { enabled = true },
                    ClientChunkPreGeneration = new() { enabled = true },
                    CrashLogger = new() { enabled = true, path = "plugins\\LiteLoader\\CrashLogger_Daemon.exe" },
                    EconomyCore = new() { enabled = true },
                    ErrorStackTraceback = new() { enabled = true, cacheSymbol = true },
                    FixBDSCrash = new() { enabled = true },
                    FixAbility = new() { enabled = true },
                    FixDisconnectBug = new() { enabled = true },
                    FixListenPort = new() { enabled = false },
                    FixMcBug = new() { enabled = true },
                    ForceUtf8Input = new() { enabled = false },
                    OutputFilter = new() { enabled = true, filterRegex = new List<object>(), onlyFilterConsoleOutput = true },
                    ParticleAPI = new() { enabled = false },
                    PermissionAPI = new() { enabled = true },
                    SimpleServerLogger = new() { enabled = true },
                    TpdimCommand = new() { enabled = true },
                    UnlockCmd = new() { enabled = true },
                    UnoccupyPort19132 = new() { enabled = true },
                    WelcomeText = new() { enabled = false }
                },
                ScriptEngine = new() { enabled = true, alwaysLaunch = false },
                Version = 1
            };
            configFile.Contents = configLayout;
            configFile.SaveToFile();
        }

        public static void WriteLiteLoaderConfigFile(IServerConfiguration server) {
            string configFilePath = $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\plugins\LiteLoader\LiteLoader.json";
            LiteLoaderFileModel configFile = new() { FilePath = configFilePath };
        }

        public static LiteLoaderConfigNodeModel LoadLiteLoaderConfigFile(IServerConfiguration server) {
            if (!File.Exists(GetServerFilePath(BdsFileNameKeys.LLConfig, server))) {
                CreateDefaultLoaderConfigFile(server);
            }
            return new("Root", File.ReadAllText(GetServerFilePath(BdsFileNameKeys.LLConfig, server)));
        }

        public static void VerifyLiteLoaderCompatableSettings(IProcessInfo processInfo, IServerConfiguration server) {
            if (server.GetLiteLoaderConfig() == null) {
                return;
            }
            server.GetLiteLoaderConfig().Properties["ColorLog"] = false;
            if (processInfo.IsDebugEnabled()) {
                server.GetLiteLoaderConfig().Properties["DebugMode"] = false;
                server.GetLiteLoaderConfig().GetChildByName("Modules").GetChildByName("CrashLogger").Properties["enabled"] = false;
            }
            server.GetLiteLoaderConfig().SaveToFile(GetServerFilePath(BdsFileNameKeys.LLConfig, server));
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
            File.WriteAllLines(GetServerFilePath(BdsFileNameKeys.ServerProps, server), output);
        }

        public static void CleanBedrockDirectory(IServerConfiguration server) {
            FileUtilities utilities = new FileUtilities();
            DirectoryInfo bedrockDir = new DirectoryInfo(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
            utilities.DeleteFilesFromDirectory($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\resource_packs", true);
            utilities.DeleteFilesFromDirectory($@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\behavior_packs", true);
            foreach(FileInfo file in bedrockDir.EnumerateFiles()) {
                if(file.Extension.Equals(".exe") || file.Extension.Equals(".dll")) {
                    File.Delete(file.FullName);
                }
            }
        }
    }
}
