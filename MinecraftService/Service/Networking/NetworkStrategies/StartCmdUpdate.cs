
using MinecraftService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class StartCmdUpdate : IMessageParser {
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IConfigurator _configurator;

        public StartCmdUpdate(IConfigurator configurator, ServiceConfigurator serviceConfiguration) {
            _configurator = configurator;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            List<StartCmdEntry> entries = JsonConvert.DeserializeObject<List<StartCmdEntry>>(Encoding.UTF8.GetString(data, 5, data.Length - 5), SharedStringBase.GlobalJsonSerialierSettings);
            _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetStartCommands(entries);
            _configurator.SaveServerConfiguration(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

