using BedrockManagementServiceASP.BedrockService.Server.Management;
using BedrockService.Shared.Interfaces;
using Topshelf;

namespace BedrockManagementServiceASP.BedrockService.Server {
    public interface IBedrockServer {
        void Initialize();
        void StartWatchdog(HostControl hostControl);
        string GetServerName();
        void WriteToStandardIn(string command);
        bool RestartServer(bool shouldPerformBackup);
        bool RollbackToBackup(byte serverIndex, string folderName);
        Task StopServer(bool stopWatchdog);
        void InitializeBackup();
        BedrockServer.ServerStatus GetServerStatus();
        void SetServerStatus(BedrockServer.ServerStatus newStatus);
        IPlayerManager GetPlayerManager();
        IBedrockLogger GetLogger();
    }
}
