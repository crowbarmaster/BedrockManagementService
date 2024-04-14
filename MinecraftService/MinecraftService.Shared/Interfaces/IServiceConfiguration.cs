using MinecraftService.Shared.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Interfaces {
    public interface IServiceConfiguration : IBaseConfiguration {
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
        string GetLatestVersion(MinecraftServerArch serverArch, bool isBeta = false);
        void SetUpdater(MinecraftServerArch serverArch, IUpdater updater);
        IUpdater GetUpdater(MinecraftServerArch serverArch);
        Dictionary<MinecraftServerArch, IUpdater> GetUpdaterTable();
    }
}
