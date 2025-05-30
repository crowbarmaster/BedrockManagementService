﻿using MinecraftService;
using MinecraftService.Service;
using MinecraftService.Service.Server;
using MinecraftService.Service.Server.ConsoleFilters;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Service.Server.ServerControllers;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;
using MinecraftService.Shared.Utilities;
using NCrontab;
using System.IO.Compression;
using System.Timers;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Server.ServerControllers
{
    public class BedrockServer : IServerController {
        private Task? _serverTask;
        private Task? _watchdogTask;
        private CancellationTokenSource _serverCanceler = new();
        private CancellationTokenSource _watchdogCanceler = new();
        private StreamWriter _stdInStream;
        private Process? _serverProcess;
        private ServerStatus _currentServerStatus;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly UserConfigManager _configurator;
        private readonly MmsLogger _logger;
        private readonly ProcessInfo _processInfo;
        private readonly PlayerManager _playerManager;
        private readonly IBackupManager _backupManager;
        private TimerService _timerService;
        private MmsLogger _serverLogger;
        private List<Player> _connectedPlayers = new();
        private DateTime _startTime;
        private const string _startupMessage = "INFO] Server started.";
        private bool _serverModifiedFlag = true;
        private ConsoleGarbageFilter _garbageFilter = new ConsoleGarbageFilter();
        private ConsoleFilterStrategyClass _consoleFilter;

        public BedrockServer(IServerConfiguration serverConfiguration, UserConfigManager configurator, MmsLogger logger, ServiceConfigurator serviceConfiguration, ProcessInfo processInfo) {
            _serverConfiguration = serverConfiguration;
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _playerManager = serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue() || processInfo.DeclaredType() == "Client" ? serviceConfiguration.PlayerManager : serverConfiguration.GetPlayerManager();
            _configurator = configurator;
            _logger = logger;
            _serverLogger = new MmsLogger(_processInfo, (ServiceConfigurator)_serviceConfiguration, _serverConfiguration);
            _backupManager = new BedrockBackupManager(_logger, this, serverConfiguration, serviceConfiguration);
            _consoleFilter = new ConsoleFilterStrategyClass(_logger, _configurator, _serverConfiguration, this, _serviceConfiguration);
        }

        public void Initialize() {
            _serverLogger.Initialize();
            _serverConfiguration.GetUpdater().SetNewLogger(_serverLogger);
            _timerService = new TimerService(this, _serverConfiguration, _serviceConfiguration);
        }

        public void CheckUpdates() {
            _serverConfiguration.GetUpdater().CheckLatestVersion().Wait();
        }

        public void StartWatchdog() {
            _watchdogCanceler = new CancellationTokenSource();
            _watchdogTask = null;
            _watchdogTask = ApplicationWatchdogMonitor();
            _watchdogTask.Start();
        }

        public Task ServerStart() {
            return Task.Run(() => {
                string exeName = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                if (ProcessUtilities.MonitoredAppExists(appName)) {
                    ProcessUtilities.KillProcessList(Process.GetProcessesByName(appName));
                    Task.Delay(500).Wait();
                }
                _serverConfiguration.ValidateDeployedServer();
                StartServerTask();
                _timerService.StartTimerService();
                while (_currentServerStatus != ServerStatus.Started) {
                    Task.Delay(10).Wait();
                }

                _startTime = DateTime.Now;
            });
        }

        public Task ServerStop(bool stopWatchdog) {
            return Task.Run(() => {
                while (_backupManager.BackupRunning()) {
                    Task.Delay(100).Wait();
                }
                if (stopWatchdog) {
                    StopWatchdog().Wait();
                }
                _timerService.StopTimerService();
                if (_currentServerStatus != ServerStatus.Started) {
                    if (_serverProcess != null) {
                        _serverProcess.Kill();
                        Task.Delay(500).Wait();
                    }
                    _currentServerStatus = ServerStatus.Stopped;
                } else {
                    _currentServerStatus = ServerStatus.Stopping;
                    WriteToStandardIn("stop");
                }
            });
        }

        public Task RestartServer() {
            return Task.Run(() => {
                ServerStop(false).Wait();
                ServerStart().Wait();
            });
        }

        public void PerformOfflineServerTask(Action taskToRun) {
            if (_currentServerStatus == ServerStatus.Started) {
                ServerStop(false).Wait();
                Task.Run(taskToRun).Wait();
                ServerStart().Wait();
            } else {
                Task.Run(taskToRun).Wait();
            }
        }

        public void ForceKillServer() => _serverProcess.Kill();

        public string GetServerName() => _serverConfiguration.GetServerName();

        public void WriteToStandardIn(string command) {
            if (_stdInStream != null) {
                _stdInStream.WriteLine(command);
            }
        }

        public bool RollbackToBackup(string zipFilePath) {
            try {
                PerformOfflineServerTask(new Action(() => PerformRollback(zipFilePath)));
                return true;
            } catch (IOException e) {
                _logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
        }

        private void PerformRollback(string zipFilePath) => _backupManager.PerformRollback(zipFilePath);

        public ServerStatusModel GetServerStatus() => new() {
            ServerUptime = _startTime,
            ServerStatus = _currentServerStatus,
            ActivePlayerList = _connectedPlayers,
            ServerIndex = _serviceConfiguration.GetServerIndex(_serverConfiguration),
            TotalBackups = _serverConfiguration.GetStatus().TotalBackups,
            TotalSizeOfBackups = _serverConfiguration.GetStatus().TotalSizeOfBackups,
            DeployedVersion = _serverConfiguration.GetServerVersion()
        };

        public List<Player> GetActivePlayerList() => _connectedPlayers;

        public MmsLogger GetLogger() => _serverLogger;

        public MmsLogger GetServiceLogger() => _logger;

        public bool IsServerModified() => _serverModifiedFlag;

        public void SetServerModified(bool isModified) => _serverModifiedFlag = isModified;

        public bool ServerAutostartEnabled() => _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerAutostartEnabled).GetBoolValue();

        public bool IsPrimaryServer() => _serverConfiguration.IsPrimaryServer();

        public PlayerManager GetPlayerManager() => _playerManager;

        private Task StopWatchdog() {
            return Task.Run(() => {
                _watchdogCanceler.Cancel();
                while (_watchdogTask != null && !_watchdogTask.IsCompleted) {
                    Task.Delay(200);
                }
            });
        }

        private void StartServerTask() {
            _logger.AppendLine($"Recieved start signal for server {_serverConfiguration.GetServerName()}.");
            _serverCanceler = new CancellationTokenSource();
            _serverTask = RunServer();
            _serverTask.Start();
            _configurator.SaveServerConfiguration(_serverConfiguration);
        }

        private void StdOutToLog(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                string input = e.Data;
                string logFileText = "NO LOG FILE! - ";
                int trimIndex = 0;
                if (input.StartsWith(logFileText))
                    input = input.Substring(logFileText.Length);
                if (input.StartsWith('[')) {
                    trimIndex = input.IndexOf(']') + 2;
                    input = input.Substring(trimIndex);
                }
                input = _garbageFilter.Filter(input);
                if (!string.IsNullOrEmpty(input)) {
                    _serverLogger.AppendLine(input);
                    if (input.Equals("Quit correctly")) {
                        _logger.AppendLine($"Server {GetServerName()} received quit signal.");
                        _currentServerStatus = ServerStatus.Stopped;
                    }
                    if (input.Contains("Changes to the world are resumed")) {
                        _backupManager.SetBackupComplete();
                    }
                    foreach (KeyValuePair<string, IConsoleFilter> filter in _consoleFilter.FilterList) {
                        if (input.Contains(filter.Key)) {
                            filter.Value.Filter(input);
                            break;
                        }
                    }
                }
            }
        }

        private Task ApplicationWatchdogMonitor() {
            return new Task(() => {
                while (true) {
                    int procId = _serverConfiguration.GetRunningPid();
                    bool appExists = ProcessUtilities.MonitoredAppExists(procId);
                    if (!appExists && _currentServerStatus == ServerStatus.Started && !_watchdogCanceler.IsCancellationRequested) {
                        _logger.AppendLine($"Started application {_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)} was not found in running processes... Resarting.");
                        _currentServerStatus = ServerStatus.Stopped;
                        ServerStart().Wait();
                    }
                    if (_watchdogCanceler.IsCancellationRequested) {
                        break;
                    }
                    Task.Delay(1000).Wait();
                }
            }, _watchdogCanceler.Token);
        }

        private Task RunServer() {
            return new Task(() => {
                string exeName = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                _configurator.WriteJSONFiles(_serverConfiguration);
                MinecraftFileUtilities.WriteServerPropsFile(_serverConfiguration);

                try {
                    if (File.Exists(GetServerFilePath(ServerFileNameKeys.MmsServer_Name, _serverConfiguration, _serverConfiguration.GetServerName()))) {
                        if (ProcessUtilities.MonitoredAppExists(appName)) {
                            ProcessUtilities.KillProcessList(Process.GetProcessesByName(appName));
                        }
                        CreateProcess();
                    } else {
                        _logger.AppendLine($"The Bedrock Server is not accessible at {$@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}"}\r\nCheck if the file is at that location and that permissions are correct.");
                    }
                } catch (Exception e) {
                    _logger.AppendLine($"Error Running Bedrock Server: {e.Message}\n{e.StackTrace}");
                }
            }, _serverCanceler.Token);
        }

        private void CreateProcess() {
            string fileName = $@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue} ";
            ProcessStartInfo processStartInfo = new() {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = fileName
            };
            _serverProcess = Process.Start(processStartInfo);
            if (_serverProcess != null) {
                _serverConfiguration.SetRunningPid(_serverProcess.Id);
                _serverProcess.PriorityClass = ProcessPriorityClass.High;
                _serverProcess.OutputDataReceived += StdOutToLog;
                _serverProcess.BeginOutputReadLine();
                _stdInStream = _serverProcess.StandardInput;
                _serverProcess.EnableRaisingEvents = false;
            }
        }

        public void RunStartupCommands() {
            foreach (StartCmdEntry cmd in _serverConfiguration.GetStartCommands()) {
                _stdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        public IBackupManager GetBackupManager() => _backupManager;

        public void SetStartupStatus(ServerStatus status) => _currentServerStatus = status;

        public bool IsServerStarted() => _currentServerStatus == ServerStatus.Started;

        public bool IsServerStopped() => _currentServerStatus == ServerStatus.Stopped;
    }
}
