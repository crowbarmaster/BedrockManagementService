namespace MinecraftService.Service.Networking.Interfaces {
    public interface IMessageParser {
        (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex);
    }
}
