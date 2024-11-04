using MinecraftService.Shared.Classes.Service.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Interfaces
{
    public interface IUpdater {
        Task Initialize();
        void SetNewLogger(MmsLogger logger);
        bool IsInitialized();
        Task CheckLatestVersion();
        List<Property> GetVersionPropList(string version);
        List<Property> GetDefaultVersionPropList();
        string GetBaseVersion(string version);
        bool ValidateBuildExists(string version);
        Task<bool> FetchBuild(string version);
        Task ReplaceBuild(IServerConfiguration serverConfiguration, string versionOverride = "");
        List<SimpleVersionModel> GetSimpleVersionList();
    }
}