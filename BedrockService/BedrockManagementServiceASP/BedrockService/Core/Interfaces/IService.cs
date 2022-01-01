namespace BedrockManagementServiceASP.BedrockService.Core.Interfaces {
    public interface IService : IHostedService {
        Task InitializeHost();
    }
}
