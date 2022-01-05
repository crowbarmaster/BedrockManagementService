using BedrockService.Shared.Interfaces;

namespace BedrockService.Shared.Classes {
    public class ServiceProcessInfo : IProcessInfo {
        private readonly string _declaredType;
        private readonly string _serviceDirectory;
        private readonly string _serverName;
        private readonly int _processPid;
        private readonly bool _debugEnabled;

        public ServiceProcessInfo(string declaredType, string serviceDirectory, int processPid, bool debugEnabled, bool shouldStartService, string serverName = null) {
            _declaredType = declaredType;
            _serviceDirectory = serviceDirectory;
            _processPid = processPid;
            _debugEnabled = debugEnabled;
            _serverName = serverName;
        }

        public string DeclaredType() => _declaredType;

        public string GetServerName() => _serverName;

        public string GetDirectory() => _serviceDirectory;

        public int GetProcessPID() => _processPid;

        public bool IsDebugEnabled() => _debugEnabled;
    }
}
