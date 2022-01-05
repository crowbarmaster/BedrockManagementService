namespace BedrockService.Service.Management {
    public interface IConfigurator {
        void DeleteBackupsForServer(byte serverIndex, List<string> list);
        List<Property> EnumerateBackupsForServer(byte serverIndex);
        Task LoadAllConfigurations();
        Task LoadServerConfigurations();
        Task LoadGlobals();
        void LoadPlayerDatabase(IServerConfiguration server);
        void LoadRegisteredPlayers(IServerConfiguration server);
        void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag);
        Task ReplaceServerBuild(IServerConfiguration server);
        void SaveGlobalFile();
        void SaveKnownPlayerDatabase(IServerConfiguration server);
        void SaveServerConfiguration(IServerConfiguration server);
        void WriteJSONFiles(IServerConfiguration server);
    }
}