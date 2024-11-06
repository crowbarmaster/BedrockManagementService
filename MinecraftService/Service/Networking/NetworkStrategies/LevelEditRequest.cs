using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class LevelEditRequest(ServiceConfigurator serviceConfiguration) : IMessageParser {

        public Message ParseMessage(Message message) {
            IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(message.ServerIndex);
            string pathToLevelDat = $@"{serviceConfiguration.GetServerInfoByIndex(message.ServerIndex).GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds\{server.GetProp(MmsDependServerPropKeys.LevelName)}\level.dat";
            byte[] levelDatToBytes = File.ReadAllBytes(pathToLevelDat);
            return new(levelDatToBytes, 0, MessageTypes.LevelEditFile);
        }
    }
}
