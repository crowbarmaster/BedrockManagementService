namespace BedrockService.Shared.Interfaces {
    public interface IProcessInfo {
        string GetDirectory();

        int GetProcessPID();

        bool IsDebugEnabled();

        bool IsConsoleMode();

        void SetArguments(bool isDebug, bool isConsole);
    }
}
