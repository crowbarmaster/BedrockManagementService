using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class WatchdogThread : IServiceThread
    {
        Thread thread;

        public WatchdogThread(ThreadStart parameterizedThreadStart)
        {
            thread = new Thread(parameterizedThreadStart)
            {
                Name = "WatchdogThread",
                IsBackground = true
            };
            thread.Start();
        }

        public void CloseThread()
        {
            thread.Abort();
            thread = null;
        }

        public bool IsAlive()
        {
            return thread != null && thread.IsAlive;
        }
    }

}

