namespace BedrockService.Service.Networking.Interfaces {
    public interface ITCPListener {
        void Initialize();
        Task StartListening();
        Task CancelAllTasks();
        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);
        void SetServiceStarted();
        void SetServiceStopped();
    }
}
