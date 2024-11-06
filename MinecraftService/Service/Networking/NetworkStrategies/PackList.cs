using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.PackParser;
using Newtonsoft.Json;
using NLog.LayoutRenderers;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class PackList(ProcessInfo processInfo, ServiceConfigurator serviceConfiguration, MmsLogger logger) : IMessageParser {

        public Message ParseMessage(Message message) {
            IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(message.ServerIndex);
            string serverPath = server.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue;
            string levelName = server.GetProp(MmsDependServerPropKeys.LevelName).StringValue;
            string pathToWorldFolder = $@"{serverPath}\worlds\{levelName}";
            List<MinecraftPackContainer> combinedList = new List<MinecraftPackContainer>();
            IProgress<ProgressModel> progress = new Progress<ProgressModel>((progress) => {
                string percent = string.Format("{0:N2}", progress.Progress);
                logger.AppendLine($"{progress.Message} {percent}%");
            });
            MinecraftPackParser resourceParser = new(logger, progress);
            MinecraftPackParser behaviorParser = new(logger, progress);
            resourceParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName), 0);
            behaviorParser.ParseDirectory(GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName), 0);
            MinecraftFileUtilities.ValidateFixWorldPackFiles(serverPath, levelName).Wait();
            combinedList.AddRange(resourceParser.FoundPacks);
            combinedList.AddRange(behaviorParser.FoundPacks);

            string arrayString = JsonConvert.SerializeObject(combinedList);
            return new(Encoding.UTF8.GetBytes(arrayString), 0, MessageTypes.PackList);
        }
    }
}