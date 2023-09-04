using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server {
    public class TimerWorker {
        private IServiceConfiguration _serviceConfiguration;
        private IServerController _parentServerController;
        private IServerConfiguration _parentServerConfig;
        private IServerLogger _serverLogger;
        private MmsTimerTypes _timerType;
        private System.Timers.Timer? _timer;
        private CrontabSchedule? _cron;
        private string _initializeMessage = "Init message not set.";
        private Func<bool> _enableTest;
        private Task _firedEventLogic;
        private CancellationTokenSource cancelSource;

        public TimerWorker(IServerController runningServer, IServerConfiguration serverConfiguration, IServiceConfiguration service, MmsTimerTypes timerType) {
            _serviceConfiguration = service;
            _parentServerController = runningServer;
            _parentServerConfig = serverConfiguration;
            _serverLogger = runningServer.GetLogger();
            _timerType = timerType;
        }

        public bool Start() {
            cancelSource = new();
            InitMessagesActions();
            if (!_enableTest.Invoke()) {
                return false;
            }
            if (_cron != null) {
                double interval = (_cron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds;
                if (_timer != null) {
                    _timer.Stop();
                    _timer = null;
                }
                if (interval >= 0) {
                    _timer = new System.Timers.Timer(interval);
                    _timer.Elapsed += Timer_Elapsed;
                    _timer.Start();
                    _serverLogger.AppendLine(_initializeMessage);
                }
            }
            return true;
        }

        public void Stop() {
            cancelSource.Cancel();
            _timer.Stop();
            _timer = null;
        }

        public MmsTimerTypes GetTimerType() => _timerType;

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                if(_firedEventLogic == null || _firedEventLogic.Status != TaskStatus.Created) {
                    return;
                }
                _firedEventLogic.Start();
                _firedEventLogic.Wait();
                ((System.Timers.Timer)sender).Stop();
                Start();
            } catch (Exception ex) {
                ((System.Timers.Timer)sender).Stop();
                _serverLogger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
                Start();
            }
        }

        private void InitMessagesActions() {
            if(_timerType == null) {
                return;
            }
            switch (_timerType) {
                case MmsTimerTypes.Update:
                    _enableTest = new Func<bool>(() => _parentServerConfig.GetSettingsProp(ServerPropertyKeys.CheckUpdatesEnabled).GetBoolValue());
                    _cron = CrontabSchedule.TryParse(_parentServerConfig.GetSettingsProp(ServerPropertyKeys.UpdateCron).ToString());
                    _initializeMessage = $"Automatic updates Enabled, will be checked at: {_cron.GetNextOccurrence(DateTime.Now):G}.";
                    _firedEventLogic = new Task(() => {
                        _parentServerConfig.GetUpdater().CheckLatestVersion().Wait();
                        if (_parentServerConfig.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() && _parentServerConfig.GetDeployedVersion() != _serviceConfiguration.GetLatestVersion(_parentServerConfig.GetServerArch())) {
                            _serverLogger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                            _parentServerController.PerformOfflineServerTask(new Action(() => _parentServerConfig.GetUpdater().ReplaceServerBuild(_serviceConfiguration.GetLatestVersion(_parentServerConfig.GetServerArch())).Wait()));
                        }
                    }, cancelSource.Token);
                    break;
                case MmsTimerTypes.Backup:
                    _enableTest = new Func<bool>(() => _parentServerConfig.GetSettingsProp(ServerPropertyKeys.AutoBackupEnabled).GetBoolValue());
                    _cron = CrontabSchedule.TryParse(_parentServerConfig.GetSettingsProp(ServerPropertyKeys.BackupCron).ToString());
                    _initializeMessage = $"Automatic backups enabled, next backup at: {_cron.GetNextOccurrence(DateTime.Now):G}.";
                    _firedEventLogic = new Task(() => {
                        bool shouldBackup = _parentServerConfig.GetSettingsProp(ServerPropertyKeys.IgnoreInactiveBackups).GetBoolValue();
                        if ((shouldBackup && _parentServerController.IsServerModified()) || !shouldBackup) {
                            _parentServerController.GetBackupManager().InitializeBackup();
                        } else {
                            _serverLogger.AppendLine($"Backup for server {_parentServerConfig.GetServerName()} was skipped due to inactivity.");
                        }

                    }, cancelSource.Token);
                    break;
                case MmsTimerTypes.Restart:
                    _enableTest = new Func<bool>(() => _parentServerConfig.GetSettingsProp(ServerPropertyKeys.AutoRestartEnabled).GetBoolValue());
                    _cron = CrontabSchedule.TryParse(_parentServerConfig.GetSettingsProp(ServerPropertyKeys.RestartCron).ToString());
                    _initializeMessage = $"Automatic server restarts enabled, next scheduled restart at: {_cron.GetNextOccurrence(DateTime.Now):G}.";
                    _firedEventLogic = new Task(() => {
                        _parentServerController.RestartServer();
                    }, cancelSource.Token);
                    break;
                default:
                    break;
            }
        }
    }
}
