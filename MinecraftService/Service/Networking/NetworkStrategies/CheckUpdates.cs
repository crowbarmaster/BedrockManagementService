using MinecraftService.Service.Networking.Interfaces;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class CheckUpdates : IMessageParser {

        private readonly IMinecraftService _service;

        public CheckUpdates(IMinecraftService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            _service.GetServerByIndex(serverIndex).CheckUpdates();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}