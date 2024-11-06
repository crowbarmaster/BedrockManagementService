using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerRestart(MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            service.GetServerByIndex(message.ServerIndex).RestartServer().Wait();
            return Message.EmptyUICallback;
        }
    }
}
