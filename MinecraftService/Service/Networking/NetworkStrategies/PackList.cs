using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.PackParser;
using Newtonsoft.Json;
using NLog.LayoutRenderers;
using System.Text;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class PackList : IMessageParser {

        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private readonly IServerLogger _logger;

        public PackList(IProcessInfo processInfo, IServiceConfiguration serviceConfiguration, IServerLogger logger) {
            _logger = logger;
            _serviceConfiguration = serviceConfiguration;
            _processInfo = processInfo;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue;
            string levelName = server.GetProp(MmsDependServerPropKeys.LevelName).StringValue;
            string pathToWorldFolder = $@"{serverPath}\worlds\{levelName}";
            List<MinecraftPackContainer> combinedList = new List<MinecraftPackContainer>();
            MinecraftPackParser resourceParser = new(_logger);
            MinecraftPackParser behaviorParser = new(_logger);
            resourceParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName));
            behaviorParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName));
            MinecraftFileUtilities.ValidateFixWorldPackFiles(serverPath, levelName).Wait();
            combinedList.AddRange(resourceParser.FoundPacks);
            combinedList.AddRange(behaviorParser.FoundPacks);

            string arrayString = JsonConvert.SerializeObject(combinedList);
            return (Encoding.UTF8.GetBytes(arrayString), 0, NetworkMessageTypes.PackList);
        }
    }
}