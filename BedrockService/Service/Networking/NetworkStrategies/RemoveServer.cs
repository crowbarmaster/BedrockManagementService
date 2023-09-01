
using MinecraftService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class RemoveServer : IFlaggedMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IBedrockService _bedrockService;

        public RemoveServer(IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
            this._bedrockService = bedrockService;
            _configurator = configurator;

            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex, NetworkMessageFlags flag) {
            _bedrockService.GetBedrockServerByIndex(serverIndex).ServerStop(true).Wait();
            _configurator.RemoveServerConfigs(_serviceConfiguration.GetServerInfoByIndex(serverIndex), flag);
            _bedrockService.RemoveBedrockServerByIndex(serverIndex);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);

        }
    }
}

