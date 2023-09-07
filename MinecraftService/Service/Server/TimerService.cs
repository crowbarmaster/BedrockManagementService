using MinecraftService.Service.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server {
    public class TimerService {
        public List<TimerWorker> ActiveTimers { get; set; } = new();
        private IServerController _runningServer;
        private IServerConfiguration _parentServerConfig;
        private IServiceConfiguration _serviceConfiguration;

        public TimerService(IServerController runningServer, IServerConfiguration serverConfiguration, IServiceConfiguration service) {
            _runningServer = runningServer;
            _parentServerConfig = serverConfiguration;
            _serviceConfiguration = service;
        }

        public void StartTimer(MmsTimerTypes timerType) {
            TimerWorker worker;
            foreach(TimerWorker entry in ActiveTimers) {
                if(entry.GetTimerType() == timerType) {
                    return;
                }
            }
            worker = CreateTimer(timerType);
            if (worker.Start()) {
                ActiveTimers.Add(worker);
            }
        }

        public void StopTimer(MmsTimerTypes timerType) {
            TimerWorker removeWorker = null;
            foreach (TimerWorker worker in ActiveTimers) {
                if (worker.GetTimerType() == timerType) {
                    worker.Stop();
                    removeWorker = worker;
                }
            }
            if(removeWorker != null) {
                ActiveTimers.Remove(removeWorker);
            }
        }

        public void DisposeAllTimers() {
            foreach (TimerWorker worker in ActiveTimers) {
                worker.Stop();
            }
            ActiveTimers.Clear();
        }

        private TimerWorker CreateTimer(MmsTimerTypes timerType) => new TimerWorker(_runningServer, _parentServerConfig, _serviceConfiguration, timerType);
    }
}
