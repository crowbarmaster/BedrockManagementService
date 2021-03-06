using System.Collections.Generic;

namespace BedrockService.Shared.Interfaces
{
    public interface IServiceConfiguration : IConfiguration
    {
        IServerConfiguration GetServerInfoByName(string serverName);

        IServerConfiguration GetServerInfoByIndex(int index);

        byte GetServerIndex(IServerConfiguration server);

        List<IServerConfiguration> GetServerList();

        void SetAllServerInfos(List<IServerConfiguration> newInfos);

        void AddNewServerInfo(IServerConfiguration serverConfiguration);

        void RemoveServerInfoByIndex(int serverIndex);

        void RemoveServerInfo(IServerConfiguration serverConfiguration);

        void SetServerVersion(string newVersion);

        string GetServerVersion();
    }
}
