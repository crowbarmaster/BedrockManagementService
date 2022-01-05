namespace BedrockService.Service.Networking.Interfaces {
    public interface IUpdater {
        void Initialize();
        Task CheckUpdates();
        bool CheckVersionChanged();
        void MarkUpToDate();
    }
}