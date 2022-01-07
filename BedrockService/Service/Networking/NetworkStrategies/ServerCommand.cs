using BedrockService.Service.Networking.Interfaces;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class ServerCommand : IMessageParser {
        private readonly IBedrockService _service;
        private readonly IBedrockLogger _logger;


        public ServerCommand(IBedrockService service, IBedrockLogger logger) {
            _service = service;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            _service.GetBedrockServerByIndex(serverIndex).WriteToStandardIn(stringData);
            _logger.AppendLine($"Sent command {stringData} to stdInput stream");
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

