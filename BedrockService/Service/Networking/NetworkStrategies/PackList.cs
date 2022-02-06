using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using Newtonsoft.Json;
using System.Text;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class PackList : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly IBedrockLogger _logger;

        public PackList(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IBedrockLogger logger) {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _processInfo = processInfo;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            MinecraftKnownPacksClass knownPacks = new MinecraftKnownPacksClass($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\valid_known_packs.json", $@"{_processInfo.GetDirectory()}\BmsConfig\stock_packs.json");
            List<MinecraftPackContainer> list = new List<MinecraftPackContainer>();
            foreach (KnownPacksJsonModel pack in knownPacks.InstalledPacks.Contents) {
                MinecraftPackParser currentParser = new MinecraftPackParser(_processInfo);
                currentParser.ParseDirectory($@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\{pack.path.Replace(@"/", @"\")}");
                list.AddRange(currentParser.FoundPacks);
            }
            string arrayString = JsonConvert.SerializeObject(list);
            return (Encoding.UTF8.GetBytes(arrayString), 0, NetworkMessageTypes.PackList);
        }
    }
}