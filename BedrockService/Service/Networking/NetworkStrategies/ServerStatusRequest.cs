using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class ServerStatusRequest : IMessageParser {
        private readonly IBedrockService _service;

        public ServerStatusRequest(IBedrockService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            byte[] serializeToBytes = Array.Empty<byte>();
            if (serverIndex != 255) {
                serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_service.GetBedrockServerByIndex(serverIndex).GetServerStatus(), Formatting.Indented, settings));
            }
            return (serializeToBytes, serverIndex, NetworkMessageTypes.ServerStatusRequest);
        }
    }
}
