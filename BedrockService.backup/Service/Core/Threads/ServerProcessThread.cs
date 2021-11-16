using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class ServerProcessThread : IServiceThread
    {
        private Thread _networkListenerThread;

        public ServerProcessThread(ThreadStart methodToRun)
        {
            _networkListenerThread = new Thread(methodToRun)
            {
                Name = "ServerThread",
                IsBackground = true
            };
            _networkListenerThread.Start();
        }

        public void CloseThread()
        {
            if (IsAlive())
            {
                _networkListenerThread.Abort();
            }
            _networkListenerThread = null;
        }


        public bool IsAlive()
        {
            return _networkListenerThread != null && _networkListenerThread.IsAlive;
        }
    }
}
