namespace BedrockService.Service.Server.Interfaces {
    public interface IPlayerManager {
        IPlayer PlayerConnected(string username, string xuid);
        IPlayer PlayerDisconnected(string xuid);
    }
}