namespace BedrockService.Shared.Interfaces
{
    public interface IPlayer
    {
        void Initialize(string xuid, string username);
        void UpdateTimes(string lastConn, string lastDiscon);
        void UpdateRegistration(string permission, string whitelisted, string ignoreMaxPlayerLimit);
        string SearchForProperty(string input);
        string GetUsername();
        string GetXUID();
        string[] GetTimes();
        string[] GetRegistration();
        bool IsPlayerWhitelisted();
        bool PlayerIgnoresLimit();
        string GetPermissionLevel();
        bool IsDefaultRegistration();
        string ToString(string format);
    }
}
