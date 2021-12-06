using BedrockService.Service.Server;

namespace BedrockService.Service.Core.Interfaces
{
    public interface IBedrockService : ServiceControl
    {
        IBedrockServer GetBedrockServerByIndex(int index);

        void RemoveBedrockServerByIndex(int serverIndex);

        IBedrockServer GetBedrockServerByName(string name);

        List<IBedrockServer> GetAllServers();

        void InitializeNewServer(IServerConfiguration serverConfiguration);

        void RestartService();
    }
}
