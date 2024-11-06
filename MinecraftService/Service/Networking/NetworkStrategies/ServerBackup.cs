using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerBackup(MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            IServerController server = service.GetServerByIndex(message.ServerIndex);
            server.SetServerModified(true);
            server.GetBackupManager().InitializeBackup();
            return Message.EmptyUICallback;
        }
    }
}
