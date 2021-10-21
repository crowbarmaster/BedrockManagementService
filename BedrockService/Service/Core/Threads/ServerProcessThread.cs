using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class ServerProcessThread : IServiceThread
    {
        Thread networkListenerThread;

        public ServerProcessThread(ThreadStart methodToRun)
        {
            networkListenerThread = new Thread(methodToRun)
            {
                Name = "ServerThread",
                IsBackground = true
            };
            networkListenerThread.Start();
        }

        public void CloseThread()
        {
            if (IsAlive())
            {
                networkListenerThread.Abort();
            }
            networkListenerThread = null;
        }


        public bool IsAlive()
        {
            return networkListenerThread != null && networkListenerThread.IsAlive;
        }
    }
}
