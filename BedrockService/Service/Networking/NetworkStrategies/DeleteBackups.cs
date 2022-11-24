
using BedrockService.Service.Networking.Interfaces;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    class DeleteBackups : IMessageParser {
        private readonly IConfigurator _configurator;

        public DeleteBackups(IConfigurator configurator) {
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            _configurator.DeleteBackupForServer(serverIndex, stringData);
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}
