using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Networking.MessageInterfaces;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class ServerRestart : IMessageParser {
        private readonly IBedrockService _service;

        public ServerRestart(IBedrockService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            _service.GetBedrockServerByIndex(serverIndex).RestartServer().Wait();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}
