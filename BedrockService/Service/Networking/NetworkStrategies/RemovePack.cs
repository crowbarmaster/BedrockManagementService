using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class RemovePack : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerLogger _logger;

        public RemovePack(IServiceConfiguration serviceConfiguration, IServerLogger logger) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            string pathToWorldFolder = $@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds\{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp(BmsDependServerPropKeys.LevelName)}";
            MinecraftKnownPacksClass knownPacks = new($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.ServerPath)}\valid_known_packs.json", pathToWorldFolder);
            List<MinecraftPackContainer>? container = JsonConvert.DeserializeObject<List<MinecraftPackContainer>>(stringData, SharedStringBase.GlobalJsonSerialierSettings);
            foreach (MinecraftPackContainer content in container) {
                knownPacks.RemovePackFromServer(_serviceConfiguration.GetServerInfoByIndex(serverIndex), content);
                _logger.AppendLine($@"{content.JsonManifest.header.name} removed from server.");
            }
            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

