using BedrockService.Shared.SerializeModels;

namespace BedrockService.Service.Server.Interfaces {
    public interface IBedrockServer {
        void Initialize();
        void StartWatchdog();
        Task AwaitableServerStart();
        Task AwaitableServerStop(bool stopWatchdog);
        void ForceKillServer();
        Task RestartServer();
        string GetServerName();
        void WriteToStandardIn(string command);
        bool RollbackToBackup(byte serverIndex, string folderName);
        void InitializeBackup();
        IBedrockLogger GetLogger();
        IPlayerManager GetPlayerManager();
        List<IPlayer> GetActivePlayerList();
        bool IsServerModified();
        void SetServerModified(bool isModified);
        bool ServerAutostartEnabled();
        ServerStatusModel GetServerStatus();
        void SetStartupStatus(ServerStatus status);
        void RunStartupCommands();
        bool IsPrimaryServer();
        void CheckUpdates();
        bool LiteLoadedServer();
        BackupManager GetBackupManager();
    }
}
