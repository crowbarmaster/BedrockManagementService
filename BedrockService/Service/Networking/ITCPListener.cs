using BedrockService.Service.Networking.MessageInterfaces;

namespace BedrockService.Service.Networking {
    public interface ITCPListener : IMessageSender {
        Task StartListening();

        void ResetListener();

        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);

        void SetKeyContainer(CommsKeyContainer keyContainer);
    }
}
