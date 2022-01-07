namespace BedrockService.Service.Management.Interfaces {
    public interface IConfigurator {
        Task LoadGlobals();
        Task LoadServerConfigurations();
        Task ReplaceServerBuild(IServerConfiguration server);
        void SaveGlobalFile();
        void SavePlayerDatabase(IServerConfiguration server);
        void SaveServerConfiguration(IServerConfiguration server);
        void WriteJSONFiles(IServerConfiguration server);
        List<Property> EnumerateBackupsForServer(byte serverIndex);
        void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag);
        void DeleteBackupsForServer(byte serverIndex, List<string> list);
    }
}