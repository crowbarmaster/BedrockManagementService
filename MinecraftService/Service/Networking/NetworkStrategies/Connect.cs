using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class Connect(ServiceConfigurator serviceConfiguration) : IMessageParser {

        public Message ParseMessage(Message message) {
            Formatting indented = Formatting.Indented;
            string jsonString = JsonConvert.SerializeObject(serviceConfiguration, indented, SharedStringBase.GlobalJsonSerialierSettings);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
            return new(serializeToBytes, 0, MessageTypes.Connect);
        }
    }
}