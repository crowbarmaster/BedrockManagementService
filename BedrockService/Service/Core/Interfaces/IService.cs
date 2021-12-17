namespace BedrockService.Service.Core.Interfaces {
    public interface IService : IHostedService {
        Task InitializeHost();
    }
}
