namespace BedrockService.Service.Networking.MessageInterfaces
{
    public interface IFlaggedMessageParser
    {
        void ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag);
    }
}
