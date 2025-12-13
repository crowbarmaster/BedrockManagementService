
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class EnumBackups(UserConfigManager configurator) : IMessageParser {

        public Message ParseMessage(Message message) {
            try {
                string jsonString = JsonConvert.SerializeObject(configurator.EnumerateBackupsForServer(message.ServerIndex).Result, Formatting.Indented);
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
                return new(serializeToBytes, 0, MessageTypes.EnumBackups);
            } catch { }
            return Message.Empty();
        }
    }
}
