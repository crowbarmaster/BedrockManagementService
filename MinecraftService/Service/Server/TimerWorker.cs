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
        private IServerLogger _serviceLogger;
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
            _timerType = timerType;
            _serviceLogger = runningServer.GetServiceLogger();
        }

        public Task<bool> Start() => Task.Run(() => {
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
                    _serviceLogger.AppendLine(_initializeMessage);
                }
            }
            return true;
        });

        public Task Stop() => Task.Run(() => {
            cancelSource.Cancel();
            _timer.Stop();
            _timer = null;
        });

        public MmsTimerTypes GetTimerType() => _timerType;

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                if (_firedEventLogic == null || _firedEventLogic.Status != TaskStatus.Created) {
                    return;
                }
                _firedEventLogic.Start();
                _firedEventLogic.Wait();
                Stop().Wait();
                Start();
            } catch (Exception ex) {
                Stop().Wait();
                _serviceLogger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
                Start();
            }
        }

        private void TryParseCron(string value, out CrontabSchedule? result) {
            try {
                result = CrontabSchedule.Parse(value);
                return;
            } catch (CrontabException ex) {
                _serviceLogger.AppendLine($"Crontab error occured: {ex.Message}");
            }
            result = null;
        }

        private bool InitMessagesActions() {
            if (_timerType == null) {
                return false;
            }
            switch (_timerType) {
                case MmsTimerTypes.Update:
                    _enableTest = new Func<bool>(() => _parentServerConfig.GetSettingsProp(ServerPropertyKeys.CheckUpdatesEnabled).GetBoolValue());
                    TryParseCron(_parentServerConfig.GetSettingsProp(ServerPropertyKeys.UpdateCron).ToString(), out _cron);
                    if (_cron == null) {
                        _serviceLogger.AppendLine("Timer execution has been halted! Please check server config.");
                        return false;
                    }
                    _initializeMessage = $"Automatic updates for server {_parentServerConfig.GetServerName()} enabled, will be checked at: {_cron.GetNextOccurrence(DateTime.Now):G}.";
                    _firedEventLogic = new Task(() => {
                        _parentServerConfig.GetUpdater().CheckLatestVersion().Wait();
                        if (_parentServerConfig.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() && _parentServerConfig.GetDeployedVersion() != _serviceConfiguration.GetLatestVersion(_parentServerConfig.GetServerArch())) {
                            _serviceLogger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                            _parentServerController.PerformOfflineServerTask(new Action(() => _parentServerConfig.GetUpdater().ReplaceServerBuild(_serviceConfiguration.GetLatestVersion(_parentServerConfig.GetServerArch())).Wait()));
                        }
                    }, cancelSource.Token);
                    return true;
                    break;
                case MmsTimerTypes.Backup:
                    _enableTest = new Func<bool>(() => _parentServerConfig.GetSettingsProp(ServerPropertyKeys.AutoBackupEnabled).GetBoolValue());
                    TryParseCron(_parentServerConfig.GetSettingsProp(ServerPropertyKeys.BackupCron).ToString(), out _cron);
                    if (_cron == null) {
                        _serviceLogger.AppendLine("Timer execution has been halted! Please check server config.");
                        return false;
                    }
                    _initializeMessage = $"Automatic backups for server {_parentServerConfig.GetServerName()} enabled, next backup at: {_cron.GetNextOccurrence(DateTime.Now):G}.";
                    _firedEventLogic = new Task(() => {
                        bool shouldBackup = _parentServerConfig.GetSettingsProp(ServerPropertyKeys.IgnoreInactiveBackups).GetBoolValue();
                        if ((shouldBackup && _parentServerController.IsServerModified()) || !shouldBackup) {
                            _parentServerController.GetBackupManager().InitializeBackup();
                        } else {
                            _serviceLogger.AppendLine($"Backup for server {_parentServerConfig.GetServerName()} was skipped due to inactivity.");
                        }

                    }, cancelSource.Token);
                    break;
                case MmsTimerTypes.Restart:
                    _enableTest = new Func<bool>(() => _parentServerConfig.GetSettingsProp(ServerPropertyKeys.AutoRestartEnabled).GetBoolValue());
                    TryParseCron(_parentServerConfig.GetSettingsProp(ServerPropertyKeys.RestartCron).ToString(), out _cron);
                    if (_cron == null) {
                        _serviceLogger.AppendLine("Timer execution has been halted! Please check server config.");
                        return false;
                    }
                    _initializeMessage = $"Automatic server restarts for server {_parentServerConfig.GetServerName()} enabled, next scheduled restart at: {_cron.GetNextOccurrence(DateTime.Now):G}.";
                    _firedEventLogic = new Task(() => {
                        _parentServerController.RestartServer();
                    }, cancelSource.Token);
                    break;
                default:
                    return false;
                    break;
            }
            return false;
        }
    }
}
