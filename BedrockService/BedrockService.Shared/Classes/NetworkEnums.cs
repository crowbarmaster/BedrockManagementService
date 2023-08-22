namespace BedrockService.Shared.Classes {
    public enum NetworkMessageSource {
        Client,
        Server,
        Service
    }


    public enum NetworkMessageDestination {
        Client,
        Server,
        Service
    }

    public enum NetworkMessageTypes {
        Connect,
        AddNewServer,
        RemoveServer,
        ConsoleLogUpdate,
        PropUpdate,
        PlayersRequest,
        PlayersUpdate,
        StartCmdUpdate,
        CheckUpdates,
        PackFile,
        PackList,
        LLPluginFile,
        ExportFile,
        ImportFile,
        VersionListRequest,
        LevelEditRequest,
        LevelEditFile,
        RemovePack,
        Command,
        Backup,
        BackupRollback,
        BackupAll,
        DelBackups,
        EnumBackups,
        Restart,
        StartStop,
        ServerStatusRequest,
        Heartbeat,
        Disconnect,
        ClientReject,
        UICallback,
        BackupCallback
    }

    public enum FileTypeFlags {
        Backup,
        ServerPackage,
        ServicePackage,
        ServerConfig,
        ServiceConfig
    }

    public enum PackageFlags {
        ConfigFile,
        LastBackup,
        WorldPacks,
        PlayerDatabase,
        Full
    }

    public enum NetworkMessageFlags {
        Failed,
        Passed,
        RemoveBackups,
        RemoveSrv,
        RemovePlayers,
        RemoveBckSrv,
        RemoveBckPly,
        RemovePlySrv,
        RemoveAll,
        None
    }
}
