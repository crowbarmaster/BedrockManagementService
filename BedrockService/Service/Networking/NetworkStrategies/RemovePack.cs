using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class RemovePack : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly IBedrockLogger _logger;

        public RemovePack(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger) {
            _serviceConfiguration = serviceConfiguration;
            _processInfo = processInfo;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp("ServerPath")}\valid_known_packs.json", $@"{_processInfo.GetDirectory()}\BmsConfig\stock_packs.json");
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            List<MinecraftPackContainer>? container = JsonConvert.DeserializeObject<List<MinecraftPackContainer>>(stringData, settings);
            foreach (MinecraftPackContainer content in container) {
                knownPacks.RemovePackFromServer(_serviceConfiguration.GetServerInfoByIndex(serverIndex), content);
                _logger.AppendLine($@"{content.JsonManifest.header.name} removed from server.");
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

