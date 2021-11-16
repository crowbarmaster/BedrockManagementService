namespace BedrockService.Shared.Interfaces
{
    public interface IBedrockLogger
    {
        void AppendLine(string incomingText);
        void AppendText(string incomingText);
        int Count();
        string FromIndex(int index);
        string ToString();
    }
}
