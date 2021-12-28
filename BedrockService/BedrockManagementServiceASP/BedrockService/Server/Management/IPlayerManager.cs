using BedrockService.Shared.Interfaces;

namespace BedrockManagementServiceASP.BedrockService.Server.Management {
    public interface IPlayerManager {
        IPlayer GetPlayerByXUID(string xuid);
        void SetPlayer(IPlayer player);
        List<IPlayer> GetPlayers();
        void PlayerConnected(string username, string xuid);
        void PlayerDisconnected(string xuid);
        void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit);
    }
}