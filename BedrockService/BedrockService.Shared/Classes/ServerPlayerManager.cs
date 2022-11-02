using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace BedrockService.Shared.Classes {
    public class ServerPlayerManager : IPlayerManager {
        readonly IServerConfiguration? _serverConfiguration;

        public ServerPlayerManager(IServerConfiguration? serverConfiguration) {
            _serverConfiguration = serverConfiguration;
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
            IPlayer foundPlayer = _serverConfiguration.GetPlayerList().FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(_serverConfiguration.GetProp(SharedStringBase.BmsDependServerPropStrings[SharedStringBase.BmsDependServerPropKeys.PermLevel]).ToString());
                player.Initialize(xuid, username);
                _serverConfiguration.GetPlayerList().Add(player);
                return player;
            }
            return foundPlayer;
        }

        public void AddUpdatePlayer(IPlayer player) {
            IPlayer foundPlayer = _serverConfiguration.GetPlayerList().FirstOrDefault(p => p.GetXUID() == player.GetXUID());
            if (foundPlayer == null) {
                _serverConfiguration.GetPlayerList().Add(player);
                return;
            }
            _serverConfiguration.GetPlayerList()[_serverConfiguration.GetPlayerList().IndexOf(foundPlayer)] = player;
        }

        public List<IPlayer> GetPlayerList() => _serverConfiguration.GetPlayerList();

        public void SetPlayerList(List<IPlayer> playerList) => _serverConfiguration.SetPlayerList(playerList);
    }
}
