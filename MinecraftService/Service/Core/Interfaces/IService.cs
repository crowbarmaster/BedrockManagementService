namespace MinecraftService.Service.Core.Interfaces {
    public interface IService : IHostedService {
        Task InitializeHost();
    }
}
