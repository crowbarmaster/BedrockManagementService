﻿using MinecraftService.Service.Networking.Interfaces;
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
            IProgress<ProgressModel> progress = new Progress<ProgressModel>((progress) => {
                string percent = string.Format("{0:N2}", progress.Progress);
                _logger.AppendLine($"{progress.Message} {percent}%");
            });
            MinecraftPackParser resourceParser = new(_logger, progress);
            MinecraftPackParser behaviorParser = new(_logger, progress);
            resourceParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName), 0);
            behaviorParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName), 0);
            MinecraftFileUtilities.ValidateFixWorldPackFiles(serverPath, levelName).Wait();
            combinedList.AddRange(resourceParser.FoundPacks);
            combinedList.AddRange(behaviorParser.FoundPacks);

            string arrayString = JsonConvert.SerializeObject(combinedList);
            return (Encoding.UTF8.GetBytes(arrayString), 0, NetworkMessageTypes.PackList);
        }
    }
}