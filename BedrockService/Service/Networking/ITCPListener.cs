using BedrockService.Service.Networking.MessageInterfaces;

namespace BedrockService.Service.Networking {
    public interface ITCPListener : IMessageSender {
        void Initialize();
        Task StartListening();
        void BlockClientConnections();
        void UnblockClientConnections();
        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);
    }
}
