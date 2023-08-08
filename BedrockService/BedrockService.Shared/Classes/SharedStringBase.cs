using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
#nullable enable

namespace BedrockService.Shared.Classes {
    public static class SharedStringBase {

        public enum MinecraftServerArch {
            Bedrock,
            LiteLoader,
            Java
        }

        public enum ServicePropertyKeys {
            ServersPath,
            AcceptedMojangLic,
            ClientPort,
            LogServerOutput,
            LogApplicationOutput,
            TimestampLogEntries,
            GlobalizedPlayerDatabase,
            DefaultGlobalPermLevel,
            UseBetaLiteLoaderVersions
        };

        public enum ServerPropertyKeys {
            MinecraftType,
            ServerName,
            FileName,
            ServerPath,
            ServerExeName,
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
            ServerVersion,
            UseBetaVersions,
            JavaArgs
        }

        public enum MinecraftPackTypes {
            Behavior,
            Resource,
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
            LatestVerIni_Name,
            BdsUpdatePackage_Ver,
            JdsUpdatePackage_Ver,
            LLUpdatePackage_Ver,
            LLModUpdatePackage_Ver,
            BedrockStockProps_Ver,
            JavaStockProps_Ver,
            ServerPlayerRegistry_Name,
            ServerPlayerTelem_Name,
            GlobalPlayerRegistry,
            GlobalPlayerTelem,
            Jdk17JavaVanillaExe,
            Jdk17JavaMmsExe
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
            JavaBuilds,
            LLBuilds,
            BuildArchives,
            CoreFiles,
            BedrockCoreFiles_Ver,
            JavaCoreFiles_Ver,
            Jdk20Path,
            Jdk17BinPath
        }

        public enum BdsFileNameKeys {
            BmsServer_Name,
            JavaServer_Name,
            DeployedINI,
            ValidKnownPacks,
            LevelDat,
            VanillaBedrock,
            VanillaJava,
            ServerProps,
            WorldResourcePacks,
            WorldBehaviorPacks,
            AllowList,
            WhiteList,
            PermList,
            LLConfig,
            JavaEula
        }

        public enum BdsDirectoryKeys {
            ServerRoot,
            ServerWorldDir_LevelName,
            ResourcePacksDir,
            BehaviorPacksDir,
            LLPlugins,
            LLConfigDir
        }

        public enum BmsUrlKeys {
            BdsDownloadPage,
            LLReleasesJson,
            BdsVersionRegx,
            BdsPackage_Ver,
            LLPackageOld_Ver,
            LLPackage_Ver,
            LLModPackage_Ver,
            LLBdsVersionRegx,
            LLBdsVersionRegxNew,
            BdsVersionJson,
            JdsVersionJson,
            PluginRepoJson,
            Jdk17DownloadLink
        }

        public static Dictionary<BmsFileNameKeys, BmsDirectoryKeys> BmsFileParentDirectories = new() {
            { BmsFileNameKeys.ServiceConfig, BmsDirectoryKeys.Root },
            { BmsFileNameKeys.ClientConfig, BmsDirectoryKeys.Root },
            { BmsFileNameKeys.ServerConfig_Name, BmsDirectoryKeys.ServerConfigs },
            { BmsFileNameKeys.BmsVersionIni, BmsDirectoryKeys.Root },
            { BmsFileNameKeys.LatestVerIni_Name, BmsDirectoryKeys.BmsConfig },
            { BmsFileNameKeys.BdsUpdatePackage_Ver, BmsDirectoryKeys.BuildArchives },
            { BmsFileNameKeys.JdsUpdatePackage_Ver, BmsDirectoryKeys.JavaCoreFiles_Ver },
            { BmsFileNameKeys.LLUpdatePackage_Ver, BmsDirectoryKeys.LLBuilds },
            { BmsFileNameKeys.LLModUpdatePackage_Ver, BmsDirectoryKeys.LLBuilds },
            { BmsFileNameKeys.BedrockStockProps_Ver, BmsDirectoryKeys.BedrockCoreFiles_Ver },
            { BmsFileNameKeys.JavaStockProps_Ver, BmsDirectoryKeys.JavaCoreFiles_Ver },
            { BmsFileNameKeys.ServerPlayerRegistry_Name, BmsDirectoryKeys.ServerPlayerPath },
            { BmsFileNameKeys.GlobalPlayerRegistry, BmsDirectoryKeys.GlobalPlayerPath },
            { BmsFileNameKeys.ServerPlayerTelem_Name, BmsDirectoryKeys.ServerPlayerPath },
            { BmsFileNameKeys.GlobalPlayerTelem, BmsDirectoryKeys.GlobalPlayerPath },
            { BmsFileNameKeys.Jdk17JavaVanillaExe, BmsDirectoryKeys.Jdk17BinPath },
            { BmsFileNameKeys.Jdk17JavaMmsExe, BmsDirectoryKeys.Jdk17BinPath }
        };

