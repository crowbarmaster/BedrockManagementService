using BedrockService.Service.Networking.Interfaces;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class BackupRollback : IMessageParser {

        private readonly IBedrockService _service;

        public BackupRollback(IBedrockService service) {
            _service = service;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            _service.GetBedrockServerByIndex(serverIndex).RollbackToBackup(serverIndex, stringData);
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

