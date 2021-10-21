using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class ClientServiceThread : IServiceThread
    {
        Thread clientService;

        public ClientServiceThread(ThreadStart methodToRun)
        {
            clientService = new Thread(methodToRun)
            {
                Name = "ClientService",
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
