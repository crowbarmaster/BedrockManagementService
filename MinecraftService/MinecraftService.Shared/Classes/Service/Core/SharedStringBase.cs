﻿using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
#nullable enable

namespace MinecraftService.Shared.Classes.Service.Core
{
    public static class SharedStringBase
    {

        public static JsonSerializerSettings GlobalJsonSerialierSettings = new() { TypeNameHandling = TypeNameHandling.All, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public enum MinecraftServerArch
        {
            Bedrock,
            Java
        }

        public enum MmsTimerTypes
        {
            Update,
            Backup,
            Restart
        }

        public enum ServicePropertyKeys
        {
            ServersPath,
            AcceptedMojangLic,
            ClientPort,
            LogServerOutput,
            LogApplicationOutput,
            TimestampLogEntries,
            GlobalizedPlayerDatabase,
            DefaultGlobalPermLevel,
        };

        public enum ServerPropertyKeys
        {
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

        public enum MinecraftPackTypes
        {
            Behavior,
            Resource,
            WorldPack
        };

        public enum MmsDependServerPropKeys
        {
            ServerName,
            PortI4,
            PortI6,
            LevelName,
            PermLevel
        }

        public enum MmsFileNameKeys
        {
            ServiceConfig,
            ClientConfig,
            ClientCommandHistory,
            ServerConfig_ServerName,
            MmsVersionIni,
            LatestVerIni_Name,
            VersionManifest_Name,
            BdsUpdatePackage_Ver,
            JdsUpdatePackage_Ver,
            BedrockStockProps_Ver,
            JavaStockProps_Ver,
            ServerPlayerRegistry_Name,
            ServerPlayerTelem_Name,
            GlobalPlayerRegistry,
            GlobalPlayerTelem,
            JavaVanillaExe,
            JavaMmsExe,
            ServerErrorFilter
        }

        public enum ServiceDirectoryKeys
        {
            WorkingDirectory,
            Root,
            MmsLogs,
            MmsConfig,
            ServerConfigs,
            ServerPlayerPath,
            GlobalPlayerPath,
            BdsBuilds,
            JavaBuilds,
            BuildArchives,
            BedrockCoreFiles_Ver,
            JavaCoreFiles_Ver,
            Jdk21Path,
            Jdk21BinPath
        }

        public enum ServerFileNameKeys
        {
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
            JavaEula
        }

        public enum ServerDirectoryKeys
        {
            ServerRoot,
            ServerWorldDir_LevelName,
            ResourcePacksDir_LevelName,
            BehaviorPacksDir_LevelName,
        }

        public enum MmsUrlKeys
        {
            BdsDownloadPage,
            BdsVersionRegx,
            BdsPackage_Ver,
            BdsVersionJson,
            JdsVersionJson,
            PluginRepoJson,
            Jdk21DownloadLink
        }

        public static Dictionary<MmsFileNameKeys, ServiceDirectoryKeys> MmsFileParentDirectories = new() {
            { MmsFileNameKeys.ServiceConfig, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.ClientConfig, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.ClientCommandHistory, ServiceDirectoryKeys.MmsConfig },
            { MmsFileNameKeys.ServerConfig_ServerName, ServiceDirectoryKeys.ServerConfigs },
            { MmsFileNameKeys.MmsVersionIni, ServiceDirectoryKeys.Root },
            { MmsFileNameKeys.LatestVerIni_Name, ServiceDirectoryKeys.MmsConfig },
            { MmsFileNameKeys.VersionManifest_Name, ServiceDirectoryKeys.MmsConfig },
            { MmsFileNameKeys.BdsUpdatePackage_Ver, ServiceDirectoryKeys.BuildArchives },
            { MmsFileNameKeys.JdsUpdatePackage_Ver, ServiceDirectoryKeys.JavaCoreFiles_Ver },
            { MmsFileNameKeys.BedrockStockProps_Ver, ServiceDirectoryKeys.BedrockCoreFiles_Ver },
            { MmsFileNameKeys.JavaStockProps_Ver, ServiceDirectoryKeys.JavaCoreFiles_Ver },
            { MmsFileNameKeys.ServerPlayerRegistry_Name, ServiceDirectoryKeys.ServerPlayerPath },
            { MmsFileNameKeys.GlobalPlayerRegistry, ServiceDirectoryKeys.GlobalPlayerPath },
            { MmsFileNameKeys.ServerPlayerTelem_Name, ServiceDirectoryKeys.ServerPlayerPath },
            { MmsFileNameKeys.GlobalPlayerTelem, ServiceDirectoryKeys.GlobalPlayerPath },
            { MmsFileNameKeys.JavaVanillaExe, ServiceDirectoryKeys.Jdk21BinPath },
            { MmsFileNameKeys.JavaMmsExe, ServiceDirectoryKeys.Jdk21BinPath },
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
            { ServerFileNameKeys.JavaEula, ServerDirectoryKeys.ServerRoot }
        };


        public static Dictionary<MinecraftServerArch, string> MinecraftArchStrings = new() {
            { MinecraftServerArch.Bedrock, "Bedrock" },
            { MinecraftServerArch.Java, "Java" }
        };

        public static string GetNewTempDirectory(string folderName)
        {
            string newPath = $"{Path.GetTempPath()}MMSTemp\\{folderName}_{Guid.NewGuid()}";
            Directory.CreateDirectory(newPath);
            return newPath;
        }

        public static MinecraftServerArch GetArchFromString(string archName)
        {
            switch (archName)
            {
                case "Bedrock":
                    return MinecraftServerArch.Bedrock;
                case "Java":
                    return MinecraftServerArch.Java;
            }
            return MinecraftServerArch.Bedrock;
        }

        public static Dictionary<ServerDirectoryKeys, string> BdsDirectoryStrings = new() {
            { ServerDirectoryKeys.ServerRoot, "" },
            { ServerDirectoryKeys.ServerWorldDir_LevelName, "worlds\\{0}" },
            { ServerDirectoryKeys.ResourcePacksDir_LevelName, "worlds\\{0}\\resource_packs" },
            { ServerDirectoryKeys.BehaviorPacksDir_LevelName, "worlds\\{0}\\behavior_packs" }
        };

        public static Dictionary<MmsUrlKeys, string> MmsUrlStrings = new() {
            { MmsUrlKeys.BdsDownloadPage, "https://www.minecraft.net/en-us/download/server/bedrock" },
            { MmsUrlKeys.BdsPackage_Ver, "https://www.minecraft.net/bedrockdedicatedserver/bin-win/bedrock-server-{0}.zip" },
            { MmsUrlKeys.BdsVersionRegx, @"(https://www.minecraft.net/bedrockdedicatedserver/bin-win/bedrock-server-)(.*)(\.zip)" },
            { MmsUrlKeys.BdsVersionJson, "https://raw.githubusercontent.com/crowbarmaster/BedrockManagementService/master/MMS_Files/bedrock_version_prop_manifest.json" },
            { MmsUrlKeys.JdsVersionJson, "https://raw.githubusercontent.com/crowbarmaster/BedrockManagementService/master/MMS_Files/java_version_prop_manifest.json" },
            { MmsUrlKeys.Jdk21DownloadLink, "https://download.oracle.com/java/21/latest/jdk-21_windows-x64_bin.zip"}
        };

        public static Dictionary<ServiceDirectoryKeys, string> MmsDirectoryStrings = new() {
            { ServiceDirectoryKeys.Root, "" },
            { ServiceDirectoryKeys.MmsConfig, "MmsConfig" },
            { ServiceDirectoryKeys.ServerConfigs, "MmsConfig\\ServerConfigs" },
            { ServiceDirectoryKeys.ServerPlayerPath, "MmsConfig\\ServerConfigs\\PlayerRecords" },
            { ServiceDirectoryKeys.GlobalPlayerPath, "MmsConfig\\ServerConfigs\\GlobalPlayers" },
            { ServiceDirectoryKeys.JavaBuilds, "MmsConfig\\JavaBuilds" },
            { ServiceDirectoryKeys.BdsBuilds, "MmsConfig\\BDSBuilds" },
            { ServiceDirectoryKeys.BuildArchives, "MmsConfig\\BDSBuilds\\BuildArchives" },
            { ServiceDirectoryKeys.JavaCoreFiles_Ver, "MmsConfig\\JavaBuilds\\Build_{0}" },
            { ServiceDirectoryKeys.BedrockCoreFiles_Ver, "MmsConfig\\BDSBuilds\\CoreFiles\\Build_{0}" },
            { ServiceDirectoryKeys.Jdk21Path, "Jdk21" },
            { ServiceDirectoryKeys.Jdk21BinPath, "Jdk21\\bin" }
        };

        public static Dictionary<MmsFileNameKeys, string> MmsFileNameStrings = new() {
            { MmsFileNameKeys.ServiceConfig, "Service.conf" },
            { MmsFileNameKeys.ClientConfig, "Client.conf" },
            { MmsFileNameKeys.ClientCommandHistory, "ClientCommandHistory.txt" },
            { MmsFileNameKeys.ServerConfig_ServerName, "{0}.conf" },
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
            { MmsFileNameKeys.JavaVanillaExe, "javaw.exe" },
            { MmsFileNameKeys.JavaMmsExe, "MmsServerInstance.exe" },
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

        public static void SetWorkingDirectory(ProcessInfo processInfo)
        {
            if (!MmsDirectoryStrings.ContainsKey(ServiceDirectoryKeys.WorkingDirectory))
            {
                MmsDirectoryStrings.Add(ServiceDirectoryKeys.WorkingDirectory, processInfo.GetDirectory());
            }
            else
            {
                MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory] = processInfo.GetDirectory();
            }
        }

        public static string GetServiceDirectory(ServiceDirectoryKeys key)
        {
            if (!MmsDirectoryStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return MmsDirectoryStrings[key] != string.Empty && key != ServiceDirectoryKeys.WorkingDirectory ?
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}\{MmsDirectoryStrings[key]}" :
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}";
        }

        public static string GetServiceDirectory(ServiceDirectoryKeys key, object var0)
        {
            if (!MmsDirectoryStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            string compile = MmsDirectoryStrings[key] != string.Empty && key != ServiceDirectoryKeys.WorkingDirectory ?
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}\{MmsDirectoryStrings[key]}" :
    $@"{MmsDirectoryStrings[ServiceDirectoryKeys.WorkingDirectory]}";
            return string.Format(compile, var0);
        }

        public static string GetServiceFileName(MmsFileNameKeys key)
        {
            if (!MmsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!MmsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!MmsDirectoryStrings.ContainsKey(MmsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {MmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return MmsFileNameStrings[key];
        }

        public static string GetServiceFileName(MmsFileNameKeys key, object var0)
        {
            if (!MmsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!MmsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!MmsDirectoryStrings.ContainsKey(MmsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {MmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format(MmsFileNameStrings[key], var0);
        }

        public static string GetServiceFilePath(MmsFileNameKeys key)
        {
            if (!MmsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            if (!MmsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!MmsDirectoryStrings.ContainsKey(MmsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {MmsFileParentDirectories[key]} does not have a directory associated.");
            }
            return $"{GetServiceDirectory(MmsFileParentDirectories[key])}\\{MmsFileNameStrings[key]}";
        }

        public static string GetServiceFilePath(MmsFileNameKeys key, object? var0)
        {
            if (!MmsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException();
            }
            string output = $"{GetServiceDirectory(MmsFileParentDirectories[key])}\\{MmsFileNameStrings[key]}";
            return string.Format(output, var0);
        }

        public static string GetServerFileName(ServerFileNameKeys key)
        {
            if (!BdsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            return BdsFileNameStrings[key];
        }

        public static string GetServerDirectory(ServerDirectoryKeys key, IServerConfiguration server)
        {
            if (!BdsDirectoryStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{BdsDirectoryStrings[key]}" :
                $@"{server.GetSettingsProp(ServerPropertyKeys.ServerPath)}";
        }

        public static string GetServerDirectory(ServerDirectoryKeys key, string serverPath)
        {
            if (!BdsDirectoryStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                $@"{serverPath}\{BdsDirectoryStrings[key]}" :
                $@"{serverPath}";
        }

        public static string GetServerDirectory(ServerDirectoryKeys key, string serverPath, object? var0)
        {
            if (!BdsDirectoryStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a directory associated.");
            }
            return BdsDirectoryStrings[key] != string.Empty ?
                string.Format($@"{serverPath}\{BdsDirectoryStrings[key]}", var0) :
                string.Format($@"{serverPath}", var0);
        }

        public static string GetServerFilePath(ServerFileNameKeys key, IServerConfiguration server)
        {
            if (!BdsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return $"{GetServerDirectory(BdsFileParentDirectories[key], server)}\\{BdsFileNameStrings[key]}";
        }

        public static string GetServerFilePath(ServerFileNameKeys key, IServerConfiguration server, object var0)
        {
            if (!BdsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format($@"{GetServerDirectory(BdsFileParentDirectories[key], server)}\{BdsFileNameStrings[key]}", var0);
        }

        public static string GetServerFilePath(ServerFileNameKeys key, string serverPath)
        {
            if (!BdsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return $@"{GetServerDirectory(BdsFileParentDirectories[key], serverPath)}\{BdsFileNameStrings[key]}";
        }

        public static string GetServerFilePath(ServerFileNameKeys key, string serverPath, object var0)
        {
            if (!BdsFileNameStrings.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Key {key} was not a does not have a file name associated.");
            }
            if (!BdsFileParentDirectories.ContainsKey(key))
            {
                throw new KeyNotFoundException($"File {key} does not have a parent directory associated.");
            }
            if (!BdsDirectoryStrings.ContainsKey(BdsFileParentDirectories[key]))
            {
                throw new KeyNotFoundException($"File {BdsFileParentDirectories[key]} does not have a directory associated.");
            }
            return string.Format($@"{GetServerDirectory(BdsFileParentDirectories[key], serverPath)}\{BdsFileNameStrings[key]}", var0);
        }
    }
}
