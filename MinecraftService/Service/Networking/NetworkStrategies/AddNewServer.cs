using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class AddNewServer : IMessageParser {
        private readonly ProcessInfo _processInfo;

        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly UserConfigManager _configurator;
        private readonly IMinecraftService _minecraftService;
        private readonly MmsLogger _logger;

        public AddNewServer(MmsLogger logger, ProcessInfo processInfo, UserConfigManager configurator, ServiceConfigurator serviceConfiguration, IMinecraftService minecraftService) {
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
            MinecraftServerArch selectedArch = GetArchFromString(archProp.StringValue);
            _configurator.VerifyServerArchInit(selectedArch);
            IServerConfiguration newServer = ServiceConfigurator.PrepareNewServerConfig(selectedArch, _processInfo, _logger, _serviceConfiguration);
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

