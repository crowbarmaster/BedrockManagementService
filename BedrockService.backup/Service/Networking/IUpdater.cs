using BedrockService.Shared.Interfaces;
using System.Threading.Tasks;

namespace BedrockService.Service.Networking
{
    public interface IUpdater
    {
        Task CheckUpdates();
        Task FetchBuild(string path, string version);
        bool CheckVersionChanged();
        void MarkUpToDate();
        Task ReplaceBuild(IServerConfiguration serverConfiguration);
    }
}