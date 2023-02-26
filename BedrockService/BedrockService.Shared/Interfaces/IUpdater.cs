using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using System.Threading.Tasks;

namespace BedrockService.Shared.Interfaces {
    public interface IUpdater {
        void Initialize();
        Task CheckLatestVersion();
        Task CheckLiteLiteLoaderVersion();
        Task<LiteLoaderVersionManifest> GetLiteLoaderVersionManifest(string version);
    }
}