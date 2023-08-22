using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.Classes;
using Newtonsoft.Json;
using System.Text;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class VersionListRequest : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerLogger _logger;

        public VersionListRequest(IServerLogger logger, IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            Dictionary<MinecraftServerArch, SimpleVersionModel[]> resultDictionary = new();
            EnumTypeLookup lookup = new(_logger, _serviceConfiguration);
            foreach(KeyValuePair<MinecraftServerArch, IUpdater> updaterKvp in lookup.UpdatersByArch) {
                List<SimpleVersionModel> verStrings = updaterKvp.Value.GetVersionList();
                verStrings.Reverse();
                resultDictionary.Add(updaterKvp.Key, verStrings.ToArray());
            }
            string jsomString = JsonConvert.SerializeObject(resultDictionary, SharedStringBase.GlobalJsonSerialierSettings);
            byte[] resultBytes = Encoding.UTF8.GetBytes(jsomString);
            return (resultBytes, 0, NetworkMessageTypes.VersionListRequest);
        }
    }
}
