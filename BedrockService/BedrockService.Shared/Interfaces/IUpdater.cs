using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BedrockService.Shared.Interfaces {
    public interface IUpdater {
        void Initialize();
        Task CheckLatestVersion();
        string GetBaseVersion(string version);
        Task<bool> FetchBuild(string version);
        Task ReplaceServerBuild(string versionOverride = "");
        List<string> GetVersionList();
    }
}