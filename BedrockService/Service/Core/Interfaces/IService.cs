using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace BedrockService.Service.Core
{
    public interface IService : IHostedService
    {
        Task InitializeHost();
    }
}
