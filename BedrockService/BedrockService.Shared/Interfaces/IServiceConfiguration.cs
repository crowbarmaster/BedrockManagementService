using BedrockService.Shared.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BedrockService.Shared.Interfaces {
    public interface IServiceConfiguration : IBedrockConfiguration {
        IServerConfiguration GetServerInfoByName(string serverName);
        IServerConfiguration GetServerInfoByIndex(int index);
        byte GetServerIndex(IServerConfiguration server);
        List<IServerConfiguration> GetServerList();
        void SetAllServerInfos(List<IServerConfiguration> newInfos);
        void AddNewServerInfo(IServerConfiguration serverConfiguration);
        void RemoveServerInfoByIndex(int serverIndex);
        void RemoveServerInfo(IServerConfiguration serverConfiguration);
        List<Property> GetServerDefaultPropList();
        Task CalculateTotalBackupsAllServers();
        (int totalBackups, int totalSize) GetServiceBackupInfo();
        void SetLatestBDSVersion(string version);
        string GetLatestBDSVersion();
        bool ValidateLatestVersion();
    }
}
