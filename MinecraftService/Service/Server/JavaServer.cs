using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;
using MinecraftService.Shared.Utilities;
using NCrontab;
using System.IO.Compression;
using System.Timers;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server {
    public class JavaServer : IServerController {
        private Task? _serverTask;
        private Task? _watchdogTask;
        private CancellationTokenSource _serverCanceler = new();
        private CancellationTokenSource _watchdogCanceler = new();
        private StreamWriter _stdInStream;
        private Process? _serverProcess;
        private ServerStatus _currentServerStatus;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IServerLogger _serviceLogger;
        private readonly IProcessInfo _processInfo;
        private readonly IPlayerManager _playerManager;
        private readonly IBackupManager _backupManager;
        private TimerService _timerService;
        private IServerLogger _serverLogger;
        private List<IPlayer> _connectedPlayers = new();
        private DateTime _startTime;
        private bool _serverModifiedFlag = true;
        private const string _startupMessage = "INFO] Server started.";

        public JavaServer(IServerConfiguration serverConfiguration, IConfigurator configurator, IServerLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo, IPlayerManager servicePlayerManager) {
            _serverConfiguration = serverConfiguration;
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _playerManager = serviceConfiguration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue() || processInfo.DeclaredType() == "Client" ? servicePlayerManager : serverConfiguration.GetPlayerManager();
            _configurator = configurator;
            _serviceLogger = logger;
            _serverLogger = new MinecraftServerLogger(_processInfo, _serviceConfiguration, _serverConfiguration);
            _backupManager = new JavaBackupManager(_serviceLogger, this, _serverConfiguration, _serviceConfiguration);
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
                if (ProcessUtilities.JarProcessExists(GetServerName()) != 0) {
                    ProcessUtilities.KillJarProcess(GetServerName());
                    Task.Delay(500).Wait();
                }
                _serverConfiguration.ValidateDeployedServer();
                StartServerTask();
                while (_currentServerStatus != ServerStatus.Started) {
                    Task.Delay(10).Wait();
                }
                for (int i = 0; i < Enum.GetNames(typeof(MmsTimerTypes)).Length; i++) {
                    _timerService.StartTimer((MmsTimerTypes)i);
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
                _timerService.DisposeAllTimers();
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

        public ServerStatusModel GetServerStatus() => new() {
            ServerUptime = _startTime,
            ServerStatus = _currentServerStatus,
            ActivePlayerList = _connectedPlayers,
            ServerIndex = _serviceConfiguration.GetServerIndex(_serverConfiguration),
            TotalBackups = _serverConfiguration.GetStatus().TotalBackups,
            TotalSizeOfBackups = _serverConfiguration.GetStatus().TotalSizeOfBackups,
            DeployedVersion = _serverConfiguration.GetServerVersion()
        };

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
                _serviceLogger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
        }

        public void RunStartupCommands() {
            foreach (StartCmdEntry cmd in _serverConfiguration.GetStartCommands()) {
                _stdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        public void ForceKillServer() => _serverProcess.Kill();

        public string GetServerName() => _serverConfiguration.GetServerName();

        public List<IPlayer> GetActivePlayerList() => _connectedPlayers;

        public IServerLogger GetLogger() => _serverLogger;

        private void PerformRollback(string zipFilePath) => _backupManager.PerformRollback(zipFilePath);

        public bool IsServerModified() => _serverModifiedFlag;

        public void SetServerModified(bool isModified) => _serverModifiedFlag = isModified;

        public bool ServerAutostartEnabled() => _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerAutostartEnabled).GetBoolValue();

        public bool IsPrimaryServer() => _serverConfiguration.IsPrimaryServer();

        public IPlayerManager GetPlayerManager() => _playerManager;

        public IBackupManager GetBackupManager() => _backupManager;

        public void SetStartupStatus(ServerStatus status) => _currentServerStatus = status;

        public bool IsServerStarted() => _currentServerStatus == ServerStatus.Started;

        public bool IsServerStopped() => _currentServerStatus == ServerStatus.Stopped;

        private Task StopWatchdog() {
            return Task.Run(() => {
                _watchdogCanceler.Cancel();
                while (_watchdogTask != null && !_watchdogTask.IsCompleted) {
                    Task.Delay(200);
                }
            });
        }

        private void StartServerTask() {
            _serviceLogger.AppendLine($"Recieved start signal for server {_serverConfiguration.GetServerName()}.");
            _serverCanceler = new CancellationTokenSource();
            _serverTask = RunServer();
            _serverTask.Start();
            _configurator.SaveServerConfiguration(_serverConfiguration);
        }

        private void StdOutToLog(object sender, DataReceivedEventArgs e) {
            if (e.Data != null && !e.Data.Contains("Running AutoCompaction...")) {
                ConsoleFilterStrategyClass consoleFilter = new ConsoleFilterStrategyClass(_serviceLogger, _configurator, _serverConfiguration, this, _serviceConfiguration);
                string input = e.Data;
                string logFileText = "NO LOG FILE! - ";
                int trimIndex = 0;
                if (input.StartsWith(logFileText))
                    input = input.Substring(logFileText.Length);
                if (input.StartsWith('[')) {
                    trimIndex = input.IndexOf(']') + 2;
                }
                _serverLogger.AppendLine(input.Substring(trimIndex));
                if (e.Data != null) {
                    if (input.Contains("All dimensions are saved") && !_backupManager.BackupRunning()) {
                        _serviceLogger.AppendLine($"Server {GetServerName()} received quit signal.");
                        _currentServerStatus = ServerStatus.Stopped;
                    }
                    if (input.Contains("Automatic saving is now enabled")) {
                        _backupManager.SetBackupComplete();
                        WriteToStandardIn("say Server backup complete.");
                    }
                    foreach (KeyValuePair<string, IConsoleFilter> filter in consoleFilter.JavaFilterList) {
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
                        _serviceLogger.AppendLine($"Started application {_serverConfiguration.GetSettingsProp(ServerPropertyKeys.FileName)} was not found in running processes... Resarting.");
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
                _configurator.WriteJSONFiles(_serverConfiguration);
                MinecraftFileUtilities.WriteServerPropsFile(_serverConfiguration);

                try {
                    if (File.Exists(GetServerFilePath(ServerFileNameKeys.JavaServer_Name, _serverConfiguration, GetServerName()))) {
                        ProcessUtilities.KillJarProcess(GetServerName());
                        CreateProcess();
                    } else {
                        _serviceLogger.AppendLine($"The Java Server is not accessible at {$@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}"}\r\nCheck if the file is at that location and that permissions are correct.");
                    }
                } catch (Exception e) {
                    _serviceLogger.AppendLine($"Error Running Java Server: {e.Message}\n{e.StackTrace}");
                }
            }, _serverCanceler.Token);
        }

        private void CreateProcess() {
            string fileName = $"\"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue}\\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue}\"";
            ProcessStartInfo processStartInfo = new() {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = GetServiceFilePath(MmsFileNameKeys.Jdk17JavaMmsExe),
                WorkingDirectory = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).StringValue,
                Arguments = $"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.JavaArgs)} -\"DMinecraftService_{_serverConfiguration.GetServerName()}\" -jar {fileName} nogui",
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
    }
}
