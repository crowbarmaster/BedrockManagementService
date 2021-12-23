using BedrockService.Shared.Classes;

namespace BedrockService.Shared.Interfaces {
    public interface IBedrockLogger {
        void Initialize();
        void AppendLine(string incomingText);
        void AppendError(System.Exception exception);
        int Count();
        string FromIndex(int index);
        string ToString();
    }
}
