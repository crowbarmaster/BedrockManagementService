using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class StatusRequest : IMessageParser {
        private readonly IBedrockService _service;
        private readonly IServiceConfiguration _serviceConfiguration;

        public StatusRequest(IBedrockService service, IServiceConfiguration serviceConfiguration) {
            _service = service;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            StatusUpdateModel model = new StatusUpdateModel();
            model.ServiceStatusModel = _service.GetServiceStatus();
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            byte[] serializeToBytes = Array.Empty<byte>();
            if (serverIndex != 255) {
                model.ServerStatusModel = _service.GetBedrockServerByIndex(serverIndex).GetServerStatus();
            }
            serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model, Formatting.Indented, settings));
            return (serializeToBytes, serverIndex, NetworkMessageTypes.ServerStatusRequest);
        }
    }
}
