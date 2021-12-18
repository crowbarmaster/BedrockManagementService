using BedrockService.Service.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace BedrockService.Service.Core.Threads
{
    class TCPThread : IServiceThread
    {
        private Thread _clientService;

        public TCPThread(ThreadStart threadStart)
        {
            _clientService = null;
            _clientService = new Thread(threadStart)
            {
                Name = "TCPListener",
                IsBackground = true
            };
            _clientService.Start();
        }

        public void CloseThread()
        {
            Task.Run(() => 
            {
                if (IsAlive())
                {
                    _clientService.Abort();
                }
            });
            _clientService = null;
        }

        public bool IsAlive()
        {
            return _clientService != null && _clientService.IsAlive;
        }
    }
}
