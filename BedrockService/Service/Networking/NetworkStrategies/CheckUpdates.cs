using BedrockService.Service.Networking.Interfaces;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class CheckUpdates : IMessageParser {

        private readonly IBedrockService _service;

        public CheckUpdates(IBedrockService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            _service.GetBedrockServerByIndex(serverIndex).CheckUpdates();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}