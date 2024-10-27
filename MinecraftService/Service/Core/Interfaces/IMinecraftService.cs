using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Service.Core.Interfaces {
    public interface IMinecraftService : ServiceControl {
        Task<bool> Initialize();
        bool RemoveServerInfoByIndex(int serverIndex);
        void InitializeNewServer(IServerConfiguration serverConfiguration);
        Task RestartService();
        ServiceStatusModel GetServiceStatus();
        IServerController GetServerByIndex(int index);
        IServerController? GetServerByName(string name);
        List<IServerController> GetAllServers();
        void TestStart();
        void TestStop();
        bool ServiceShutdown();
    }
}
