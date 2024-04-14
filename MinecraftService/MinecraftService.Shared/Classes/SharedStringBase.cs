using MinecraftService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
#nullable enable

namespace MinecraftService.Shared.Classes {
    public static class SharedStringBase {

        public static JsonSerializerSettings GlobalJsonSerialierSettings = new() { TypeNameHandling = TypeNameHandling.All, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public enum MinecraftServerArch {
            Bedrock,
            LiteLoader,
            Java
        }

        public enum MmsTimerTypes {
            Update,
            Backup,
            Restart
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
            AutoBackupEnabled,
            AutoRestartEnabled,
            CheckUpdatesEnabled,
            BackupPath,
            BackupCron,
            RestartCron,
            UpdateCron,
            MaxBackupCount,
            AutoBackupsContainPacks,
            IgnoreInactiveBackups,
            AutoDeployUpdates,
            ServerVersion,
            UseBetaVersions,
            JavaArgs,
            UseErrorFilter
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
            ClientCommandHistory,
            ServerConfig_Name,
            MmsVersionIni,
            LatestVerIni_Name,
            VersionManifest_Name,
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
            Jdk17JavaMmsExe,
            ServerErrorFilter
        }

        public enum ServiceDirectoryKeys {
            WorkingDirectory,
            Root,
            MmsLogs,
            MmsConfig,
            ServerConfigs,
            ServerPlayerPath,
            GlobalPlayerPath,
            BdsBuilds,
            JavaBuilds,
            LLBuilds,
            BuildArchives,
            BedrockCoreFiles_Ver,
            JavaCoreFiles_Ver,
            Jdk17Path,
            Jdk17BinPath
        }

        public enum ServerFileNameKeys {
            MmsServer_Name,
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

        public static Dictionary<MmsFileNameKeys, ServiceDirectoryKeys> MmsFileParentDirectories = new() {
            { MmsFileNameKeys.ServiceConfig, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.ClientConfig, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.ClientCommandHistory, ServiceDirectoryKeys.MmsConfig },
            { MmsFileNameKeys.ServerConfig_Name, ServiceDirectoryKeys.ServerConfigs },
            { MmsFileNameKeys.MmsVersionIni, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.LatestVerIni_Name, ServiceDirectoryKeys.MmsConfig },
            { MmsFileNameKeys.VersionManifest_Name, ServiceDirectoryKeys.MmsConfig },
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
            { MmsFileNameKeys.Jdk17JavaMmsExe, ServiceDirectoryKeys.Jdk17BinPath },
            { MmsFileNameKeys.ServerErrorFilter, ServiceDirectoryKeys.MmsConfig }
        };

        public static Dictionary<ServerFileNameKeys, ServerDirectoryKeys> BdsFileParentDirectories = new() {
            { ServerFileNameKeys.MmsServer_Name, ServerDirectoryKeys.ServerRoot },
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

        public static string GetNewTempDirectory(string folderName) => $"{Path.GetTempPath()}MMSTemp\\{folderName}_{Guid.NewGuid()}";

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

        public static Dictionary<MmsUrlKeys, string> MmsUrlStrings = new() {
            { MmsUrlKeys.BdsDownloadPage, "https://www.minecraft.net/en-us/download/server/bedrock" },
            { MmsUrlKeys.BdsPackage_Ver, "https://minecraft.azureedge.net/bin-win/bedrock-server-{0}.zip" },
            { MmsUrlKeys.BdsVersionRegx, @"(https://minecraft.azureedge.net/bin-win/bedrock-server-)(.*)(\.zip)" },
            { MmsUrlKeys.LLPackageOld_Ver, "https://github.com/LiteLDev/LiteLoaderBDSv2/releases/download/{0}/LiteLoader-{0}.zip" },
            { MmsUrlKeys.LLPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDSv2/releases/download/{0}/LiteLoaderBDS.zip" },
            { MmsUrlKeys.LLModPackage_Ver, "https://github.com/LiteLDev/LiteLoaderBDSv2/releases/download/{0}/Modules.zip" },
            { MmsUrlKeys.LLReleasesJson, "https://raw.githubusercontent.com/crowbarmaster/BedrockManagementService/master/BMS_Files/liteloader_version_manifest.json" },
            { MmsUrlKeys.BdsVersionJson, "https://raw.githubusercontent.com/crowbarmaster/BedrockManagementService/master/MMS_Files/bedrock_version_prop_manifest.json" },
            { MmsUrlKeys.JdsVersionJson, "https://raw.githubusercontent.com/crowbarmaster/BedrockManagementService/master/MMS_Files/java_version_prop_manifest.json" },
            { MmsUrlKeys.Jdk17DownloadLink, "https://download.oracle.com/java/17/latest/jdk-17_windows-x64_bin.zip"}
        };

        public static Dictionary<ServiceDirectoryKeys, string> MmsDirectoryStrings = new() {
            { ServiceDirectoryKeys.Root, "" },
            { ServiceDirectoryKeys.MmsConfig, "MmsConfig" },
            { ServiceDirectoryKeys.ServerConfigs, "MmsConfig\\ServerConfigs" },
            { ServiceDirectoryKeys.ServerPlayerPath, "MmsConfig\\ServerConfigs\\PlayerRecords" },
            { ServiceDirectoryKeys.GlobalPlayerPath, "MmsConfig\\ServerConfigs\\GlobalPlayers" },
            { ServiceDirectoryKeys.LLBuilds, "MmsConfig\\LLBuilds" },
            { ServiceDirectoryKeys.JavaBuilds, "MmsConfig\\JavaBuilds" },
            { ServiceDirectoryKeys.BdsBuilds, "MmsConfig\\BDSBuilds" },
            { ServiceDirectoryKeys.BuildArchives, "MmsConfig\\BDSBuilds\\BuildArchives" },
            { ServiceDirectoryKeys.JavaCoreFiles_Ver, "MmsConfig\\JavaBuilds\\Build_{0}" },
            { ServiceDirectoryKeys.BedrockCoreFiles_Ver, "MmsConfig\\BDSBuilds\\CoreFiles\\Build_{0}" },
            { ServiceDirectoryKeys.Jdk17Path, "Jdk17" },
            { ServiceDirectoryKeys.Jdk17BinPath, "Jdk17\\bin" }
        };

        public static Dictionary<MmsFileNameKeys, string> MmsFileNameStrings = new() {
            { MmsFileNameKeys.ServiceConfig, "Service.conf" },
            { MmsFileNameKeys.ClientConfig, "Client.conf" },
            { MmsFileNameKeys.ClientCommandHistory, "ClientCommandHistory.txt" },
            { MmsFileNameKeys.ServerConfig_Name, "{0}.conf" },
            { MmsFileNameKeys.LatestVerIni_Name, "LatestVer-{0}.ini" },
            { MmsFileNameKeys.VersionManifest_Name, "VersionPropManifest-{0}.json" },
            { MmsFileNameKeys.MmsVersionIni, "ServiceVersion.ini" },
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
            { MmsFileNameKeys.Jdk17JavaMmsExe, "MmsServerInstance.exe" },
            { MmsFileNameKeys.ServerErrorFilter, "ServerErrorFilters.txt" }
        };

        public static Dictionary<ServerFileNameKeys, string> BdsFileNameStrings = new() {
            { ServerFileNameKeys.MmsServer_Name, "MinecraftService.{0}.exe" },
            { ServerFileNameKeys.JavaServer_Name, "MinecraftService.{0}.jar" },
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
            { ServerPropertyKeys.AutoBackupEnabled, "BackupEnabled" },
            { ServerPropertyKeys.BackupPath, "BackupPath" },
            { ServerPropertyKeys.BackupCron, "BackupCron" },
            { ServerPropertyKeys.MaxBackupCount, "MaxBackupCount" },
            { ServerPropertyKeys.AutoBackupsContainPacks, "AutoBackupsContainPacks" },
            { ServerPropertyKeys.IgnoreInactiveBackups, "IgnoreInactiveBackups" },
            { ServerPropertyKeys.CheckUpdatesEnabled, "CheckUpdates" },
            { ServerPropertyKeys.AutoRestartEnabled, "AutoRestartEnabled" },
            { ServerPropertyKeys.AutoDeployUpdates, "AutoDeployUpdates" },
            { ServerPropertyKeys.UpdateCron, "UpdateCron" },
            { ServerPropertyKeys.RestartCron, "RestartCron" },
            { ServerPropertyKeys.ServerVersion, "ServerVersion" },
            { ServerPropertyKeys.UseBetaVersions, "UseBetaVersions" },
            { ServerPropertyKeys.JavaArgs, "JavaArgs" },
            { ServerPropertyKeys.UseErrorFilter, "UseErrorFilter" }
        };

        public static Dictionary<MmsDependServerPropKeys, string> MmsDependServerPropStrings = new() {
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
            if (!MmsDirectoryStrings.ContainsKey(ServiceDirectoryKeys.WorkingDirectory)) {
                MmsDirectoryStrings.Add(ServiceDirectoryKeys.WorkingDirectory, processInfo.GetDirectory());
            } else {
                MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory] = processInfo.GetDirectory();
            }
        }

        public static string GetServiceDirectory(ServiceDirectoryKeys key) {
            if (!MmsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return MmsDirectoryStrings[key] != string.Empty && key != ServiceDirectoryKeys.WorkingDirectory ?
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}\{MmsDirectoryStrings[key]}" :
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}";
        }

        public static string GetServiceDirectory(ServiceDirectoryKeys key, object var0) {
            if (!MmsDirectoryStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            string compile = MmsDirectoryStrings[key] != string.Empty && key != ServiceDirectoryKeys.WorkingDirectory ?
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}\{MmsDirectoryStrings[key]}" :
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}";
            return string.Format(compile, var0);
        }

        public static string GetServiceFileName(MmsFileNameKeys key) {
            if (!MmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!MmsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!MmsDirectoryStrings.ContainsKey(MmsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {MmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return MmsFileNameStrings[key];
        }

        public static string GetServiceFileName(MmsFileNameKeys key, object var0) {
            if (!MmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!MmsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!MmsDirectoryStrings.ContainsKey(MmsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {MmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format(MmsFileNameStrings[key], var0);
        }

        public static string GetServiceFilePath(MmsFileNameKeys key) {
            if (!MmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            if (!MmsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!MmsDirectoryStrings.ContainsKey(MmsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {MmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return $"{GetServiceDirectory(MmsFileParentDirectories[key])}\\{MmsFileNameStrings[key]}";
        }

        public static string GetServiceFilePath(MmsFileNameKeys key, object? var0) {
            if (!MmsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException();
            }
            string output = $"{GetServiceDirectory(MmsFileParentDirectories[key])}\\{MmsFileNameStrings[key]}";
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

        public static string GetServerFilePath(ServerFileNameKeys key, string serverPath, object var0) {
            if (!BdsFileNameStrings.ContainsKey(key)) {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key)) {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key])) {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format($@"{GetServerDirectory(BdsFileParentDirectories[key], serverPath)}\{BdsFileNameStrings[key]}", var0);
        }
    }
}
