using BedrockService.Service.Server.Interfaces;

namespace BedrockService.Service.Server {
    public class PlayerManager : IPlayerManager {
        readonly IServerConfiguration _serverConfiguration;

        public PlayerManager(IServerConfiguration serverConfiguration) {
            _serverConfiguration = serverConfiguration;
        }

        public void PlayerConnected(string username, string xuid) {
            IPlayer playerFound = _serverConfiguration.GetOrCreatePlayer(xuid, username);
            playerFound.UpdateTimes(DateTime.Now.Ticks, playerFound.GetTimes().Disconn);
        }

        public void PlayerDisconnected(string xuid) {
            IPlayer playerFound = _serverConfiguration.GetOrCreatePlayer(xuid);
            playerFound.UpdateTimes(playerFound.GetTimes().Conn, DateTime.Now.Ticks);
        }
    }
}
