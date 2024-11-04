using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Shared.Classes.Server
{
    public class ProcessInfo
    {
        private readonly string _declaredType;
        private readonly string _serviceDirectory;
        private readonly int _processPid;
        private readonly bool _debugEnabled;

        public ProcessInfo(string declaredType, string serviceDirectory, int processPid, bool debugEnabled, bool shouldStartService, string serverName = null)
        {
            _declaredType = declaredType;
            _serviceDirectory = serviceDirectory;
            _processPid = processPid;
            _debugEnabled = debugEnabled;
        }

        public ProcessInfo(ProcessInfo spi)
        {
            _declaredType = spi.DeclaredType();
            _serviceDirectory = spi.GetDirectory();
            _processPid = spi.GetProcessPID();
            _debugEnabled = spi.IsDebugEnabled();
        }

        public string DeclaredType() => _declaredType;

        public string GetDirectory() => _serviceDirectory;

        public int GetProcessPID() => _processPid;

        public bool IsDebugEnabled() => _debugEnabled;
    }
}
