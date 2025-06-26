using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerCommand(MmsService service, MmsLogger logger) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data);
            service.GetServerByIndex(message.ServerIndex).WriteToStandardIn(stringData);
            logger.AppendLine($"Sent command {stringData} to stdInput stream");
            return Message.EmptyUICallback;
        }
    }
}

