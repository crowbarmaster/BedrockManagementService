using NLog;

namespace BedrockService.Shared.Interfaces {
    public interface IServerLogger {
        void Initialize();
        void AppendLine(string incomingText);
        void AppendErrorFromException(System.Exception exception);
        int Count();
        string FromIndex(int index);
        LogFactory GetNLogFactory();
        string ToString();
    }
}
