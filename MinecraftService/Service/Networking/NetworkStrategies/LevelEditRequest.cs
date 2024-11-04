using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
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
