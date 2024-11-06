using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class PlayerRequest(MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            message.Data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(service.GetServerByIndex(message.ServerIndex).GetPlayerManager().GetPlayerList(), Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return message;
        }
    }
}

