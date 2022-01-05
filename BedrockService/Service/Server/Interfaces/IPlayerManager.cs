namespace BedrockService.Service.Server.Interfaces {
    public interface IPlayerManager {
        IPlayer GetPlayerByXUID(string xuid);
        void PlayerConnected(string username, string xuid);
        void PlayerDisconnected(string xuid);
        void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit);
        void SetPlayer(IPlayer player);
        List<IPlayer> GetPlayers();
    }
}