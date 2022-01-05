using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class PlayerRequest : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;

        public PlayerRequest(IServiceConfiguration serviceConfiguration) {

            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(server.GetPlayerList(), Formatting.Indented, settings));
            return (serializeToBytes, serverIndex, NetworkMessageTypes.PlayersRequest);
        }
    }
}

