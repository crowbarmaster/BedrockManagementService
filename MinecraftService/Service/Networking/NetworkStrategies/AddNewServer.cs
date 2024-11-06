using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class AddNewServer(MmsLogger logger, ProcessInfo processInfo, UserConfigManager configurator, ServiceConfigurator serviceConfiguration, MmsService minecraftService) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            ServerCombinedPropModel propModel = JsonConvert.DeserializeObject<ServerCombinedPropModel>(stringData, GlobalJsonSerialierSettings);
            Property? archProp = propModel?.ServicePropList?.First(x => x.KeyName == ServerPropertyStrings[ServerPropertyKeys.MinecraftType]);
            MinecraftServerArch selectedArch = GetArchFromString(archProp.StringValue);
            configurator.VerifyServerArchInit(selectedArch);
            IServerConfiguration newServer = ServiceConfigurator.PrepareNewServerConfig(selectedArch, processInfo, logger, serviceConfiguration);
            newServer.InitializeDefaults();
            newServer.SetAllSettings(propModel?.ServicePropList);
            newServer.SetAllProps(propModel?.ServerPropList);
            newServer.ProcessNewServerConfiguration();
            configurator.SaveServerConfiguration(newServer);
            minecraftService.InitializeNewServer(newServer);

            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serviceConfiguration, Formatting.Indented, GlobalJsonSerialierSettings));
            return new Message { 
                Data = serializeToBytes,
                Type = MessageTypes.Connect 
            };
        }
    }
}

