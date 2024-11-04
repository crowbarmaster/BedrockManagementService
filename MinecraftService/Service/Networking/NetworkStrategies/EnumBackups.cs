
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class EnumBackups : IMessageParser {
        private readonly UserConfigManager _configurator;

        public EnumBackups(UserConfigManager configurator) {
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string jsonString = JsonConvert.SerializeObject(_configurator.EnumerateBackupsForServer(serverIndex).Result, Formatting.Indented);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
            return (serializeToBytes, 0, NetworkMessageTypes.EnumBackups);
        }
    }
}
