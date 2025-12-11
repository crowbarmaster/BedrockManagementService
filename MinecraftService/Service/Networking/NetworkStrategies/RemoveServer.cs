
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
    public class RemoveServer(UserConfigManager configurator, ServiceConfigurator serviceConfiguration, MmsService minecraftService) : IMessageParser {

        public Message ParseMessage(Message message) {
            minecraftService.GetServerByIndex(message.ServerIndex).ServerStop(true).Wait();
            configurator.RemoveServerConfigs(serviceConfiguration.GetServerInfoByIndex(message.ServerIndex), message.Flag).Wait();
            minecraftService.RemoveServerInfoByIndex(message.ServerIndex);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serviceConfiguration, Formatting.Indented, SharedStringBase.GlobalJsonSerialierSettings));
            return new(serializeToBytes, 0, MessageTypes.Connect);
        }
    }
}

