using BedrockService.Shared.Classes;

namespace BedrockManagementServiceASP.BedrockService.Networking.MessageInterfaces {
    public interface IFlaggedMessageParser {
        void ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag);
    }
}
