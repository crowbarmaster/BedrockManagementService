using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class VersionListRequest : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerLogger _logger;

        public VersionListRequest(IServerLogger logger, IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            Dictionary<MinecraftServerArch, SimpleVersionModel[]> resultDictionary = new();
            foreach (KeyValuePair<MinecraftServerArch, IUpdater> updaterKvp in _serviceConfiguration.GetUpdaterTable()) {
                updaterKvp.Value.Initialize().Wait();
                List<SimpleVersionModel> verStrings = updaterKvp.Value.GetSimpleVersionList();
                verStrings.Reverse();
                resultDictionary.Add(updaterKvp.Key, verStrings.ToArray());
            }
            string jsonString = JsonConvert.SerializeObject(resultDictionary, SharedStringBase.GlobalJsonSerialierSettings);
            byte[] resultBytes = Encoding.UTF8.GetBytes(jsonString);
            return (resultBytes, 0, NetworkMessageTypes.VersionListRequest);
        }
    }
}
