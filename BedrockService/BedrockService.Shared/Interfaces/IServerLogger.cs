using NLog;

namespace BedrockService.Shared.Interfaces {
    public interface IServerLogger {
        void Initialize();
        void AppendLine(string incomingText);
        int Count();
        string FromIndex(int index);
        LogFactory GetNLogFactory();
        string ToString();
    }
}
