using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class HeartbeatThread : IServiceThread
    {
        Thread heartbeatThread;

        public HeartbeatThread(ThreadStart parameterizedThreadStart)
        {
            heartbeatThread = new Thread(parameterizedThreadStart)
            {
                Name = "HeartbeatThread",
                IsBackground = true
            };
            heartbeatThread.Start();
        }

        public void CloseThread()
        {
            heartbeatThread.Abort();
            heartbeatThread = null;
        }

        public bool IsAlive()
        {
            return heartbeatThread != null && heartbeatThread.IsAlive;
        }
    }
}
