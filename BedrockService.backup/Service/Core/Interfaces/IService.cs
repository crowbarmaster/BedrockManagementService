using System.Threading.Tasks;

namespace BedrockService.Service.Core
{
    public interface IService
    {
        Task InitializeHost();
        Topshelf.TopshelfExitCode Run();
    }
}
