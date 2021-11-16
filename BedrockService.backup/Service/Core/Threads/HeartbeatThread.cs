using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class HeartbeatThread : IServiceThread
    {
        private Thread _heartbeatThread;

        public HeartbeatThread(ThreadStart parameterizedThreadStart)
        {
            _heartbeatThread = new Thread(parameterizedThreadStart)
            {
                Name = "HeartbeatThread",
                IsBackground = true
            };
            _heartbeatThread.Start();
        }

        public void CloseThread()
        {
            _heartbeatThread.Abort();
            _heartbeatThread = null;
        }

        public bool IsAlive()
        {
            return _heartbeatThread != null && _heartbeatThread.IsAlive;
        }
    }
}
