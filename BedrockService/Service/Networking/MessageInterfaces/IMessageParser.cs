namespace BedrockService.Service.Networking.MessageInterfaces {
    public interface IMessageParser {
        void ParseMessage(byte[] data, byte serverIndex);
    }
}
