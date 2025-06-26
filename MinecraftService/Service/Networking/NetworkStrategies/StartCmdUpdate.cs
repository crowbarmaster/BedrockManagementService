
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class StartCmdUpdate(UserConfigManager configurator, ServiceConfigurator serviceConfiguration) : IMessageParser {
        public Message ParseMessage(Message message) {
            List<StartCmdEntry> entries = JsonConvert.DeserializeObject<List<StartCmdEntry>>(Encoding.UTF8.GetString(message.Data), SharedStringBase.GlobalJsonSerialierSettings);
            serviceConfiguration.GetServerInfoByIndex(message.ServerIndex).SetStartCommands(entries);
            configurator.SaveServerConfiguration(serviceConfiguration.GetServerInfoByIndex(message.ServerIndex));
            return Message.EmptyUICallback;
        }
    }
}

