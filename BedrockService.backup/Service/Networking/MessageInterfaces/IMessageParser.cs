namespace BedrockService.Service.Networking.NetworkMessageClasses
{
    public interface IMessageParser
    {
        void ParseMessage(byte[] data, byte serverIndex);
    }
}
