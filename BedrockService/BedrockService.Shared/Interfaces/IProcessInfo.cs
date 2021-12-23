namespace BedrockService.Shared.Interfaces {
    public interface IProcessInfo {
        string DeclaredType();

        string GetDirectory();

        int GetProcessPID();

        bool IsDebugEnabled();
    }
}
