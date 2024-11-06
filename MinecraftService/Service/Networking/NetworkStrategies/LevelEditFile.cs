using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class LevelEditFile(ServiceConfigurator serviceConfiguration, MmsService mineraftService, MmsLogger logger) : IMessageParser {

        public Message ParseMessage(Message message) {
            byte[] stripHeaderFromBuffer = new byte[message.Data.Length - 5];
            Buffer.BlockCopy(message.Data, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
            IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(message.ServerIndex);
            string pathToLevelDat = $@"{serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath)}\{server.GetProp(MmsDependServerPropKeys.ServerName)}\worlds\{server.GetProp(MmsDependServerPropKeys.LevelName)}\level.dat";
            mineraftService.GetServerByIndex(message.ServerIndex).PerformOfflineServerTask(() => {
                File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                logger.AppendLine($"level.dat writen to server {server.GetServerName()}");
            });
            return new();
        }
    }
}
