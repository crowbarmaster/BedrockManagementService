using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerCommand : IMessageParser {
        private readonly IMinecraftService _service;
        private readonly MmsLogger _logger;


        public ServerCommand(IMinecraftService service, MmsLogger logger) {
            _service = service;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            _service.GetServerByIndex(serverIndex).WriteToStandardIn(stringData);
            _logger.AppendLine($"Sent command {stringData} to stdInput stream");
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

