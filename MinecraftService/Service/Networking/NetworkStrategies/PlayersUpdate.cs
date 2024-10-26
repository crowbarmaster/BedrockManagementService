
using MinecraftService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System.Text;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class PlayersUpdate : IMessageParser {

        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IMinecraftService _service;

        public PlayersUpdate(IConfigurator configurator, ServiceConfigurator serviceConfiguration, IMinecraftService service) {
            _service = service;
            _configurator = configurator;
            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            List<IPlayer> fetchedPlayers = JsonConvert.DeserializeObject<List<IPlayer>>(stringData, GlobalJsonSerialierSettings);
            foreach (IPlayer player in fetchedPlayers) {
                try {
                    _service.GetServerByIndex(serverIndex).GetPlayerManager().AddUpdatePlayer(player);
                } catch (Exception) {
                }
            }
            if (_serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                foreach (IServerConfiguration server in _serviceConfiguration.GetServerList()) {
                    _configurator.SavePlayerDatabase(server);
                    _configurator.WriteJSONFiles(server);
                    Task.Delay(500).Wait();
                    _service.GetServerByIndex(_serviceConfiguration.GetServerIndex(server)).WriteToStandardIn("ops reload");
                    _service.GetServerByIndex(_serviceConfiguration.GetServerIndex(server)).WriteToStandardIn("whitelist reload");
                }
            } else {
                _configurator.SavePlayerDatabase(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
                _configurator.WriteJSONFiles(_serviceConfiguration.GetServerInfoByIndex(serverIndex));
                Task.Delay(500).Wait();
                _service.GetServerByIndex(serverIndex).WriteToStandardIn("ops reload");
                _service.GetServerByIndex(serverIndex).WriteToStandardIn("whitelist reload");
            }

            return (Array.Empty<byte>(), 0, NetworkMessageTypes.UICallback);
        }
    }
}

