using BedrockService.Shared.Classes;

namespace BedrockService.Service.Networking.NetworkMessageClasses
{
    public interface IFlaggedMessageParser
    {
        void ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag);
    }
}
