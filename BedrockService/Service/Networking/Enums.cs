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
        Command,
        Backup,
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
