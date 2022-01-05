using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Networking.MessageInterfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class ServerPropUpdate : IMessageParser {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockService _bedrockService;
        private readonly IConfigurator _configurator;

        public ServerPropUpdate(IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
            _configurator = configurator;
            _serviceConfiguration = serviceConfiguration;
            _bedrockService = bedrockService;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
            Property prop = propList.FirstOrDefault(p => p.KeyName == "server-name");
            if (prop == null) {
                _serviceConfiguration.SetAllProps(propList);
                _configurator.SaveGlobalFile();
                _bedrockService.RestartService();
                return (Array.Empty<byte>(), 0, 0);
            }
            _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetAllProps(propList);
            _configurator.SaveServerConfiguration(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
            _bedrockService.GetBedrockServerByIndex(serverIndex).RestartServer().Wait();
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

