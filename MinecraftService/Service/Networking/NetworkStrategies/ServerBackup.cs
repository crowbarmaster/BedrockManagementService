using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerBackup : IMessageParser {

        private readonly IMinecraftService _service;
        public ServerBackup(IMinecraftService service) {

            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            IServerController server = _service.GetServerByIndex(serverIndex);
            server.SetServerModified(true);
            server.GetBackupManager().InitializeBackup();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}