        public static Dictionary<BdsFileNameKeys, BdsDirectoryKeys> BdsFileParentDirectories = new() {
            { BdsFileNameKeys.BmsServer_Name, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.JavaServer_Name, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.VanillaBedrock, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.VanillaJava, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.ServerProps, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.DeployedINI, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.ValidKnownPacks, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.WorldBehaviorPacks, BdsDirectoryKeys.ServerWorldDir_LevelName },
            { BdsFileNameKeys.WorldResourcePacks, BdsDirectoryKeys.ServerWorldDir_LevelName },
            { BdsFileNameKeys.AllowList, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.WhiteList, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.PermList, BdsDirectoryKeys.ServerRoot },
            { BdsFileNameKeys.LevelDat, BdsDirectoryKeys.ServerWorldDir_LevelName },
            { BdsFileNameKeys.LLConfig, BdsDirectoryKeys.LLConfigDir },
            { BdsFileNameKeys.JavaEula, BdsDirectoryKeys.ServerRoot }
        };


        public static Dictionary<MinecraftServerArch, string> MinecraftArchStrings = new() {
            { MinecraftServerArch.Bedrock, "Bedrock" },
            { MinecraftServerArch.LiteLoader, "LiteLoader" },
            { MinecraftServerArch.Java, "Java" }
        };

        public static Dictionary<BdsDirectoryKeys, string> BdsDirectoryStrings = new() {
            { BdsDirectoryKeys.ServerRoot, "" },
            { BdsDirectoryKeys.ServerWorldDir_LevelName, "worlds\\{0}" },
            { BdsDirectoryKeys.ResourcePacksDir, "development_resource_packs" },
            { BdsDirectoryKeys.BehaviorPacksDir, "development_behavior_packs" },
            { BdsDirectoryKeys.LLPlugins, "plugins" },
            { BdsDirectoryKeys.LLConfigDir, "plugins\\LiteLoader" }
        };

