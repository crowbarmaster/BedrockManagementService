using BedrockService.Service.Networking.NetworkMessageClasses;
using BedrockService.Shared.Classes;
using System.Collections.Generic;

namespace BedrockService.Service.Networking
{
    public interface ITCPListener : IMessageSender
    {
        void StartListening();

        void StopListening();

        void SetStrategyDictionaries(Dictionary<NetworkMessageTypes, IMessageParser> standard, Dictionary<NetworkMessageTypes, IFlaggedMessageParser> flagged);
    }
}
