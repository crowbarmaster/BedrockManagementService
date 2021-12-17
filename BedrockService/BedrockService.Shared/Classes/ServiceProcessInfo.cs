using BedrockService.Shared.Interfaces;

namespace BedrockService.Shared.Classes {
    public class ServiceProcessInfo : IProcessInfo {
        private readonly string _serviceDirectory;
        private readonly int _processPid;
        private bool _debugEnabled;
        private bool _isConsoleMode;

        public ServiceProcessInfo(string serviceDirectory, int processPid, bool debugEnabled, bool isConsoleMode) {
            _serviceDirectory = serviceDirectory;
            _processPid = processPid;
            _debugEnabled = debugEnabled;
            _isConsoleMode = isConsoleMode;
        }

        public string GetDirectory() => _serviceDirectory;

        public int GetProcessPID() => _processPid;

        public bool IsDebugEnabled() => _debugEnabled;

        public bool IsConsoleMode() => _isConsoleMode;

        public void SetArguments(bool isDebug, bool isConsole) {
            _debugEnabled = isDebug;
            _isConsoleMode = isConsole;
        }
    }
}
