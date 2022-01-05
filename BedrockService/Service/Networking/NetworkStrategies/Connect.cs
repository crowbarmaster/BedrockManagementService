using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class Connect : IMessageParser {
        private readonly IServiceConfiguration _serviceConfiguration;

        public Connect(IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            Formatting indented = Formatting.Indented;
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            string jsonString = JsonConvert.SerializeObject(_serviceConfiguration, indented, settings);
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(jsonString);
            return (serializeToBytes, 0, NetworkMessageTypes.Connect);
        }
    }
}