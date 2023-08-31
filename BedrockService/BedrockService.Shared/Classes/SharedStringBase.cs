using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
#nullable enable

namespace BedrockService.Shared.Classes {
    public static class SharedStringBase {

        public static JsonSerializerSettings GlobalJsonSerialierSettings = new() { TypeNameHandling = TypeNameHandling.All, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

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

        public enum MmsDependServerPropKeys {
            ServerName,
            PortI4,
            PortI6,
            LevelName,
            PermLevel
        }

        public enum MmsFileNameKeys {
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

        public enum ServiceDirectoryKeys {
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
            Jdk17Path,
            Jdk17BinPath
        }

        public enum ServerFileNameKeys {
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

        public enum ServerDirectoryKeys {
            ServerRoot,
            ServerWorldDir_LevelName,
            ResourcePacksDir,
            BehaviorPacksDir,
            LLPlugins,
            LLConfigDir
        }

        public enum MmsUrlKeys {
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

        public static Dictionary<MmsFileNameKeys, ServiceDirectoryKeys> BmsFileParentDirectories = new() {
            { MmsFileNameKeys.ServiceConfig, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.ClientConfig, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.ServerConfig_Name, ServiceDirectoryKeys.ServerConfigs },
            { MmsFileNameKeys.BmsVersionIni, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.LatestVerIni_Name, ServiceDirectoryKeys.BmsConfig },
            { MmsFileNameKeys.BdsUpdatePackage_Ver, ServiceDirectoryKeys.BuildArchives },
            { MmsFileNameKeys.JdsUpdatePackage_Ver, ServiceDirectoryKeys.JavaCoreFiles_Ver },
            { MmsFileNameKeys.LLUpdatePackage_Ver, ServiceDirectoryKeys.LLBuilds },
            { MmsFileNameKeys.LLModUpdatePackage_Ver, ServiceDirectoryKeys.LLBuilds },
            { MmsFileNameKeys.BedrockStockProps_Ver, ServiceDirectoryKeys.BedrockCoreFiles_Ver },
            { MmsFileNameKeys.JavaStockProps_Ver, ServiceDirectoryKeys.JavaCoreFiles_Ver },
            { MmsFileNameKeys.ServerPlayerRegistry_Name, ServiceDirectoryKeys.ServerPlayerPath },
            { MmsFileNameKeys.GlobalPlayerRegistry, ServiceDirectoryKeys.GlobalPlayerPath },
            { MmsFileNameKeys.ServerPlayerTelem_Name, ServiceDirectoryKeys.ServerPlayerPath },
            { MmsFileNameKeys.GlobalPlayerTelem, ServiceDirectoryKeys.GlobalPlayerPath },
            { MmsFileNameKeys.Jdk17JavaVanillaExe, ServiceDirectoryKeys.Jdk17BinPath },
            { MmsFileNameKeys.Jdk17JavaMmsExe, ServiceDirectoryKeys.Jdk17BinPath }
        };

        public static Dictionary<ServerFileNameKeys, ServerDirectoryKeys> BdsFileParentDirectories = new() {
            { ServerFileNameKeys.BmsServer_Name, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.JavaServer_Name, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.VanillaBedrock, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.VanillaJava, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.ServerProps, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.DeployedINI, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.ValidKnownPacks, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.WorldBehaviorPacks, ServerDirectoryKeys.ServerWorldDir_LevelName },
            { ServerFileNameKeys.WorldResourcePacks, ServerDirectoryKeys.ServerWorldDir_LevelName },
            { ServerFileNameKeys.AllowList, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.WhiteList, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.PermList, ServerDirectoryKeys.ServerRoot },
            { ServerFileNameKeys.LevelDat, ServerDirectoryKeys.ServerWorldDir_LevelName },
            { ServerFileNameKeys.LLConfig, ServerDirectoryKeys.LLConfigDir },
            { ServerFileNameKeys.JavaEula, ServerDirectoryKeys.ServerRoot }
        };


        public static Dictionary<MinecraftServerArch, string> MinecraftArchStrings = new() {
            { MinecraftServerArch.Bedrock, "Bedrock" },
            { MinecraftServerArch.LiteLoader, "LiteLoader" },
            { MinecraftServerArch.Java, "Java" }
        };

        public static MinecraftServerArch GetArchFromString(string archName) {
            switch (archName) {
                case "Bedrock":
                    return MinecraftServerArch.Bedrock;
                case "LiteLoader":
                    return MinecraftServerArch.LiteLoader;
                case "Java":
                    return MinecraftServerArch.Java;
            }
            return MinecraftServerArch.Bedrock;
        }

        public static Dictionary<ServerDirectoryKeys, string> BdsDirectoryStrings = new() {
            { ServerDirectoryKeys.ServerRoot, "" },
            { ServerDirectoryKeys.ServerWorldDir_LevelName, "worlds\\{0}" },
            { ServerDirectoryKeys.ResourcePacksDir, "development_resource_packs" },
            { ServerDirectoryKeys.BehaviorPacksDir, "development_behavior_packs" },
            { ServerDirectoryKeys.LLPlugins, "plugins" },
            { ServerDirectoryKeys.LLConfigDir, "plugins\\LiteLoader" }
        };

        public static Dictionary<MmsUrlKeys, string> BmsUrlStrings = new() {
            { MmsUrlKeys.BdsDownloadPage, "https://www.minecraft.net/en-us/download/server/bedrock" },
            { MmsUrlKeys.BdsPackage_Ver, "https://minecraft.azureedge.net/bin-win/bedrock-server-{0}.zip" },
            { MmsUrlKeys.BdsVersionRegx, @"(https://minecraft.azureedge.net/bin-win/bedrock-server-)(.*)(\.zip)" },
            { MmsUrlKeys.LLPackageOld_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/LiteLoader-{0}.zip" },
            { MmsUrlKeys.LLPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/LiteLoaderBDS.zip" },
            { MmsUrlKeys.LLModPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDS/releases/download/{0}/Modules.zip" },
            { MmsUrlKeys.LLReleasesJson, "https://github.com/crowbarmaster/BedrockManagementService/raw/master/BMS_Files/liteloader_version_manifest.json" },
            { MmsUrlKeys.BdsVersionJson, "https://github.com/crowbarmaster/BedrockManagementService/raw/master/BMS_Files/bedrock_version_manifest.json" },
            { MmsUrlKeys.JdsVersionJson, "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json" },
            { MmsUrlKeys.PluginRepoJson, "https://github.com/crowbarmaster/BedrockManagementService/raw/master/BMS_Files/plugin_repo.json"},
            { MmsUrlKeys.Jdk17DownloadLink, "https://download.oracle.com/java/17/latest/jdk-17_windows-x64_bin.zip"}
        };

        public static Dictionary<ServiceDirectoryKeys, string> BmsDirectoryStrings = new() {
            { ServiceDirectoryKeys.Root, "" },
            { ServiceDirectoryKeys.BmsConfig, "BmsConfig" },
            { ServiceDirectoryKeys.ServerConfigs, "BmsConfig\\ServerConfigs" },
            { ServiceDirectoryKeys.ServerPlayerPath, "BmsConfig\\ServerConfigs\\PlayerRecords" },
            { ServiceDirectoryKeys.GlobalPlayerPath, "BmsConfig\\ServerConfigs\\GlobalPlayers" },
            { ServiceDirectoryKeys.LLBuilds, "BmsConfig\\LLBuilds" },
            { ServiceDirectoryKeys.JavaBuilds, "BmsConfig\\JavaBuilds" },
            { ServiceDirectoryKeys.BdsBuilds, "BmsConfig\\BDSBuilds" },
            { ServiceDirectoryKeys.BuildArchives, "BmsConfig\\BDSBuilds\\BuildArchives" },
            { ServiceDirectoryKeys.JavaCoreFiles_Ver, "BmsConfig\\JavaBuilds\\Build_{0}" },
            { ServiceDirectoryKeys.BedrockCoreFiles_Ver, "BmsConfig\\BDSBuilds\\CoreFiles\\Build_{0}" },
            { ServiceDirectoryKeys.Jdk17Path, "Jdk17" },
            { ServiceDirectoryKeys.Jdk17BinPath, "Jdk17\\bin" }
        };

        public static Dictionary<MmsFileNameKeys, string> BmsFileNameStrings = new() {
            { MmsFileNameKeys.ServiceConfig, "Service.conf" },
            { MmsFileNameKeys.ClientConfig, "Client.conf" },
            { MmsFileNameKeys.ServerConfig_Name, "{0}.conf" },
            { MmsFileNameKeys.LatestVerIni_Name, "LatestVer-{0}.ini" },
            { MmsFileNameKeys.BmsVersionIni, "ServiceVersion.ini" },
            { MmsFileNameKeys.BedrockStockProps_Ver, "stock_props.conf" },
            { MmsFileNameKeys.JavaStockProps_Ver, "server.properties" },
            { MmsFileNameKeys.GlobalPlayerRegistry, "Service.preg" },
            { MmsFileNameKeys.ServerPlayerRegistry_Name, "{0}.preg" },
            { MmsFileNameKeys.GlobalPlayerTelem, "Service.playerdb" },
            { MmsFileNameKeys.ServerPlayerTelem_Name, "{0}.playerdb" },
            { MmsFileNameKeys.BdsUpdatePackage_Ver, "Update_{0}.zip" },
            { MmsFileNameKeys.JdsUpdatePackage_Ver, "server.jar" },
            { MmsFileNameKeys.LLUpdatePackage_Ver, "LLUpdate_{0}.zip" },
            { MmsFileNameKeys.LLModUpdatePackage_Ver, "LLModUpdate_{0}.zip" },
            { MmsFileNameKeys.Jdk17JavaVanillaExe, "javaw.exe" },
            { MmsFileNameKeys.Jdk17JavaMmsExe, "MmsServerInstance.exe" }
        };

        public static Dictionary<ServerFileNameKeys, string> BdsFileNameStrings = new() {
            { ServerFileNameKeys.BmsServer_Name, "BedrockService.{0}.exe" },
            { ServerFileNameKeys.JavaServer_Name, "BedrockService.{0}.jar" },
            { ServerFileNameKeys.VanillaBedrock, "bedrock_server.exe" },
            { ServerFileNameKeys.VanillaJava, "server.jar" },
            { ServerFileNameKeys.ValidKnownPacks, "valid_known_packs.json" },
            { ServerFileNameKeys.DeployedINI, "DeployedVer.ini" },
            { ServerFileNameKeys.LevelDat, "level.dat" },
            { ServerFileNameKeys.ServerProps, "server.properties" },
            { ServerFileNameKeys.WorldResourcePacks, "world_resource_packs.json" },
            { ServerFileNameKeys.WorldBehaviorPacks, "world_behavior_packs.json" },
            { ServerFileNameKeys.AllowList, "allowlist.json" },
            { ServerFileNameKeys.WhiteList, "whitelist.json" },
            { ServerFileNameKeys.PermList, "permissions.json" },
            { ServerFileNameKeys.LLConfig, "LiteLoader.json" },
            { ServerFileNameKeys.JavaEula, "eula.txt" }
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

        public static Dictionary<MmsDependServerPropKeys, string> BmsDependServerPropStrings = new() {
            { MmsDependServerPropKeys.ServerName, "server-name" },
            { MmsDependServerPropKeys.PortI4, "server-port" },
            { MmsDependServerPropKeys.PortI6, "server-portv6" },
            { MmsDependServerPropKeys.LevelName, "level-name" },
            { MmsDependServerPropKeys.PermLevel, "default-player-permission-level" }
        };

        public static Dictionary<MinecraftPackTypes, string> MinecraftPackTypeStrings = new() {
            { MinecraftPackTypes.Behavior, "data" },
            { MinecraftPackTypes.Resource, "resources" },
            { MinecraftPackTypes.WorldPack, "WorldPack" }
        };

        public static void SetWorkingDirectory(IProcessInfo processInfo) {
            if (!BmsDirectoryStrings.ContainsKey(ServiceDirectoryKeys.WorkingDirectory)) {
                BmsDirectoryStrings.Add(ServiceDirectoryKeys.WorkingDirectory, processInfo.GetDirectory());
            } else {
                BmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory] = processInfo.GetDirectory();
            }
        }

        public static string GetServiceDirectory(ServiceDirectoryKeys key) {
            if (!BmsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BmsDirectoryStrings[key] != string.Empty ?
    $@"{BmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}\{BmsDirectoryStrings[key]}" :
    $@"{BmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}";
        }

        public static string GetServiceDirectory(ServiceDirectoryKeys key, object var0) {
            if (!BmsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            string compile = BmsDirectoryStrings[key] != string.Empty ?
    $@"{BmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}\{BmsDirectoryStrings[key]}" :
    $@"{BmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}";
            return string.Format(compile, var0);
        }

        public static string GetServiceFileName(MmsFileNameKeys key) {
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

        public static string GetServiceFileName(MmsFileNameKeys key, object var0) {
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

        public static string GetServiceFilePath(MmsFileNameKeys key) {
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

        public static string GetServiceFilePath(MmsFileNameKeys key, object? var0) {
            if (!BmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException();
            }
            string output = $"{GetServiceDirectory(BmsFileParentDirectories[key])}\\{BmsFileNameStrings[key]}";
            return string.Format(output, var0);
        }

        public static string GetServerFileName(ServerFileNameKeys key) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return BdsFileNameStrings[key];
        }

        public static string GetServerDirectory(ServerDirectoryKeys key, IServerConfiguration server) {
            if (!BdsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{BdsDirectoryStrings[key]}" :
                $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}";
        }

        public static string GetServerDirectory(ServerDirectoryKeys key, string serverPath) {
            if (!BdsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                $@"{serverPath}\{BdsDirectoryStrings[key]}" :
                $@"{serverPath}";
        }

        public static string GetServerDirectory(ServerDirectoryKeys key, string serverPath, object? var0) {
            if (!BdsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                String.Format($@"{serverPath}\{BdsDirectoryStrings[key]}", var0) :
                String.Format($@"{serverPath}", var0);
        }

        public static string GetServerFilePath(ServerFileNameKeys key, IServerConfiguration server) {
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

        public static string GetServerFilePath(ServerFileNameKeys key, IServerConfiguration server, object var0) {
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

        public static string GetServerFilePath(ServerFileNameKeys key, string serverPath) {
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
