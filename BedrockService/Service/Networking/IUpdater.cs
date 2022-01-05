namespace BedrockService.Service.Networking {
    public interface IUpdater {
        void Initialize();
        Task CheckUpdates();
        bool CheckVersionChanged();
        void MarkUpToDate();
    }
}