namespace BedrockService.Service.Server.Management {
    public class PlayerManager : IPlayerManager {
        readonly IServerConfiguration _serverConfiguration;

        public PlayerManager(IServerConfiguration serverConfiguration) {
            _serverConfiguration = serverConfiguration;
        }

        public void PlayerConnected(string username, string xuid) {
            string currentTimeInTicks = DateTime.Now.Ticks.ToString();
            IPlayer playerFound = _serverConfiguration.GetPlayerByXuid(xuid);
            if (playerFound == null) {
                _serverConfiguration.AddUpdatePlayer(new Player(xuid, username, currentTimeInTicks, currentTimeInTicks, "0", false, _serverConfiguration.GetProp("default-player-permission-level").ToString(), false));
                return;
            }
            playerFound.UpdateTimes(currentTimeInTicks, playerFound.GetTimes().Disconn);
            _serverConfiguration.AddUpdatePlayer(playerFound);
        }

        public void PlayerDisconnected(string xuid) {
            IPlayer playerFound = _serverConfiguration.GetPlayerByXuid(xuid);
            var oldTimes = playerFound.GetTimes();
            playerFound.UpdateTimes(oldTimes.Conn, DateTime.Now.Ticks.ToString());
            _serverConfiguration.AddUpdatePlayer(playerFound);
        }

        public void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit) {
            IPlayer playerFound = _serverConfiguration.GetPlayerByXuid(xuid);
            if (playerFound == null) {
                playerFound = new Player(_serverConfiguration.GetProp("default-player-permission-level").ToString());
                playerFound.Initialize(xuid, username);
            }
            playerFound.UpdateRegistration(permission, whitelisted, ignoreMaxPlayerLimit);
        }

        public IPlayer GetPlayerByXUID(string xuid) {
            if (GetPlayers().Count > 0)
                return _serverConfiguration.GetPlayerByXuid(xuid);
            return null;
        }

        public void SetPlayer(IPlayer player) {
            try {
                _serverConfiguration.GetPlayerList()[_serverConfiguration.GetPlayerList().IndexOf(player)] = player;
            }
            catch {
                _serverConfiguration.GetPlayerList().Add(player);
            }
        }

        public List<IPlayer> GetPlayers() => _serverConfiguration.GetPlayerList();
    }
}
