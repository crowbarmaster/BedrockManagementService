using BedrockService.Service.Management.Interfaces;
using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class StartCmdUpdate : IMessageParser {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;

        public StartCmdUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration) {
            _configurator = configurator;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            List<StartCmdEntry> entries = JsonConvert.DeserializeObject<List<StartCmdEntry>>(Encoding.UTF8.GetString(data, 5, data.Length - 5), settings);
            _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetStartCommands(entries);
            _configurator.SaveServerConfiguration(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

