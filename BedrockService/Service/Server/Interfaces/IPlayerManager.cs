namespace BedrockService.Service.Server.Interfaces {
    public interface IPlayerManager {
        void PlayerConnected(string username, string xuid);
        void PlayerDisconnected(string xuid);
    }
}