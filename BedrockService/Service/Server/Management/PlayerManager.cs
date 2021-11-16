using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace BedrockService.Service.Server.Management
{
    public class PlayerManager : IPlayerManager
    {
        readonly IServerConfiguration _serverConfiguration;
        readonly IBedrockLogger _logger;

        public PlayerManager(IServerConfiguration serverConfiguration, IBedrockLogger logger)
        {
            this._serverConfiguration = serverConfiguration;
            this._logger = logger;
        }

        public void PlayerConnected(string username, string xuid)
        {
            IPlayer playerFound = _serverConfiguration.GetPlayerByXuid(xuid);
            if (playerFound == null)
            {
                _serverConfiguration.AddUpdatePlayer(new Player(xuid, username, DateTime.Now.Ticks.ToString(), "0", "0", false, _serverConfiguration.GetProp("default-player-permission-level").ToString(), false));
                return;
            }
            playerFound.UpdateTimes(DateTime.Now.Ticks.ToString(), playerFound.GetTimes()[2]);
            _serverConfiguration.AddUpdatePlayer(playerFound);
        }

        public void PlayerDisconnected(string xuid)
        {
            IPlayer playerFound = _serverConfiguration.GetPlayerByXuid(xuid);
            string[] oldTimes = playerFound.GetTimes();
            playerFound.UpdateTimes(oldTimes[1], DateTime.Now.Ticks.ToString());
            _serverConfiguration.AddUpdatePlayer(playerFound);
        }

        public void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit)
        {
            IPlayer playerFound = _serverConfiguration.GetPlayerByXuid(xuid);
            if (playerFound == null)
            {
                playerFound = new Player(_serverConfiguration.GetProp("default-player-permission-level").ToString());
                playerFound.Initialize(xuid, username);
            }
            playerFound.UpdateRegistration(permission, whitelisted, ignoreMaxPlayerLimit);
        }

        public IPlayer GetPlayerByXUID(string xuid)
        {
            if (GetPlayers().Count > 0)
                return _serverConfiguration.GetPlayerByXuid(xuid);
            return null;
        }

        public void SetPlayer(IPlayer player)
        {
            try
            {
                _serverConfiguration.GetPlayerList()[_serverConfiguration.GetPlayerList().IndexOf(player)] = player;
            }
            catch
            {
                _serverConfiguration.GetPlayerList().Add(player);
            }
        }

        public List<IPlayer> GetPlayers() => _serverConfiguration.GetPlayerList();
    }
}
