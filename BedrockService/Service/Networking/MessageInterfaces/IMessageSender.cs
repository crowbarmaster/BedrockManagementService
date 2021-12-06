namespace BedrockService.Service.Networking.MessageInterfaces
{
    public interface IMessageSender
    {
        void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type, NetworkMessageFlags status);

        void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type);

        void SendData(NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type);

        void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type);

        void SendData(byte[] bytes, NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type);

        void SendData(NetworkMessageSource source, NetworkMessageDestination destination, NetworkMessageTypes type, NetworkMessageFlags status);
    }
}