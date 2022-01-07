using BedrockService.Service.Management.Interfaces;
using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class AddNewServer : IMessageParser {
        private readonly IProcessInfo _processInfo;

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IBedrockService _bedrockService;

        public AddNewServer(IProcessInfo processInfo, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
            _processInfo = processInfo;
            _bedrockService = bedrockService;
            _configurator = configurator;

            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, settings);
            Property serverNameProp = propList.First(p => p.KeyName == "server-name");
            ServerConfigurator newServer = new ServerConfigurator(_serviceConfiguration.GetProp("ServersPath").ToString(), _serviceConfiguration.GetServerDefaultPropList()) {
                ServerName = serverNameProp.ToString(),
                ServerPropList = propList,
                ServerPath = new Property("ServerPath", "") {
                    Value = $@"{_serviceConfiguration.GetProp("ServersPath")}\{serverNameProp}"
                },
                ServerExeName = new Property("ServerExeName", "") {
                    Value = $"BedrockService.{serverNameProp}.exe"
                },
                FileName = $@"{serverNameProp}.conf"
            };
            _configurator.SaveServerConfiguration(newServer);
            _bedrockService.InitializeNewServer(newServer);

            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, settings));
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);

        }
    }
}

