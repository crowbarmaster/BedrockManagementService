namespace BedrockService.Service.Management
{
    public interface IConfigurator
    {
        void DeleteBackupsForServer(byte serverIndex, List<string> list);
        List<Property> EnumerateBackupsForServer(byte serverIndex);
        Task LoadAllConfigurations();
        Task LoadConfiguration(IBedrockConfiguration configuration);
        void LoadPlayerDatabase(IServerConfiguration server);
        void LoadRegisteredPlayers(IServerConfiguration server);
        void RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag);
        Task ReplaceServerBuild(IServerConfiguration server);
        Task SaveConfiguration(IBedrockConfiguration configuration);
        void SaveGlobalFile();
        void SaveKnownPlayerDatabase(IServerConfiguration server);
        void SaveServerProps(IServerConfiguration server, bool SaveServerInfo);
        void WriteJSONFiles(IServerConfiguration server);
        CommsKeyContainer GetKeyContainer();
    }
}