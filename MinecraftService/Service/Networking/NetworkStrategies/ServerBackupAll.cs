using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    class ServerBackupAll : IMessageParser {

        private readonly IMinecraftService _service;

        public ServerBackupAll(IMinecraftService service) {

            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            foreach (IServerController server in _service.GetAllServers()) {
                server.SetServerModified(true);
                server.GetBackupManager().InitializeBackup();
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}
