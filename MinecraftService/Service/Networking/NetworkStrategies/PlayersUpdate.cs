
using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class PlayersUpdate(UserConfigManager configurator, ServiceConfigurator serviceConfiguration, MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            List<Player> fetchedPlayers = JsonConvert.DeserializeObject<List<Player>>(stringData, GlobalJsonSerialierSettings);
            foreach (Player player in fetchedPlayers) {
                try {
                    service.GetServerByIndex(message.ServerIndex).GetPlayerManager().AddUpdatePlayer(player);
                } catch (Exception) {
                }
            }
            if (serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                foreach (IServerConfiguration server in serviceConfiguration.GetServerList()) {
                    configurator.SavePlayerDatabase(server);
                    configurator.WriteJSONFiles(server);
                    Task.Delay(500).Wait();
                    service.GetServerByIndex(serviceConfiguration.GetServerIndex(server)).WriteToStandardIn("ops reload");
                    service.GetServerByIndex(serviceConfiguration.GetServerIndex(server)).WriteToStandardIn("whitelist reload");
                }
            } else {
                configurator.SavePlayerDatabase(serviceConfiguration.GetServerInfoByIndex(message.ServerIndex));
                configurator.WriteJSONFiles(serviceConfiguration.GetServerInfoByIndex(message.ServerIndex));
                Task.Delay(500).Wait();
                service.GetServerByIndex(message.ServerIndex).WriteToStandardIn("ops reload");
                service.GetServerByIndex(message.ServerIndex).WriteToStandardIn("whitelist reload");
            }
            return Message.EmptyUICallback;
        }
    }
}

