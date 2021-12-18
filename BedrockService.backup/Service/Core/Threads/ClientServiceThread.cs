using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Threads
{
    class ClientServiceThread : IServiceThread
    {
        private Thread _clientService;

        public ClientServiceThread(ThreadStart methodToRun)
        {
            _clientService = new Thread(methodToRun)
            {
                Name = "ClientService",
                IsBackground = true
            };
            _clientService.Start();
        }

        public void CloseThread()
        {
            try
            {
                if (IsAlive())
                {
                    _clientService.Abort();
                }
                _clientService = null;
            }
            catch (ThreadAbortException) { }
        }

        public bool IsAlive()
        {
            return _clientService != null && _clientService.IsAlive;
        }
    }
}
