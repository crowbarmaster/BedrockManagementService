using MinecraftService.Service.Networking.Interfaces;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class BackupRollback : IMessageParser {

        private readonly IBedrockService _service;

        public BackupRollback(IBedrockService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            byte[] sendBytes = new byte[1];

            if (_service.GetBedrockServerByIndex(serverIndex).RollbackToBackup(stringData)) {
                sendBytes[0] = 1;
            } else {
                sendBytes[0] = 0;
            }
            return (sendBytes, serverIndex, NetworkMessageTypes.BackupCallback);
        }
    }
}

