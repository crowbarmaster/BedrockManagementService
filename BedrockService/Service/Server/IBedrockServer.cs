using BedrockService.Service.Server.Management;

namespace BedrockService.Service.Server
{
    public interface IBedrockServer
    {
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
