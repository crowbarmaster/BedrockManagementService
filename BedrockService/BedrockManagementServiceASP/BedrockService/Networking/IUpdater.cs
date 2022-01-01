using BedrockService.Shared.Interfaces;

namespace BedrockManagementServiceASP.BedrockService.Networking {
    public interface IUpdater {
        void Initialize();
        Task CheckUpdates();
        Task FetchBuild(string path, string version);
        bool CheckVersionChanged();
        void MarkUpToDate();
        Task ReplaceBuild(IServerConfiguration serverConfiguration);
    }
}