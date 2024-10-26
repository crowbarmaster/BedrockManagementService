using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Configurations;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class AddNewServer : IMessageParser {
        private readonly IProcessInfo _processInfo;

        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IMinecraftService _minecraftService;
        private readonly IServerLogger _logger;

        public AddNewServer(IServerLogger logger, IProcessInfo processInfo, IConfigurator configurator, ServiceConfigurator serviceConfiguration, IMinecraftService minecraftService) {
            _processInfo = processInfo;
            _minecraftService = minecraftService;
            _configurator = configurator;
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            ServerCombinedPropModel propModel = JsonConvert.DeserializeObject<ServerCombinedPropModel>(stringData, GlobalJsonSerialierSettings);
            Property? archProp = propModel?.ServicePropList?.First(x => x.KeyName == ServerPropertyStrings[ServerPropertyKeys.MinecraftType]);
            EnumTypeLookup typeLookup = new(_logger, _serviceConfiguration);
            MinecraftServerArch selectedArch = GetArchFromString(archProp.StringValue);
            _configurator.VerifyServerArchInit(selectedArch);
            IServerConfiguration newServer = typeLookup.PrepareNewServerByArch(selectedArch, _processInfo, _logger, _serviceConfiguration);
            newServer.InitializeDefaults();
            newServer.SetAllSettings(propModel?.ServicePropList);
            newServer.SetAllProps(propModel?.ServerPropList);
            newServer.ProcessNewServerConfiguration();
            _configurator.SaveServerConfiguration(newServer);
            _minecraftService.InitializeNewServer(newServer);

            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented, GlobalJsonSerialierSettings));
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);
        }
    }
}

