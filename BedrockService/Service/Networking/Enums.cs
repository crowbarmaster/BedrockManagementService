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
        ConsoleLogUpdate,
        PropUpdate,
        PlayersRequest,
        PlayersUpdate,
        StartCmdUpdate,
        CheckUpdates,
        Command,
        Backup,
        BackupAll,
        Restart,
        Heartbeat
    }

    public enum NetworkMessageStatus
    {
        Failed,
        Passed,
        None
    }
}
