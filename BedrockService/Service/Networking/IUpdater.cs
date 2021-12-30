namespace BedrockService.Service.Networking {
    public interface IUpdater {
        void Initialize();
        Task CheckUpdates();
        Task FetchBuild(string path, string version);
        bool CheckVersionChanged();
        void MarkUpToDate();
    }
}