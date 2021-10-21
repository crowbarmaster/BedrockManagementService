using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;

namespace BedrockService.Service.Networking.NetworkMessageClasses
{
    public interface IFlaggedMessageParser
    {
        void ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag);
    }
}
