using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class WatchdogThread : IServiceThread
    {
        private Thread _thread;

        public WatchdogThread(ThreadStart parameterizedThreadStart)
        {
            _thread = new Thread(parameterizedThreadStart)
            {
                Name = "WatchdogThread",
                IsBackground = true
            };
            _thread.Start();
        }

        public void CloseThread()
        {
            _thread.Abort();
            _thread = null;
        }

        public bool IsAlive()
        {
            return _thread != null && _thread.IsAlive;
        }
    }

}

