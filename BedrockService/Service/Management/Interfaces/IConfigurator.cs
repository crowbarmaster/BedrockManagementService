using BedrockService.Shared.SerializeModels;

namespace BedrockService.Service.Management.Interfaces {
    public interface IConfigurator {
        Task LoadGlobals();
        Task LoadServerConfigurations();
        Task ReplaceServerBuild(IServerConfiguration server, string buildVersion);
        void SaveGlobalFile();
        void SavePlayerDatabase(IServerConfiguration server);
        void SaveServerConfiguration(IServerConfiguration server);
        void WriteJSONFiles(IServerConfiguration server);
        Task<List<BackupInfoModel>> EnumerateBackupsForServer(byte serverIndex);
        void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag);
        void DeleteBackupForServer(byte serverIndex, string backupName);
    }
}