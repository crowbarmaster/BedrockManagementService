using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class ServerStatusRequest : IMessageParser {
        private readonly IMinecraftService _service;
        private readonly IServiceConfiguration _serviceConfiguration;

        public ServerStatusRequest(IMinecraftService service, IServiceConfiguration serviceConfiguration) {
            _service = service;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            StatusUpdateModel model = new();
            model.ServiceStatusModel = _service.GetServiceStatus();
            byte[] serializeToBytes = Array.Empty<byte>();
            if (serverIndex != 255) {
                model.ServerStatusModel = _service.GetServerByIndex(serverIndex).GetServerStatus();
            }
            serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model, Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return (serializeToBytes, serverIndex, NetworkMessageTypes.ServerStatusRequest);
        }
    }
}
