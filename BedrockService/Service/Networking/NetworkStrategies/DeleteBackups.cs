using BedrockService.Service.Networking.MessageInterfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    class DeleteBackups : IMessageParser {
        private readonly IConfigurator _configurator;

        public DeleteBackups(IConfigurator configurator) {
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            List<string> backupFileNames = JsonConvert.DeserializeObject<List<string>>(stringData, settings);
            _configurator.DeleteBackupsForServer(serverIndex, backupFileNames);
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}
