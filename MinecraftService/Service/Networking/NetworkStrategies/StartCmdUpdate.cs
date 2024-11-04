
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class StartCmdUpdate : IMessageParser {
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly UserConfigManager _configurator;

        public StartCmdUpdate(UserConfigManager configurator, ServiceConfigurator serviceConfiguration) {
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

