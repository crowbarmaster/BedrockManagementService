namespace BedrockService.Shared.Interfaces
{
    public interface ILogger
    {
        void AppendLine(string incomingText);
        void AppendText(string incomingText);
        int Count();
        string FromIndex(int index);
        string ToString();
    }
}
