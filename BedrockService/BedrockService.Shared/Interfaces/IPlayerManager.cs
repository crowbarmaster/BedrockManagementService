
using System.Collections.Generic;

namespace BedrockService.Shared.Interfaces {
    public interface IPlayerManager {
        IPlayer PlayerConnected(string username, string xuid);
        IPlayer PlayerDisconnected(string username, string xuid);
        IPlayer GetOrCreatePlayer(string xuid, string username = null);
        void AddUpdatePlayer(IPlayer player);
        List<IPlayer> GetPlayerList();
        void SetPlayerList(List<IPlayer> playerList);
        void LoadPlayerDatabase();
        void SavePlayerDatabase();
    }
}