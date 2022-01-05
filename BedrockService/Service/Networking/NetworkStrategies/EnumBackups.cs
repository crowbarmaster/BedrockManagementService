using BedrockService.Service.Networking.MessageInterfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class EnumBackups : IMessageParser {
        private readonly IConfigurator _configurator;

        public EnumBackups(IConfigurator configurator) {
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_configurator.EnumerateBackupsForServer(serverIndex), Formatting.Indented, settings));
            return (serializeToBytes, 0, NetworkMessageTypes.EnumBackups);
        }
    }
}
