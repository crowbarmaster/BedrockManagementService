using BedrockService.Service.Networking.Interfaces;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class LevelEditRequest : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;

        public LevelEditRequest(IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            string pathToLevelDat = $@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetProp("ServerPath")}\worlds\{server.GetProp("level-name")}\level.dat";
            byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
            return (levelDatToBytes, 0, NetworkMessageTypes.LevelEditFile);
        }
    }
}
