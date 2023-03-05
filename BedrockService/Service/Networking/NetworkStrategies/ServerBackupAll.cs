using BedrockService.Service.Networking.Interfaces;
using BedrockService.Service.Server.Interfaces;

namespace BedrockService.Service.Networking.NetworkStrategies {
    class ServerBackupAll : IMessageParser {

        private readonly IBedrockService _service;

        public ServerBackupAll(IBedrockService service) {

            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            foreach (IBedrockServer server in _service.GetAllServers()) {
                server.SetServerModified(true);
                server.InitializeBackup();
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}
