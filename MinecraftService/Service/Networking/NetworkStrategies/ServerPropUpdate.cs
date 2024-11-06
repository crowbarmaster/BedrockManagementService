
using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ServerPropUpdate(MmsLogger logger, UserConfigManager configurator, ServiceConfigurator serviceConfiguration, MmsService mineraftService) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            List<Property> propList = JsonConvert.DeserializeObject<List<Property>>(stringData, SharedStringBase.GlobalJsonSerialierSettings);
            Property prop = propList.FirstOrDefault(p => p.KeyName == "AcceptedMojangLic");
            if (prop != null) {
                serviceConfiguration.SetAllProps(propList);
                configurator.SaveGlobalFile();
                logger.AppendLine("Successfully wrote service configuration to file! Restarting service to apply changes!");
                mineraftService.RestartService();
                return new();
            }
            prop = propList.FirstOrDefault(p => p.KeyName == "server-name");
            if (prop != null) {
                foreach (Property property in propList) {
                    serviceConfiguration.GetServerInfoByIndex(message.ServerIndex).SetProp(property);
                }
            } else {
                foreach (Property property in propList) {
                    serviceConfiguration.GetServerInfoByIndex(message.ServerIndex).SetSettingsProp(property.KeyName, property.StringValue);
                }
            }
            configurator.SaveServerConfiguration(serviceConfiguration.GetServerInfoByIndex(message.ServerIndex));
            logger.AppendLine("Successfully wrote server configuration to file! Restart server to apply changes!");
            return Message.EmptyUICallback;
        }
    }
}

