using BedrockManagementServiceASP.BedrockService.Networking.MessageInterfaces;
using BedrockService.Shared.Classes;

namespace BedrockManagementServiceASP.BedrockService.Networking {
    public interface ITCPListener : IMessageSender {
        void Initialize();
        Task StartListening();
        void BlockClientConnections();
        void UnblockClientConnections();
        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);
    }
}
