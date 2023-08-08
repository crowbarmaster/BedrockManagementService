using BedrockService.Shared.Classes;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Interfaces {
    public interface IServiceConfiguration : IBedrockConfiguration {
        IServerConfiguration GetServerInfoByName(string serverName);
        IServerConfiguration GetServerInfoByIndex(int index);
        byte GetServerIndex(IServerConfiguration server);
        List<IServerConfiguration> GetServerList();
        List<IPlayer> GetPlayerList();
        void SetProp(ServicePropertyKeys keyName, string value);
        void SetPlayerList(List<IPlayer> playerList);
        IPlayer GetOrCreatePlayer(string xuid, string username = null);
        void SetAllServerInfos(List<IServerConfiguration> newInfos);
        void AddNewServerInfo(IServerConfiguration serverConfiguration);
        void RemoveServerInfoByIndex(int serverIndex);
        void RemoveServerInfo(IServerConfiguration serverConfiguration);
        List<Property> GetServerDefaultPropList(MinecraftServerArch serverArch);
        void SetServerDefaultPropList(MinecraftServerArch serverArch, List<Property> newProps);
        Task CalculateTotalBackupsAllServers();
        (int totalBackups, int totalSize) GetServiceBackupInfo();
        void SetLatestVersion(MinecraftServerArch serverArch, string version);
        string GetLatestVersion(MinecraftServerArch serverArch);
    }
}
