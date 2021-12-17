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
        Heartbeat,
        Disconnect,
        UICallback
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
