using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class StartStopServer(MmsService service) : IMessageParser {

                public Message ParseMessage(Message message) {
            IServerController server = service.GetServerByIndex(message.ServerIndex);
            if (server.GetServerStatus().ServerStatus == ServerStatus.Started) {
                server.ServerStop(true).Wait();
            } else if (server.GetServerStatus().ServerStatus == ServerStatus.Stopped) {
                server.ServerStart().Wait();
                server.StartWatchdog();
            }
            return Message.EmptyUICallback;
        }
    }
}
