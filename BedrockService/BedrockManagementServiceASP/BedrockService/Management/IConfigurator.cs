using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;

namespace BedrockManagementServiceASP.BedrockService.Management {
    public interface IConfigurator {
        void DeleteBackupsForServer(byte serverIndex, List<string> list);
        List<Property> EnumerateBackupsForServer(byte serverIndex);
        Task LoadAllConfigurations();
        void LoadPlayerDatabase(IServerConfiguration server);
        void LoadRegisteredPlayers(IServerConfiguration server);
        void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag);
        Task ReplaceServerBuild(IServerConfiguration server);
        void SaveGlobalFile();
        void SaveKnownPlayerDatabase(IServerConfiguration server);
        void SaveServerProps(IServerConfiguration server, bool SaveServerInfo);
        void WriteJSONFiles(IServerConfiguration server);
    }
}