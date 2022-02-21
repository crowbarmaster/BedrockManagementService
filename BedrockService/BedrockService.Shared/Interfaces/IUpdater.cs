using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public interface IUpdater {
        void Initialize();
        Task CheckLatestVersion();
        bool CheckVersionChanged();
        void MarkUpToDate();
    }
}