        public static Dictionary<BmsUrlKeys, string> BmsUrlStrings = new() {
            { BmsUrlKeys.BdsDownloadPage, "https://www.minecraft.net/en-us/download/server/bedrock" },
            { BmsUrlKeys.BdsPackage_Ver, "https://minecraft.azureedge.net/bin-win/bedrock-server-{0}.zip" },
            { BmsUrlKeys.BdsVersionRegx, @"(https://minecraft.azureedge.net/bin-win/bedrock-server-)(.*)(\.zip)" },
            { BmsUrlKeys.LLPackageOld_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/LiteLoader-{0}.zip" },
            { BmsUrlKeys.LLPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/LiteLoaderBDS.zip" },
            { BmsUrlKeys.LLModPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/Modules.zip" },
            { BmsUrlKeys.LLReleasesJson, "https://github.com/crowbarmaster/BedrockManagementService/raw/master/BMS_Files/liteloader_version_manifest.json" },
            { BmsUrlKeys.BdsVersionJson, "https://github.com/crowbarmaster/BedrockManagementService/raw/master/BMS_Files/bedrock_version_manifest.json" },
            { BmsUrlKeys.JdsVersionJson, "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json" },
            { BmsUrlKeys.PluginRepoJson, "https://github.com/crowbarmaster/BedrockManagementService/raw/master/BMS_Files/plugin_repo.json"},
            { BmsUrlKeys.Jdk17DownloadLink, "https://download.oracle.com/java/17/latest/jdk-17_windows-x64_bin.zip"}
        };

        public static Dictionary<BmsDirectoryKeys, string> BmsDirectoryStrings = new() {
            { BmsDirectoryKeys.Root, "" },
            { BmsDirectoryKeys.BmsConfig, "BmsConfig" },
            { BmsDirectoryKeys.ServerConfigs, "BmsConfig\\ServerConfigs" },
            { BmsDirectoryKeys.ServerPlayerPath, "BmsConfig\\ServerConfigs\\PlayerRecords" },
            { BmsDirectoryKeys.GlobalPlayerPath, "BmsConfig\\ServerConfigs\\GlobalPlayers" },
            { BmsDirectoryKeys.LLBuilds, "BmsConfig\\LLBuilds" },
            { BmsDirectoryKeys.JavaBuilds, "BmsConfig\\JavaBuilds" },
            { BmsDirectoryKeys.BdsBuilds, "BmsConfig\\BDSBuilds" },
            { BmsDirectoryKeys.BuildArchives, "BmsConfig\\BDSBuilds\\BuildArchives" },
            { BmsDirectoryKeys.JavaCoreFiles_Ver, "BmsConfig\\JavaBuilds\\Build_{0}" },
            { BmsDirectoryKeys.BedrockCoreFiles_Ver, "BmsConfig\\BDSBuilds\\CoreFiles\\Build_{0}" },
            { BmsDirectoryKeys.Jdk20Path, "Jdk17" },
            { BmsDirectoryKeys.Jdk17BinPath, "Jdk17\\bin" }
        };

        public static Dictionary<BmsFileNameKeys, string> BmsFileNameStrings = new() {
            { BmsFileNameKeys.ServiceConfig, "Service.conf" },
            { BmsFileNameKeys.ClientConfig, "Client.conf" },
            { BmsFileNameKeys.ServerConfig_Name, "{0}.conf" },
            { BmsFileNameKeys.LatestVerIni_Name, "LatestVer-{0}.ini" },
            { BmsFileNameKeys.BmsVersionIni, "ServiceVersion.ini" },
            { BmsFileNameKeys.BedrockStockProps_Ver, "stock_props.conf" },
            { BmsFileNameKeys.JavaStockProps_Ver, "server.properties" },
            { BmsFileNameKeys.GlobalPlayerRegistry, "Service.preg" },
            { BmsFileNameKeys.ServerPlayerRegistry_Name, "{0}.preg" },
            { BmsFileNameKeys.GlobalPlayerTelem, "Service.playerdb" },
            { BmsFileNameKeys.ServerPlayerTelem_Name, "{0}.playerdb" },
            { BmsFileNameKeys.BdsUpdatePackage_Ver, "Update_{0}.zip" },
            { BmsFileNameKeys.JdsUpdatePackage_Ver, "server.jar" },
            { BmsFileNameKeys.LLUpdatePackage_Ver, "LLUpdate_{0}.zip" },
            { BmsFileNameKeys.LLModUpdatePackage_Ver, "LLModUpdate_{0}.zip" },
            { BmsFileNameKeys.Jdk17JavaVanillaExe, "javaw.exe" },
            { BmsFileNameKeys.Jdk17JavaMmsExe, "MmsServerInstance.exe" }
        };

        public static Dictionary<BdsFileNameKeys, string> BdsFileNameStrings = new() {
            { BdsFileNameKeys.BmsServer_Name, "BedrockService.{0}.exe" },
            { BdsFileNameKeys.JavaServer_Name, "BedrockService.{0}.jar" },
            { BdsFileNameKeys.VanillaBedrock, "bedrock_server.exe" },
            { BdsFileNameKeys.VanillaJava, "server.jar" },
            { BdsFileNameKeys.ValidKnownPacks, "valid_known_packs.json" },
            { BdsFileNameKeys.DeployedINI, "DeployedVer.ini" },
            { BdsFileNameKeys.LevelDat, "level.dat" },
            { BdsFileNameKeys.ServerProps, "server.properties" },
            { BdsFileNameKeys.WorldResourcePacks, "world_resource_packs.json" },
            { BdsFileNameKeys.WorldBehaviorPacks, "world_behavior_packs.json" },
            { BdsFileNameKeys.AllowList, "allowlist.json" },
            { BdsFileNameKeys.WhiteList, "whitelist.json" },
            { BdsFileNameKeys.PermList, "permissions.json" },
            { BdsFileNameKeys.LLConfig, "LiteLoader.json" },
            { BdsFileNameKeys.JavaEula, "eula.txt" }
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
        };

        public static Dictionary<ServerPropertyKeys, string> ServerPropertyStrings = new() {
            { ServerPropertyKeys.MinecraftType, "MinecraftType" },
            { ServerPropertyKeys.ServerName, "ServerName" },
            { ServerPropertyKeys.FileName, "FileName" },
            { ServerPropertyKeys.ServerPath, "ServerPath" },
            { ServerPropertyKeys.ServerExeName, "ServerExeName" },
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
            { ServerPropertyKeys.ServerVersion, "ServerVersion" },
            { ServerPropertyKeys.UseBetaVersions, "UseBetaVersions" },
            { ServerPropertyKeys.JavaArgs, "JavaArgs" }
        };

        public static Dictionary<BmsDependServerPropKeys, string> BmsDependServerPropStrings = new() {
            { BmsDependServerPropKeys.ServerName, "server-name" },
            { BmsDependServerPropKeys.PortI4, "server-port" },
            { BmsDependServerPropKeys.PortI6, "server-portv6" },
            { BmsDependServerPropKeys.LevelName, "level-name" },
            { BmsDependServerPropKeys.PermLevel, "default-player-permission-level" }
        };

        public static Dictionary<MinecraftPackTypes, string> MinecraftPackTypeStrings = new() {
            { MinecraftPackTypes.Behavior, "data" },
            { MinecraftPackTypes.Resource, "resources" },
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
            if (!BmsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BmsDirectoryStrings.ContainsKey(BmsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return BmsFileNameStrings[key];
        }

        public static string GetServiceFileName(BmsFileNameKeys key, object var0) {
            if (!BmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BmsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BmsDirectoryStrings.ContainsKey(BmsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format(BmsFileNameStrings[key], var0);
        }

        public static string GetServiceFilePath(BmsFileNameKeys key) {
            if (!BmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            if (!BmsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BmsDirectoryStrings.ContainsKey(BmsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BmsFileParentDirectories[key]} does not have a directory associated.");
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

        public static string GetServerDirectory(BdsDirectoryKeys key, string serverPath, object? var0) {
            if (!BdsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                String.Format($@"{serverPath}\{BdsDirectoryStrings[key]}", var0) :
                String.Format($@"{serverPath}", var0);
        }

        public static string GetServerFilePath(BdsFileNameKeys key, IServerConfiguration server) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return $"{GetServerDirectory(BdsFileParentDirectories[key], server)}\\{BdsFileNameStrings[key]}";
        }

        public static string GetServerFilePath(BdsFileNameKeys key, IServerConfiguration server, object var0) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format($@"{GetServerDirectory(BdsFileParentDirectories[key], server)}\{BdsFileNameStrings[key]}", var0);
        }

        public static string GetServerFilePath(BdsFileNameKeys key, string serverPath) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return $@"{GetServerDirectory(BdsFileParentDirectories[key], serverPath)}\{BdsFileNameStrings[key]}";
        }
    }
}
