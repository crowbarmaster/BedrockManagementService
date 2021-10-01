namespace BedrockService.Service.Networking
{
    public enum NetworkMessageSource
    {
        Client,
        Server,
        Service
    }


    public enum NetworkMessageDestination
    {
        Client,
        Server,
        Service
    }

    public enum NetworkMessageTypes
    {
        Connect,
        Disconnect,
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
        RemovePack,
        Command,
        Backup,
        BackupRollback,
        BackupAll,
        DelBackups,
        EnumBackups,
        Restart,
        Heartbeat,
        UICallback
    }

    public enum NetworkMessageFlags
    {
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
