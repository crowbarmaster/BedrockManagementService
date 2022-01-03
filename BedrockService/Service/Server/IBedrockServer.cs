using BedrockService.Service.Server.Management;

namespace BedrockService.Service.Server {
    public interface IBedrockServer {
        void Initialize();
        void StartWatchdog();
        Task AwaitableServerStart();
        Task AwaitableServerStop(bool stopWatchdog);
        string GetServerName();
        void WriteToStandardIn(string command);
        Task RestartServer();
        bool RollbackToBackup(byte serverIndex, string folderName);
        void InitializeBackup();
        IBedrockLogger GetLogger();
    }
}
