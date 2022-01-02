namespace BedrockService.Service.Networking.MessageInterfaces {
    public interface IMessageParser {
        (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex);
    }
}
