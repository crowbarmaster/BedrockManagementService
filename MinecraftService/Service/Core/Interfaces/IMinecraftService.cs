using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Service.Core.Interfaces {
    public interface IMinecraftService : ServiceControl {
        Task<bool> Initialize();
        void RemoveServerByIndex(int serverIndex);
        void InitializeNewServer(IServerConfiguration serverConfiguration);
        Task RestartService();
        ServiceStatusModel GetServiceStatus();
        IServerController GetServerByIndex(int index);
        IServerController? GetServerByName(string name);
        IPlayerManager GetPlayerManager();
        List<IServerController> GetAllServers();
        void TestStart();
        void TestStop();
        bool ServiceShutdown();
    }
}
