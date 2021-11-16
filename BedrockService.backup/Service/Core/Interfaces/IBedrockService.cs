using BedrockService.Service.Server;
using BedrockService.Shared.Interfaces;
using System.Collections.Generic;
using Topshelf;

namespace BedrockService.Service.Core
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
