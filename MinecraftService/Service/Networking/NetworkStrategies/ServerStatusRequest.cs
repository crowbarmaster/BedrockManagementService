using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerStatusRequest(MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            StatusUpdateModel model = new();
            model.ServiceStatusModel = service.GetServiceStatus();
            byte[] serializeToBytes = Array.Empty<byte>();
            if (message.ServerIndex != 255) {
                model.ServerStatusModel = service.GetServerByIndex(message.ServerIndex).GetServerStatus();
            }
            serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model, Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return new(serializeToBytes, message.ServerIndex, MessageTypes.ServerStatusRequest);
        }
    }
}
