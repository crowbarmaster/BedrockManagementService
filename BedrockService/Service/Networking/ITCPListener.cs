using BedrockService.Service.Networking.MessageInterfaces;

namespace BedrockService.Service.Networking {
    public interface ITCPListener {
        void Initialize();
        Task StartListening();
        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);
        Task CancelAllTasks();
        void SetServiceStarted();
        void SetServiceStopped();
    }
}
