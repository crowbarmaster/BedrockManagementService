namespace BedrockManagementServiceASP.BedrockService.Networking.MessageInterfaces {
    public interface IMessageParser {
        void ParseMessage(byte[] data, byte serverIndex);
    }
}
