using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class Connect : IMessageParser {
        private readonly ServiceConfigurator _serviceConfiguration;

        public Connect(ServiceConfigurator serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            Formatting indented = Formatting.Indented;
            string jsonString = JsonConvert.SerializeObject(_serviceConfiguration, indented, SharedStringBase.GlobalJsonSerialierSettings);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);
        }
    }
}