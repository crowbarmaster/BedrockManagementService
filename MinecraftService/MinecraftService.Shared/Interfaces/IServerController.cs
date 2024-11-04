using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Interfaces
{
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
        MmsLogger GetLogger();
        MmsLogger GetServiceLogger();
        PlayerManager GetPlayerManager();
        List<Player> GetActivePlayerList();
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
