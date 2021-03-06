using BedrockManagementServiceASP.BedrockService.Server;
using BedrockService.Shared.Interfaces;
using Topshelf;

namespace BedrockManagementServiceASP.BedrockService.Core.Interfaces {
    public interface IBedrockService : ServiceControl {
        Task<bool> Initialize();
        void RemoveBedrockServerByIndex(int serverIndex);
        void InitializeNewServer(IServerConfiguration serverConfiguration);
        Task RestartService();
        ServiceStatus GetServiceStatus();
        IBedrockServer GetBedrockServerByIndex(int index);
        IBedrockServer? GetBedrockServerByName(string name);
        List<IBedrockServer> GetAllServers();
    }
}
