namespace BedrockService.Shared.Interfaces {
    public interface IClientSideServiceConfiguration {
        string GetAddress();
        string GetDisplayName();
        string GetHostName();
        string GetPort();
    }
}