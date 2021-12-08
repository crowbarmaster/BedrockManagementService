using BedrockService.Service.Core.Interfaces;
using System.Threading;

namespace BedrockService.Service.Core.Tasks
{
    class HeartbeatTask : IServiceTask
    {
        private Task _thread;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public HeartbeatTask(Action<CancellationToken> method, CancellationTokenSource source)
        {
            _cancellationTokenSource = source;
            _thread = Task.Factory.StartNew((CancellationToken) => method(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        public void CancelTask() => _cancellationTokenSource.Cancel();
        public bool IsAlive() => _thread != null && _thread.Status == TaskStatus.Running && !_cancellationTokenSource.IsCancellationRequested;
    }
}