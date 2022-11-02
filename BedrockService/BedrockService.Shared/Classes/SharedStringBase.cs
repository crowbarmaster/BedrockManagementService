using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace BedrockService.Shared.Classes {
    public static class SharedStringBase {

        public enum ServicePropertyKeys {
            ServersPath,
            AcceptedMojangLic,
            ClientPort,
            LogServerOutput,
            LogApplicationOutput,
            TimestampLogEntries,
            GlobalizedPlayerDatabase,
            DefaultGlobalPermLevel,
            LatestLiteLoaderVersion,
        };

        public enum ServerPropertyKeys {
            ServerName,
            FileName,
            ServerPath,
            ServerExeName,
            LiteLoaderEnabled,
            ServerAutostartEnabled,
            BackupEnabled,
            BackupPath,
            BackupCron,
            MaxBackupCount,
            AutoBackupsContainPacks,
            IgnoreInactiveBackups,
            CheckUpdates,
            AutoDeployUpdates,
            UpdateCron,
            SelectedServerVersion,
            DeployedVersion
        }

        public enum MinecraftPackTypes {
            data,
            resources,
            WorldPack
        };

        public enum BmsDependServerPropKeys {
            ServerName,
            PortI4,
            PortI6,
            LevelName,
            PermLevel
        }

        public enum BmsFileNameKeys {
            ServiceConfig,
            ClientConfig,
            ServerConfig_Name,
            BmsVersionIni,
            BedrockVersionIni,
            BdsUpdatePackage_Ver,
            LLUpdatePackage_Ver,
            StockProps,
            ServerPlayerRegistry_Name,
            ServerPlayerTelem_Name,
            GlobalPlayerRegistry,
            GlobalPlayerTelem
        }

        public enum BmsDirectoryKeys {
            WorkingDirectory,
            Root,
            BmsLogs,
            BmsConfig,
            ServerConfigs,
            ServerPlayerPath,
            GlobalPlayerPath,
            BdsBuilds,
            LLBuilds,
            BuildArchives,
            CoreFiles,
            CoreFileBuild_Ver,
        }

        public enum BdsFileNameKeys {
            BmsServer_Name,
            ValidKnownPacks,
            LevelDat,
            VanillaBedrock,
            ServerProps,
            WorldResourcePacks,
            WorldBehaviorPacks,
            AllowList,
            WhiteList,
            PermList,
            LLConfig
        }

        public enum BdsDirectoryKeys {
            ServerRoot,
            ServerWorldDir_Name,
            ResourcePacksDir,
            BehaviorPacksDir,
            LLPlugins,
            LLConfigDir
        }

        public enum BmsUrlKeys {
            BdsDownloadPage,
            VersionRegx,
            BdsPackage_Ver,
            LLPackage_Ver
        }

        public static Dictionary<BmsFileNameKeys, BmsDirectoryKeys> BmsFileParentDirectories = new() {
            { BmsFileNameKeys.ServiceConfig, BmsDirectoryKeys.Root },
            { BmsFileNameKeys.ClientConfig, BmsDirectoryKeys.Root },
            { BmsFileNameKeys.ServerConfig_Name, BmsDirectoryKeys.ServerConfigs },
            { BmsFileNameKeys.BmsVersionIni, BmsDirectoryKeys.Root },
            { BmsFileNameKeys.BedrockVersionIni, BmsDirectoryKeys.BmsConfig },
            { BmsFileNameKeys.BdsUpdatePackage_Ver, BmsDirectoryKeys.BuildArchives },
            { BmsFileNameKeys.LLUpdatePackage_Ver, BmsDirectoryKeys.LLBuilds },
            { BmsFileNameKeys.StockProps, BmsDirectoryKeys.CoreFileBuild_Ver },
            { BmsFileNameKeys.ServerPlayerRegistry_Name, BmsDirectoryKeys.ServerPlayerPath },
            { BmsFileNameKeys.GlobalPlayerRegistry, BmsDirectoryKeys.GlobalPlayerPath },
            { BmsFileNameKeys.ServerPlayerTelem_Name, BmsDirectoryKeys.ServerPlayerPath },
            { BmsFileNameKeys.GlobalPlayerTelem, BmsDirectoryKeys.GlobalPlayerPath }
        };

        public static Dictionary<BdsFileNameKeys, BdsDirectoryKeys> BdsFileParentDirectories = new() {
            { BdsFileNameKeys.BmsServer_Name, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.VanillaBedrock, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.ServerProps, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.ValidKnownPacks, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.WorldBehaviorPacks, BdsDirectoryKeys.ServerWorldDir_Name },
            { BdsFileNameKeys.WorldResourcePacks, BdsDirectoryKeys.ServerWorldDir_Name },
            { BdsFileNameKeys.AllowList, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.WhiteList, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.PermList, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.LevelDat, BdsDirectoryKeys.ServerWorldDir_Name }
        };

        public static Dictionary<BdsDirectoryKeys, string> BdsDirectoryStrings = new() {
            { BdsDirectoryKeys.ServerRoot, "" },
            { BdsDirectoryKeys.ServerWorldDir_Name, "worlds\\{0}" },
            { BdsDirectoryKeys.ResourcePacksDir, "development_resource_packs" },
            { BdsDirectoryKeys.BehaviorPacksDir, "development_behavior_packs" },
            { BdsDirectoryKeys.LLPlugins, "plugins" },
            { BdsDirectoryKeys.LLConfigDir, "plugins\\LiteLoader" }
        };

        public static Dictionary<BmsUrlKeys, string> BmsUrlStrings = new() {
            { BmsUrlKeys.BdsDownloadPage, "https://www.minecraft.net/en-us/download/server/bedrock" },
            { BmsUrlKeys.BdsPackage_Ver, "https://minecraft.azureedge.net/bin-win/bedrock-server-{0}.zip" },
            { BmsUrlKeys.VersionRegx, @"(https://minecraft.azureedge.net/bin-win/bedrock-server-)(.*)(\.zip)" },
            { BmsUrlKeys.LLPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/LiteLoader-{0}.zip" }
        };

        public static Dictionary<BmsDirectoryKeys, string> BmsDirectoryStrings = new() {
            { BmsDirectoryKeys.Root, "" },
            { BmsDirectoryKeys.BmsConfig, "BmsConfig" },
            { BmsDirectoryKeys.ServerConfigs, "BmsConfig\\ServerConfigs" },
            { BmsDirectoryKeys.ServerPlayerPath, "BmsConfig\\ServerConfigs\\PlayerRecords" },
            { BmsDirectoryKeys.GlobalPlayerPath, "BmsConfig\\ServerConfigs\\GlobalPlayers" },
            { BmsDirectoryKeys.LLBuilds, "BmsConfig\\LLBuilds" },
            { BmsDirectoryKeys.BdsBuilds, "BmsConfig\\BDSBuilds" },
            { BmsDirectoryKeys.BuildArchives, "BmsConfig\\BDSBuilds\\BuildArchives" },
            { BmsDirectoryKeys.CoreFiles, "BmsConfig\\BDSBuilds\\CoreFiles" },
            { BmsDirectoryKeys.CoreFileBuild_Ver, "BmsConfig\\BDSBuilds\\CoreFiles\\Build_{0}" }
        };

        public static Dictionary<BmsFileNameKeys, string> BmsFileNameStrings = new() {
            { BmsFileNameKeys.ServiceConfig, "Service.conf" },
            { BmsFileNameKeys.ClientConfig, "Client.conf" },
            { BmsFileNameKeys.ServerConfig_Name, "{0}.conf" },
            { BmsFileNameKeys.BedrockVersionIni, "latest_bedrock_ver.ini" },
            { BmsFileNameKeys.BmsVersionIni, "ServiceVersion.ini" },
            { BmsFileNameKeys.StockProps, "stock_props.conf" },
            { BmsFileNameKeys.GlobalPlayerRegistry, "Service.preg" },
            { BmsFileNameKeys.ServerPlayerRegistry_Name, "{0}.preg" },
            { BmsFileNameKeys.GlobalPlayerTelem, "Service.playerdb" },
            { BmsFileNameKeys.ServerPlayerTelem_Name, "{0}.playerdb" },
            { BmsFileNameKeys.BdsUpdatePackage_Ver, "Update_{0}.zip" },
            { BmsFileNameKeys.LLUpdatePackage_Ver, "LLUpdate_{0}.zip" }
        };

        public static Dictionary<BdsFileNameKeys, string> BdsFileNameStrings = new() {
            { BdsFileNameKeys.BmsServer_Name, "BedrockService.{0}.exe" },
            { BdsFileNameKeys.VanillaBedrock, "bedrock_server.exe" },
            { BdsFileNameKeys.ValidKnownPacks, "valid_known_packs.json" },
            { BdsFileNameKeys.LevelDat, "level.dat" },
            { BdsFileNameKeys.ServerProps, "server.properties" },
            { BdsFileNameKeys.WorldResourcePacks, "world_resource_packs.json" },
            { BdsFileNameKeys.WorldBehaviorPacks, "world_behavior_packs.json" },
            { BdsFileNameKeys.AllowList, "allowlist.json" },
            { BdsFileNameKeys.WhiteList, "whitelist.json" },
            { BdsFileNameKeys.PermList, "permissions.json" },
            { BdsFileNameKeys.LLConfig, "LiteLoader.json" }
        };

        public static Dictionary<ServicePropertyKeys, string> ServicePropertyStrings = new() {
            { ServicePropertyKeys.ServersPath, "ServersPath" },
            { ServicePropertyKeys.AcceptedMojangLic, "AcceptedMojangLic" },
            { ServicePropertyKeys.ClientPort, "ClientPort" },
            { ServicePropertyKeys.LogServerOutput, "LogServerOutput" },
            { ServicePropertyKeys.LogApplicationOutput, "LogApplicationOutput" },
            { ServicePropertyKeys.TimestampLogEntries, "TimestampLogEntries" },
            { ServicePropertyKeys.GlobalizedPlayerDatabase, "GlobalizedPlayerDatabase" },
            { ServicePropertyKeys.DefaultGlobalPermLevel, "DefaultGlobalPermLevel" },
            { ServicePropertyKeys.LatestLiteLoaderVersion, "LatestLiteLoaderVersion" }
        };

        public static Dictionary<ServerPropertyKeys, string> ServerPropertyStrings = new() {
            { ServerPropertyKeys.ServerName, "ServerName" },
            { ServerPropertyKeys.FileName, "FileName" },
            { ServerPropertyKeys.ServerPath, "ServerPath" },
            { ServerPropertyKeys.ServerExeName, "ServerExeName" },
            { ServerPropertyKeys.LiteLoaderEnabled, "LiteLoaderEnabled" },
            { ServerPropertyKeys.ServerAutostartEnabled, "ServerAutostartEnabled" },
            { ServerPropertyKeys.BackupEnabled, "BackupEnabled" },
            { ServerPropertyKeys.BackupPath, "BackupPath" },
            { ServerPropertyKeys.BackupCron, "BackupCron" },
            { ServerPropertyKeys.MaxBackupCount, "MaxBackupCount" },
            { ServerPropertyKeys.AutoBackupsContainPacks, "AutoBackupsContainPacks" },
            { ServerPropertyKeys.IgnoreInactiveBackups, "IgnoreInactiveBackups" },
            { ServerPropertyKeys.CheckUpdates, "CheckUpdates" },
            { ServerPropertyKeys.AutoDeployUpdates, "AutoDeployUpdates" },
            { ServerPropertyKeys.UpdateCron, "UpdateCron" },
            { ServerPropertyKeys.SelectedServerVersion, "SelectedServerVersion" },
            { ServerPropertyKeys.DeployedVersion, "DeployedVersion" },
        };

        public static Dictionary<BmsDependServerPropKeys, string> BmsDependServerPropStrings = new() {
            { BmsDependServerPropKeys.ServerName, "server-name" },
            { BmsDependServerPropKeys.PortI4, "server-port" },
            { BmsDependServerPropKeys.PortI6, "server-portv6" },
            { BmsDependServerPropKeys.LevelName, "level-name" },
            { BmsDependServerPropKeys.PermLevel, "default-player-permission-level" }
        };

        public static Dictionary<MinecraftPackTypes, string> MinecraftPackTypeStrings = new() {
            { MinecraftPackTypes.data, "data" },
            { MinecraftPackTypes.resources, "resources" },
            { MinecraftPackTypes.WorldPack, "WorldPack" }
        };

        public static void SetWorkingDirectory(IProcessInfo processInfo) {
            if (!BmsDirectoryStrings.ContainsKey(BmsDirectoryKeys.WorkingDirectory)) {
                BmsDirectoryStrings.Add(BmsDirectoryKeys.WorkingDirectory, processInfo.GetDirectory());
            } else {
                BmsDirectoryStrings[BmsDirectoryKeys.WorkingDirectory] = processInfo.GetDirectory();
            }
        }

        public static string GetServiceDirectory(BmsDirectoryKeys key) {
            if (!BmsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BmsDirectoryStrings[key] != string.Empty ?
    $@"{BmsDirectoryStrings[BmsDirectoryKeys.WorkingDirectory]}\{BmsDirectoryStrings[key]}" :
    $@"{BmsDirectoryStrings[BmsDirectoryKeys.WorkingDirectory]}";
        }

        public static string GetServiceDirectory(BmsDirectoryKeys key, object var0) {
            if (!BmsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            string compile = BmsDirectoryStrings[key] != string.Empty ?
    $@"{BmsDirectoryStrings[BmsDirectoryKeys.WorkingDirectory]}\{BmsDirectoryStrings[key]}" :
    $@"{BmsDirectoryStrings[BmsDirectoryKeys.WorkingDirectory]}";
            return string.Format(compile, var0);
        }

        public static string GetServiceFileName(BmsFileNameKeys key) {
            if (!BmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return BmsFileNameStrings[key];
        }

        public static string GetServiceFilePath(BmsFileNameKeys key) {
            if (!BmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return $"{GetServiceDirectory(BmsFileParentDirectories[key])}\\{BmsFileNameStrings[key]}";
        }

        public static string GetServiceFilePath(BmsFileNameKeys key, object? var0) {
            if (!BmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException();
            }
            string output = $"{GetServiceDirectory(BmsFileParentDirectories[key])}\\{BmsFileNameStrings[key]}";
            return string.Format(output, var0);
        }

        public static string GetServerFileName(BdsFileNameKeys key) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return BdsFileNameStrings[key];
        }

        public static string GetServerDirectory(BdsDirectoryKeys key, IServerConfiguration server) {
            if (!BdsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{BdsDirectoryStrings[key]}" :
                $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}";
        }

        public static string GetServerDirectory(BdsDirectoryKeys key, string serverPath) {
            if (!BdsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                $@"{serverPath}\{BdsDirectoryStrings[key]}" :
                $@"{serverPath}";
        }

        public static string GetServerFilePath(BdsFileNameKeys key, IServerConfiguration server) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return $"{GetServerDirectory(BdsFileParentDirectories[key], server)}\\{BdsFileNameStrings[key]}";
        }

        public static string GetServerFilePath(BdsFileNameKeys key, IServerConfiguration server, object var0) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return string.Format($@"{GetServerDirectory(BdsFileParentDirectories[key], server)}\{BdsFileNameStrings[key]}", var0);
        }

        public static string GetServerFilePath(BdsFileNameKeys key, string serverPath) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return $@"{GetServerDirectory(BdsFileParentDirectories[key], serverPath)}\{BdsFileNameStrings[key]}";
        }
    }
}
