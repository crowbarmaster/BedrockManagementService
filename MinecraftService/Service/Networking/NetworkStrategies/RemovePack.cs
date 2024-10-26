using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.PackParser;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class RemovePack : IMessageParser {

        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IServerLogger _logger;

        public RemovePack(ServiceConfigurator serviceConfiguration, IServerLogger logger) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            string pathToWorldFolder = $@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds\{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp(MmsDependServerPropKeys.LevelName)}";
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

