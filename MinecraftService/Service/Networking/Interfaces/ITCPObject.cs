using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Service.Networking.Interfaces
{
    public interface ITCPObject {
        void Initialize();
        Task Begin();
        Task CancelAllTasks();
        void SetStrategies(Dictionary<MessageTypes, IMessageParser> strategies);
        void SetServiceStarted();
        void SetServiceStopped();
    }
}
