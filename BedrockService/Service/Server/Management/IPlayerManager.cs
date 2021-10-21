using BedrockService.Shared.Interfaces;
using System.Collections.Generic;

namespace BedrockService.Service.Server.Management
{
    public interface IPlayerManager
    {
        IPlayer GetPlayerByXUID(string xuid);
        void SetPlayer(IPlayer player);
        List<IPlayer> GetPlayers();
        void PlayerConnected(string username, string xuid);
        void PlayerDisconnected(string xuid);
        void ProcessConfiguration(string[] entries);
        void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit);
    }
}