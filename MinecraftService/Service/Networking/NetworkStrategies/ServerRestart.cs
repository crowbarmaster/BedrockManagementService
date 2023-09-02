using MinecraftService.Service.Networking.Interfaces;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class ServerRestart : IMessageParser {
        private readonly IMinecraftService _service;

        public ServerRestart(IMinecraftService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            _service.GetServerByIndex(serverIndex).RestartServer().Wait();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}
