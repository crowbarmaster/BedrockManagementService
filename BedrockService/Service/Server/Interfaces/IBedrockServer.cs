using BedrockService.Shared.SerializeModels;

namespace BedrockService.Service.Server.Interfaces {
    public interface IBedrockServer {
        void Initialize();
        void StartWatchdog();
        Task AwaitableServerStart();
        Task AwaitableServerStop(bool stopWatchdog);
        Task RestartServer();
        string GetServerName();
        void WriteToStandardIn(string command);
        bool RollbackToBackup(byte serverIndex, string folderName);
        void InitializeBackup();
        IBedrockLogger GetLogger();
        IPlayerManager GetPlayerManager();
        bool IsServerModified();
        void ForceServerModified();
        bool ServerAutostartEnabled();
        ServerStatusModel GetServerStatus();
        bool IsPrimaryServer();
        bool IsServerLLCapable();
        void CheckUpdates();
    }
}
