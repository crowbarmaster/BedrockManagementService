using MinecraftService.Service.Networking.Interfaces;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class LevelEditRequest : IMessageParser {

        private readonly ServiceConfigurator _serviceConfiguration;

        public LevelEditRequest(ServiceConfigurator serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            string pathToLevelDat = $@"{_serviceConfiguration.GetServerInfoByIndex(serverIndex).GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds\{server.GetProp(MmsDependServerPropKeys.LevelName)}\level.dat";
            byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
            return (levelDatToBytes, 0, NetworkMessageTypes.LevelEditFile);
        }
    }
}
