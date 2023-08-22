using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.SerializeModels;

namespace BedrockService.Service.Core.Interfaces {
    public interface IBedrockService : ServiceControl {
        Task<bool> Initialize();
        void RemoveBedrockServerByIndex(int serverIndex);
        void InitializeNewServer(IServerConfiguration serverConfiguration);
        Task RestartService();
        ServiceStatusModel GetServiceStatus();
        IServerController GetBedrockServerByIndex(int index);
        IServerController? GetBedrockServerByName(string name);
        IPlayerManager GetPlayerManager();
        List<IServerController> GetAllServers();
        void TestStart();
        void TestStop();
        bool ServiceShutdown();
    }
}
