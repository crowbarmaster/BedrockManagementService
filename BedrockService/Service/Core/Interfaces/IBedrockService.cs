using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.SerializeModels;

namespace BedrockService.Service.Core.Interfaces {
    public interface IBedrockService : ServiceControl {
        Task<bool> Initialize();
        void RemoveBedrockServerByIndex(int serverIndex);
        void InitializeNewServer(IServerConfiguration serverConfiguration);
        Task RestartService();
        ServiceStatusModel GetServiceStatus();
        IBedrockServer GetBedrockServerByIndex(int index);
        IBedrockServer? GetBedrockServerByName(string name);
        IPlayerManager GetPlayerManager();
        List<IBedrockServer> GetAllServers();
        void TestStart();
        void TestStop();
        bool ServiceShutdown();
    }
}
