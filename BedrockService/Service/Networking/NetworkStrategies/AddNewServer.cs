using BedrockService.Service.Management.Interfaces;
using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class AddNewServer : IMessageParser {
        private readonly IProcessInfo _processInfo;

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IBedrockService _bedrockService;
        private readonly IBedrockLogger _logger;

        public AddNewServer(IBedrockLogger logger, IProcessInfo processInfo, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
            _processInfo = processInfo;
            _bedrockService = bedrockService;
            _configurator = configurator;
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            ServerCombinedPropModel propModel = JsonConvert.DeserializeObject<ServerCombinedPropModel>(stringData, settings);
            string serversPath = _serviceConfiguration.GetProp("ServersPath").ToString();
            List<Property>? propList = propModel?.ServerPropList;
            List<Property>? servicePropList = propModel?.ServicePropList;
            Property? serverNameProp = propList?.First(p => p.KeyName == "server-name");
            Property? ipV4Prop = propList?.First(p => p.KeyName == "server-port");
            Property? ipV6Prop = propList?.First(p => p.KeyName == "server-portv6");
            Property? versionProp = servicePropList?.First(p => p.KeyName == "DeployedVersion");
            ServerConfigurator newServer = new ServerConfigurator(_processInfo, _logger, _serviceConfiguration);
            newServer.ServicePropList = servicePropList;
            if (_serviceConfiguration.GetLatestBDSVersion() == versionProp?.StringValue) {
                newServer.ServerPropList = propList;
            } else {
                newServer.SetServerVersion(versionProp?.StringValue);
                newServer.ValidateVersion(versionProp?.StringValue);
                newServer.GetSettingsProp("DeployedVersion").SetValue(versionProp?.StringValue);
                newServer.SetProp("server-name", serverNameProp?.StringValue);
                newServer.SetProp("server-port", ipV4Prop?.StringValue);
                newServer.SetProp("server-portv6", ipV6Prop?.StringValue);
            }
            newServer.GetSettingsProp("ServerName").SetValue(serverNameProp.StringValue);
            newServer.GetSettingsProp("ServerPath").SetValue($@"{serversPath}\{serverNameProp.StringValue}");
            newServer.GetSettingsProp("ServerExeName").SetValue($"BedrockService.{serverNameProp.StringValue}.exe");
            newServer.GetSettingsProp("FileName").SetValue($@"{serverNameProp.StringValue}.conf");
            newServer.SetServerVersion(versionProp?.StringValue);
            _configurator.SaveServerConfiguration(newServer);
            _bedrockService.InitializeNewServer(newServer);

            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, settings));
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);
        }
    }
}

