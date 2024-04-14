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

        public void StartTimerService() {
            for (int i = 0; i < Enum.GetNames(typeof(MmsTimerTypes)).Length; i++) {
                StartTimer((MmsTimerTypes)i);
            }
        }

        public void StopTimerService() {
            foreach (TimerWorker worker in ActiveTimers) {
                worker.Stop().Wait();
            }
            ActiveTimers.Clear();
        }

        public void StartTimer(MmsTimerTypes timerType) {
            foreach (TimerWorker entry in ActiveTimers) {
                if (entry.GetTimerType() == timerType) {
                    return;
                }
            }
            TimerWorker worker;
            worker = CreateTimer(timerType);
            
            if (worker.Start().Result) {
                ActiveTimers.Add(worker);
            }
        }

        public void StopTimer(MmsTimerTypes timerType) {
            TimerWorker removeWorker = null;
            foreach (TimerWorker worker in ActiveTimers) {
                if (worker.GetTimerType() == timerType) {
                    worker.Stop().Wait();
                    removeWorker = worker;
                }
            }
            if (removeWorker != null) {
                ActiveTimers.Remove(removeWorker);
            }
        }

        private TimerWorker CreateTimer(MmsTimerTypes timerType) => new TimerWorker(_runningServer, _parentServerConfig, _serviceConfiguration, timerType);
    }
}
