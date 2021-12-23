using BedrockService.Service.Server;

namespace BedrockService.Service.Core.Interfaces {
    public interface IBedrockService : ServiceControl {
        Task Initialize();
        void RemoveBedrockServerByIndex(int serverIndex);
        void InitializeNewServer(IServerConfiguration serverConfiguration);
        void RestartService();
        IBedrockServer GetBedrockServerByIndex(int index);
        IBedrockServer GetBedrockServerByName(string name);
        List<IBedrockServer> GetAllServers();
    }
}
