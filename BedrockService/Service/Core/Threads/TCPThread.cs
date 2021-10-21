using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class TCPThread : IServiceThread
    {
        Thread clientService;

        public TCPThread(ThreadStart threadStart)
        {
            clientService = new Thread(threadStart)
            {
                Name = "TCPListener",
                IsBackground = true
            };
            clientService.Start();
        }

        public void CloseThread()
        {
            if (IsAlive())
            {
                clientService.Abort();
            }
            clientService = null;
        }

        public bool IsAlive()
        {
            return clientService != null && clientService.IsAlive;
        }
    }
}
