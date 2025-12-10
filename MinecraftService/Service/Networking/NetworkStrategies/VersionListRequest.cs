using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class VersionListRequest(MmsLogger logger, ServiceConfigurator serviceConfiguration) : IMessageParser {

        public Message ParseMessage(Message message) {
            logger.AppendLine("Client requested version list data...");
            Dictionary<MinecraftServerArch, SimpleVersionModel[]> resultDictionary = new();
            foreach (KeyValuePair<MinecraftServerArch, IUpdater> updaterKvp in serviceConfiguration.GetUpdaterTable()) {
                if (!updaterKvp.Value.IsInitialized())
                    updaterKvp.Value.Initialize().Wait();
                List<SimpleVersionModel> verStrings = updaterKvp.Value.GetSimpleVersionList();
                verStrings.Reverse();
                resultDictionary.Add(updaterKvp.Key, verStrings.ToArray());
            }
            string jsonString = JsonConvert.SerializeObject(resultDictionary, SharedStringBase.GlobalJsonSerialierSettings);
            byte[] resultBytes = Encoding.UTF8.GetBytes(jsonString);
            return new(resultBytes, message.ServerIndex, MessageTypes.VersionListRequest);
        }
    }
}
