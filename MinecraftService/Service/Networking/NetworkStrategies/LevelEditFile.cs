using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class LevelEditFile : IMessageParser {
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IMinecraftService _mineraftService;
        private readonly MmsLogger _logger;

        public LevelEditFile(ServiceConfigurator serviceConfiguration, IMinecraftService mineraftService, MmsLogger logger) {
            _logger = logger;
            _mineraftService = mineraftService;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            byte[] stripHeaderFromBuffer = new byte[data.Length - 5];
            Buffer.BlockCopy(data, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            string pathToLevelDat = $@"{_serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath)}\{server.GetProp(MmsDependServerPropKeys.ServerName)}\worlds\{server.GetProp(MmsDependServerPropKeys.LevelName)}\level.dat";
            _mineraftService.GetServerByIndex(serverIndex).PerformOfflineServerTask(() => {
                File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
                _logger.AppendLine($"level.dat writen to server {server.GetServerName()}");
            });
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}
