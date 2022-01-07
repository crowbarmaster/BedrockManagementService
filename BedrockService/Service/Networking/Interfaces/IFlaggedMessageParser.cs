namespace BedrockService.Service.Networking.Interfaces {
    public interface IFlaggedMessageParser {
        (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag);
    }
}
