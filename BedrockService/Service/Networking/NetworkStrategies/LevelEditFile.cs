using BedrockService.Service.Networking.Interfaces;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class LevelEditFile : IMessageParser {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IBedrockService _bedrockService;
        private readonly IServerLogger _logger;

        public LevelEditFile(IServiceConfiguration serviceConfiguration, IBedrockService bedrockService, IServerLogger logger) {
            _logger = logger;
            _bedrockService = bedrockService;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            byte[] stripHeaderFromBuffer = new byte[data.Length - 5];
            Buffer.BlockCopy(data, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            string pathToLevelDat = $@"{_serviceConfiguration.GetProp(ServicePropertyKeys.ServersPath)}\{server.GetProp(BmsDependServerPropKeys.ServerName)}\worlds\{server.GetProp(BmsDependServerPropKeys.LevelName)}\level.dat";
            _bedrockService.GetBedrockServerByIndex(serverIndex).ServerStop(false).Wait();
            File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
            _logger.AppendLine($"level.dat writen to server {server.GetServerName()}");
            _bedrockService.GetBedrockServerByIndex(serverIndex).ServerStart().Wait();
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}
