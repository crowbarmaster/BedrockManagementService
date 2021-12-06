using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Tasks
{
    class ClientServiceTask : IServiceTask
    {
        private Task _thread;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public ClientServiceTask(Action<CancellationToken> method) => _thread = Task.Factory.StartNew((CancellationToken) => method(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        public void CancelTask() => _cancellationTokenSource.Cancel();
        public bool IsAlive() => _thread != null && _thread.Status == TaskStatus.Running && !_cancellationTokenSource.IsCancellationRequested;
    }
}