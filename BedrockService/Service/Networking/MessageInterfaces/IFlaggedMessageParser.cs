namespace BedrockService.Service.Networking.MessageInterfaces {
    public interface IFlaggedMessageParser {
        (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag);
    }
}
