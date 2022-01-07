namespace BedrockService.Shared.Interfaces {
    public interface IPlayer {
        IPlayer Initialize(string xuid, string username);
        string SearchForProperty(string input);
        string GetUsername();
        string GetXUID();
        (long First, long Conn, long Disconn) GetTimes();
        void UpdateTimes(long conn, long disconn);
        bool IsPlayerWhitelisted();
        bool PlayerIgnoresLimit();
        string GetPermissionLevel();
        bool IsDefaultRegistration();
        string ToString(string format);
        IPlayer UpdatePlayerFromDbStrings(string[] dbString);
        IPlayer UpdatePlayerFromRegStrings(string[] regString);
    }
}
