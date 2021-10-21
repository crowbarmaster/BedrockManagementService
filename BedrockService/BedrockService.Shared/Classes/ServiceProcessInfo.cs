using BedrockService.Shared.Interfaces;

namespace BedrockService.Shared.Classes
{
    public class ServiceProcessInfo : IProcessInfo
    {
        private readonly string serviceDirectory;
        private readonly string serviceExeName;
        private readonly int processPid;
        private bool debugEnabled;
        private bool isConsoleMode;

        public ServiceProcessInfo(string serviceDirectory, string serviceExeName, int processPid, bool debugEnabled, bool isConsoleMode)
        {
            this.serviceDirectory = serviceDirectory;
            this.serviceExeName = serviceExeName;
            this.processPid = processPid;
            this.debugEnabled = debugEnabled;
            this.isConsoleMode = isConsoleMode;
        }

        public string GetDirectory() => serviceDirectory;

        public string GetExecutableName() => serviceExeName;

        public int GetProcessPID() => processPid;

        public bool IsDebugEnabled() => debugEnabled;

        public bool IsConsoleMode() => isConsoleMode;

        public void SetArguments(bool isDebug, bool isConsole)
        {
            debugEnabled = isDebug;
            isConsoleMode = isConsole;
        }
    }
}
