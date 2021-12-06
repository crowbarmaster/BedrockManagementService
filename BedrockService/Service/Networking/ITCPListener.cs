using BedrockService.Service.Networking.MessageInterfaces;
using System.Threading;

namespace BedrockService.Service.Networking
{
    public interface ITCPListener : IMessageSender
    {
        void StartListening(CancellationToken token);

        void ResetListener();

        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);

        void SetKeyContainer(CommsKeyContainer keyContainer);
    }
}
