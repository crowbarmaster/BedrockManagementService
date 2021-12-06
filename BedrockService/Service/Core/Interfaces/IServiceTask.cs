namespace BedrockService.Service.Core.Interfaces
{
    public interface IServiceTask
    {
        bool IsAlive();
        void CancelTask();
    }
}
