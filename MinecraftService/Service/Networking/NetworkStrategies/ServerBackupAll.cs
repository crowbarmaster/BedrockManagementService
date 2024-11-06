using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    class ServerBackupAll(MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            foreach (IServerController server in service.GetAllServers()) {
                server.SetServerModified(true);
                server.GetBackupManager().InitializeBackup();
            }
            return Message.EmptyUICallback;
        }
    }
}
