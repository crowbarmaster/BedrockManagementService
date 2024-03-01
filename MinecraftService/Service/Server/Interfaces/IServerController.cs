using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Service.Server.Interfaces {
    public interface IServerController {
        void Initialize();
        void StartWatchdog();
        Task ServerStart();
        Task ServerStop(bool stopWatchdog);
        void ForceKillServer();
        Task RestartServer();
        string GetServerName();
        void WriteToStandardIn(string command);
        bool RollbackToBackup(string targetZip);
        IServerLogger GetLogger();
        IServerLogger GetServiceLogger();
        IPlayerManager GetPlayerManager();
        List<IPlayer> GetActivePlayerList();
        bool IsServerModified();
        void SetServerModified(bool isModified);
        bool ServerAutostartEnabled();
        ServerStatusModel GetServerStatus();
        void SetStartupStatus(ServerStatus status);
        void RunStartupCommands();
        bool IsPrimaryServer();
        bool IsServerStarted();
        bool IsServerStopped();
        void CheckUpdates();
        IBackupManager GetBackupManager();
        void PerformOfflineServerTask(Action methodToRun);
    }
}
