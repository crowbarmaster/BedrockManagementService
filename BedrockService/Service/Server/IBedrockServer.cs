using BedrockService.Service.Server.Management;
using BedrockService.Shared.Interfaces;
using System.Threading.Tasks;
using Topshelf;

namespace BedrockService.Service.Server
{
    public interface IBedrockServer
    {
        void StartWatchdog(HostControl hostControl);
        void StopWatchdog();
        string GetServerName();
        void WriteToStandardIn(string command);
        bool RestartServer(bool shouldPerformBackup);
        Task StopServer();
        BedrockServer.ServerStatus GetServerStatus();
        void SetServerStatus(BedrockServer.ServerStatus newStatus);
        IPlayerManager GetPlayerManager();
        ILogger GetLogger();
    }
}
