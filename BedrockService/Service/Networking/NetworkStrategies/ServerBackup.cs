using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class ServerBackup : IMessageParser {

        private readonly IBedrockService _service;
        public ServerBackup(IBedrockService service) {

            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            IServerController server = _service.GetBedrockServerByIndex(serverIndex);
            server.SetServerModified(true);
            server.GetBackupManager().InitializeBackup();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}
