using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class ServicePlayerManager : IPlayerManager {
        readonly IServiceConfiguration _serviceConfiguration;

        public ServicePlayerManager(IServiceConfiguration? serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
        }

        public IPlayer PlayerConnected(string username, string xuid) {
            IPlayer playerFound = GetOrCreatePlayer(xuid, username);
            playerFound.UpdateTimes(DateTime.Now.Ticks, playerFound.GetTimes().Disconn);
            return playerFound;
        }

        public IPlayer PlayerDisconnected(string xuid) {
            IPlayer playerFound = GetOrCreatePlayer(xuid);
            playerFound.UpdateTimes(playerFound.GetTimes().Conn, DateTime.Now.Ticks);
            return playerFound;
        }

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = _serviceConfiguration.GetPlayerList().FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(_serviceConfiguration.GetProp(SharedStringBase.ServicePropertyKeys.DefaultGlobalPermLevel).ToString());
                player.Initialize(xuid, username);
                _serviceConfiguration.GetPlayerList().Add(player);
                return player;
            }
            return foundPlayer;
        }

        public void AddUpdatePlayer(IPlayer player) {
            IPlayer foundPlayer = _serviceConfiguration.GetPlayerList().FirstOrDefault(p => p.GetXUID() == player.GetXUID());
            if (foundPlayer == null) {
                _serviceConfiguration.GetPlayerList().Add(player);
                return;
            }
            _serviceConfiguration.GetPlayerList()[_serviceConfiguration.GetPlayerList().IndexOf(foundPlayer)] = player;
        }

        public List<IPlayer> GetPlayerList() => _serviceConfiguration.GetPlayerList();

        public void SetPlayerList(List<IPlayer> playerList) => _serviceConfiguration.SetPlayerList(playerList);
    }
}
