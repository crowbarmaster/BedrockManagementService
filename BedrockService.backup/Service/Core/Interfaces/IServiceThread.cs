namespace BedrockService.Service.Core.Interfaces
{
    public interface IServiceThread
    {
        bool IsAlive();
        void CloseThread();
    }
}
