using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.Classes.Configurations;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking.NetworkStrategies
{
    public class AddNewServer : IMessageParser {
        private readonly IProcessInfo _processInfo;

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IBedrockService _bedrockService;
        private readonly IServerLogger _logger;

        public AddNewServer(IServerLogger logger, IProcessInfo processInfo, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IBedrockService bedrockService) {
            _processInfo = processInfo;
            _bedrockService = bedrockService;
            _configurator = configurator;
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            ServerCombinedPropModel propModel = JsonConvert.DeserializeObject<ServerCombinedPropModel>(stringData, GlobalJsonSerialierSettings);
            string serversPath = _serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath).ToString();
            List<Property>? propList = propModel?.ServerPropList;
            List<Property>? servicePropList = propModel?.ServicePropList;
            Property? serverNameProp = new(string.Empty, string.Empty);
            Property? ipV6Prop = new(string.Empty, string.Empty);
            Property? archProp = servicePropList?.First(x => x.KeyName == ServerPropertyStrings[ServerPropertyKeys.MinecraftType]);
            if(archProp.StringValue == "Java") { 
                serverNameProp = servicePropList?.First(p => p.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerName]);
            } else {
                serverNameProp = propList?.First(p => p.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.ServerName]);
            }
            Property? ipV4Prop = propList?.First(p => p.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.PortI4]);
            if(archProp.StringValue != "Java") {
                ipV6Prop = propList?.First(p => p.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.PortI6]);
            }
            Property? versionProp = servicePropList?.First(p => p.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerVersion]);
            EnumTypeLookup typeLookup = new(_logger, _serviceConfiguration);
            IServerConfiguration newServer = typeLookup.PrepareNewServerByArchName(archProp.StringValue, _processInfo, _logger, _serviceConfiguration);
            newServer.InitializeDefaults();
            newServer.SetAllSettings(servicePropList);
            newServer.SetAllProps(propList);
            newServer.ProcessNewServerConfiguration();
            _configurator.SaveServerConfiguration(newServer);
            _bedrockService.InitializeNewServer(newServer);

            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, GlobalJsonSerialierSettings));
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);
        }
    }
}

