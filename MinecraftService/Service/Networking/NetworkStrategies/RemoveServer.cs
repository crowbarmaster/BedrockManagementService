
using MinecraftService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class RemoveServer : IFlaggedMessageParser {

        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IMinecraftService _mineraftService;

        public RemoveServer(IConfigurator configurator, ServiceConfigurator serviceConfiguration, IMinecraftService mineraftService) {
            this._mineraftService = mineraftService;
            _configurator = configurator;

            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag) {
            _mineraftService.GetServerByIndex(serverIndex).ServerStop(true).Wait();
            _configurator.RemoveServerConfigs(_serviceConfiguration.GetServerInfoByIndex(serverIndex), flag).Wait();
            _mineraftService.RemoveServerInfoByIndex(serverIndex);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);

        }
    }
}

