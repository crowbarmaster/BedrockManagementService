
using MinecraftService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class ServerPropUpdate : IMessageParser {
        private readonly IServerLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IMinecraftService _mineraftService;
        private readonly IConfigurator _configurator;

        public ServerPropUpdate(IServerLogger logger, IConfigurator configurator, IServiceConfiguration serviceConfiguration, IMinecraftService mineraftService) {
            _logger = logger;
            _configurator = configurator;
            _serviceConfiguration = serviceConfiguration;
            _mineraftService = mineraftService;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, SharedStringBase.GlobalJsonSerialierSettings);
            Property prop = propList.FirstOrDefault(p => p.KeyName == "AcceptedMojangLic");
            if (prop != null) {
                _serviceConfiguration.SetAllProps(propList);
                _configurator.SaveGlobalFile();
                _logger.AppendLine("Successfully wrote service configuration to file! Restarting service to apply changes!");
                _mineraftService.RestartService();
                return (Array.Empty<byte>(), 0, 0);
            }
            prop = propList.FirstOrDefault(p => p.KeyName == "server-name");
            if (prop != null) {
                foreach (Property property in propList) {
                    _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetProp(property);
                }
            } else {
                foreach (Property property in propList) {
                    _serviceConfiguration.GetServerInfoByIndex(serverIndex).SetSettingsProp(property.KeyName, property.StringValue);
                }
            }
            _configurator.SaveServerConfiguration(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
            _logger.AppendLine("Successfully wrote server configuration to file! Restart server to apply changes!");
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

