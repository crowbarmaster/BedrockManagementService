using BedrockService.Shared.Classes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BedrockService.Shared.Interfaces {
    public interface IUpdater {
        void Initialize();
        void SetNewLogger(IServerLogger logger);
        Task CheckLatestVersion();
        string GetBaseVersion(string version);
        Task<bool> FetchBuild(string version);
        Task ReplaceServerBuild(string versionOverride = "");
        List<SimpleVersionModel> GetVersionList();
    }
}