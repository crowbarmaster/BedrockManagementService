using BedrockService.Service.Management.Interfaces;
using BedrockService.Service.Networking.Interfaces;
using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.SerializeModels;
using NCrontab;
using System.IO.Compression;
using System.Timers;

namespace BedrockService.Service.Server {
    public class BedrockServer : IBedrockServer {
        private Task? _serverTask;
        private Task? _watchdogTask;
        private CancellationTokenSource _serverCanceler = new CancellationTokenSource();
        private CancellationTokenSource _watchdogCanceler = new CancellationTokenSource();
        private StreamWriter _stdInStream;
        private Process? _serverProcess;
        private ServerStatus _currentServerStatus;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly IUpdater _updater;
        private readonly IPlayerManager _playerManager;
        private readonly FileUtilities _fileUtils;
        private readonly BackupManager _backupManager;
        private System.Timers.Timer? _backupTimer { get; set; }
        private CrontabSchedule? _backupCron { get; set; }
        private CrontabSchedule? _updaterCron { get; set; }
        private System.Timers.Timer? _updaterTimer { get; set; }
        private IBedrockLogger _serverLogger;
        private List<IPlayer> _connectedPlayers = new List<IPlayer>();
        private DateTime _startTime;
        private const string _startupMessage = "INFO] Server started.";
        private bool _AwaitingStopSignal = true;
        private bool _backupRunning = false;
        private bool _serverModifiedFlag = true;
        private bool _LiteLoaderEnabledServer = false;
     
        public BedrockServer(IServerConfiguration serverConfiguration, IConfigurator configurator, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo, FileUtilities fileUtils, IPlayerManager servicePlayerManager) {
            _fileUtils = fileUtils;
            _serverConfiguration = serverConfiguration;
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _playerManager = serviceConfiguration.GetProp("GlobalizedPlayerDatabase").GetBoolValue() ? servicePlayerManager : new ServerPlayerManager(serverConfiguration);
            _configurator = configurator;
            _logger = logger;
            _backupManager = new BackupManager(_processInfo, _logger, this, serverConfiguration, serviceConfiguration);
            _updater = new Updater(_processInfo, _logger, _serviceConfiguration);
        }

        public void Initialize() {
            _serverLogger = new BedrockLogger(_processInfo, _serviceConfiguration, _serverConfiguration);
            _serverLogger.Initialize();
            _updater.Initialize();
            if (_serverConfiguration.GetSettingsProp("CheckUpdates").GetBoolValue()) {
                _updater.CheckLatestVersion().Wait();
            }
        }

        public void CheckUpdates() {
           _updater.CheckLatestVersion().Wait();
        }

        public void StartWatchdog() {
            _watchdogCanceler = new CancellationTokenSource();
            _watchdogTask = null;
            _watchdogTask = ApplicationWatchdogMonitor();
            _watchdogTask.Start();
        }

        public Task AwaitableServerStart() {
            return Task.Run(() => {
                string exeName = _serverConfiguration.GetSettingsProp("ServerExeName").ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                if (MonitoredAppExists(appName)) {
                    KillProcesses(Process.GetProcessesByName(appName));
                    Task.Delay(500).Wait();
                }
                StartServerTask();
                InitializeBackupTimer();
                InitializeUpdateTimer();
                while (_currentServerStatus != ServerStatus.Started) {
                    Task.Delay(10).Wait();
                }
                _startTime = DateTime.Now;
            });
        }

        public Task AwaitableServerStop(bool stopWatchdog) {
            return Task.Run(() => {
                if (_currentServerStatus != ServerStatus.Started) {
                    if (stopWatchdog) {
                        StopWatchdog().Wait();
                    }
                    if (_serverProcess != null) {
                        _serverProcess.Kill();
                        Task.Delay(500).Wait();
                    }
                    _currentServerStatus = ServerStatus.Stopped;
                    return;
                }
                while (_backupRunning) {
                    Task.Delay(100).Wait();
                }
                if (stopWatchdog) {
                    StopWatchdog().Wait();
                }
                _currentServerStatus = ServerStatus.Stopping;
                WriteToStandardIn("stop");
                while (_AwaitingStopSignal) {
                    Task.Delay(100).Wait();
                }
                _currentServerStatus = ServerStatus.Stopped;
                _AwaitingStopSignal = true;
                Task.Delay(500).Wait();
            });
        }

        public Task RestartServer() {
            return Task.Run(() => {
                AwaitableServerStop(false).Wait();
                AwaitableServerStart().Wait();
            });
        }

        public string GetServerName() => _serverConfiguration.GetServerName();

        public void WriteToStandardIn(string command) {
            if(_stdInStream != null) {
                _stdInStream.WriteLine(command);
            }
        }

        private void InitializeBackupTimer() {
             _backupCron = CrontabSchedule.TryParse(_serverConfiguration.GetSettingsProp("BackupCron").ToString());
            if (_serverConfiguration.GetSettingsProp("BackupEnabled").GetBoolValue() && _backupCron != null) {
                double interval = (_backupCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds;
                if (interval >= 0) {
                    if (_backupTimer != null) {
                        _backupTimer.Stop();
                        _backupTimer = null;
                    }                    
                    _backupTimer = new System.Timers.Timer(interval);
                    _logger.AppendLine($"Automatic backups for server {GetServerName()} enabled, next backup at: {_backupCron.GetNextOccurrence(DateTime.Now):G}.");
                    _backupTimer.Elapsed += BackupTimer_Elapsed;
                    _backupTimer.AutoReset = false;
                    _backupTimer.Start();
                }
            }
            if (!_serverConfiguration.GetSettingsProp("BackupEnabled").GetBoolValue()) {
                if (_backupTimer != null) {
                    _backupTimer.Stop();
                    _backupTimer.Dispose();
                    _backupTimer = null;
                }
            }
        }

        private void BackupTimer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                bool shouldBackup = _serverConfiguration.GetSettingsProp("IgnoreInactiveBackups").GetBoolValue();
                if ((shouldBackup && _serverModifiedFlag) || !shouldBackup) {
                    InitializeBackup();
                } else {
                    _logger.AppendLine($"Backup for server {GetServerName()} was skipped due to inactivity.");
                }
                InitializeBackupTimer();
            } catch (Exception ex) {
                _logger.AppendLine($"Error in BackupTimer_Elapsed {ex}");
            }
        }

        private void InitializeUpdateTimer() {
            _updaterCron = CrontabSchedule.TryParse(_serverConfiguration.GetSettingsProp("UpdateCron").ToString());
            if (_serverConfiguration.GetSettingsProp("CheckUpdates").GetBoolValue() && _updaterCron != null) {
                double interval = (_updaterCron.GetNextOccurrence(DateTime.Now) - DateTime.Now).TotalMilliseconds;
                if (interval >= 0) {
                    _updaterTimer = new System.Timers.Timer(interval);
                    _updaterTimer.Elapsed += UpdateTimer_Elapsed;
                    _logger.AppendLine($"Automatic updates Enabled, will be checked at: {_updaterCron.GetNextOccurrence(DateTime.Now):G}.");
                    _updaterTimer.Start();
                }
            }
            if (!_serverConfiguration.GetSettingsProp("CheckUpdates").GetBoolValue()) {
                if (_updaterTimer != null) {
                    _updaterTimer.Stop();
                    _updaterTimer.Dispose();
                    _updaterTimer = null;
                }
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                _updater.CheckLatestVersion().Wait();
                if (_serverConfiguration.GetSettingsProp("AutoDeployUpdates").GetBoolValue()) {
                    _logger.AppendLine("Version change detected! Restarting server(s) to apply update...");
                    RestartServer();
                }
                InitializeUpdateTimer();
            } catch (Exception ex) {
                _logger.AppendLine($"Error in UpdateTimer_Elapsed {ex}");
            }
        }

        public bool RollbackToBackup(byte serverIndex, string zipFilePath) {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            DirectoryInfo worldsDir = new DirectoryInfo($@"{server.GetSettingsProp("ServerPath")}\worlds");
            FileInfo backupZipFileInfo = new FileInfo($@"{server.GetSettingsProp("BackupPath")}\{server.GetServerName()}\{zipFilePath}");
            DirectoryInfo backupPacksDir = new DirectoryInfo($@"{worldsDir.FullName}\InstalledPacks");
            AwaitableServerStop(false).Wait();
            try {
                _fileUtils.DeleteFilesFromDirectory(worldsDir, true).Wait();
                _logger.AppendLine($"Deleted world folder \"{worldsDir.Name}\"");
                ZipFile.ExtractToDirectory(backupZipFileInfo.FullName, worldsDir.FullName);
                _logger.AppendLine($"Copied files from backup \"{backupZipFileInfo.Name}\" to server worlds directory.");
                MinecraftPackParser parser = new MinecraftPackParser(_processInfo);
                foreach (FileInfo file in backupPacksDir.GetFiles()) {
                    _fileUtils.ClearTempDir().Wait();
                    ZipFile.ExtractToDirectory(file.FullName, $@"{Path.GetTempPath()}\BMSTemp\PackTemp", true);
                    parser.FoundPacks.Clear();
                    parser.ParseDirectory($@"{Path.GetTempPath()}\BMSTemp\PackTemp");
                    if (parser.FoundPacks[0].ManifestType == "data") {
                        string folderPath = $@"{server.GetSettingsProp("ServerPath")}\development_behavior_packs\{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}";
                        Task.Run(() => _fileUtils.DeleteFilesFromDirectory(folderPath, false)).Wait();
                        ZipFile.ExtractToDirectory(file.FullName, folderPath, true);
                    }
                    if (parser.FoundPacks[0].ManifestType == "resources") {
                        string folderPath = $@"{server.GetSettingsProp("ServerPath")}\development_resource_packs\{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}";
                        Task.Run(() => _fileUtils.DeleteFilesFromDirectory(folderPath, false)).Wait();
                        ZipFile.ExtractToDirectory(file.FullName, folderPath, true);
                    }
                }
                AwaitableServerStart().Wait();
                return true;
            } catch (IOException e) {
                _logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
        }

        public ServerStatusModel GetServerStatus() => new ServerStatusModel() {
            ServerUptime = _startTime,
            ServerStatus = _currentServerStatus,
            ActivePlayerList = _connectedPlayers,
            ServerIndex = _serviceConfiguration.GetServerIndex(_serverConfiguration),
            TotalBackups = _serverConfiguration.GetStatus().TotalBackups,
            TotalSizeOfBackups = _serverConfiguration.GetStatus().TotalSizeOfBackups,
            DeployedVersion = _serverConfiguration.GetServerVersion()
        };

        public IBedrockLogger GetLogger() => _serverLogger;

        public bool IsServerModified() => _serverModifiedFlag;

        public void ForceServerModified() => _serverModifiedFlag = true;

        public bool ServerAutostartEnabled() => _serverConfiguration.GetSettingsProp("ServerAutostartEnabled").GetBoolValue();

        public bool IsPrimaryServer() {
            return _serverConfiguration.GetProp("server-port").StringValue == "19132" ||
            _serverConfiguration.GetProp("server-port").StringValue == "19133" ||
            _serverConfiguration.GetProp("server-portv6").StringValue == "19132" ||
            _serverConfiguration.GetProp("server-portv6").StringValue == "19133";
        }

        public IPlayerManager GetPlayerManager() => _playerManager;

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
        }

        private void StdOutToLog(object sender, DataReceivedEventArgs e) {
            if (e.Data != null && !e.Data.Contains("Running AutoCompaction...")) {
                string dataMsg = e.Data;
                string logFileText = "NO LOG FILE! - ";
                if (dataMsg.StartsWith(logFileText))
                    dataMsg = dataMsg.Substring(logFileText.Length, dataMsg.Length - logFileText.Length);
                _serverLogger.AppendLine(dataMsg);
                if (e.Data != null) {
                    if (dataMsg.Contains("[PreLoader]")) {
                        _serverConfiguration.SetLiteLoaderStatus(true);
                    }
                    if (dataMsg.Contains(_startupMessage) || dataMsg.Contains("[Server] Done")) {
                        _currentServerStatus = ServerStatus.Started;
                        Task.Delay(3000).Wait();
                        if (_serverConfiguration.GetStartCommands().Count > 0) {
                            RunStartupCommands();
                        }
                    }
                    if (dataMsg.Equals("Quit correctly")) {
                        _logger.AppendLine($"Server {GetServerName()} received quit signal.");
                        _AwaitingStopSignal = false;
                    }
                    if (dataMsg.Contains("Player connected")) {
                        var playerInfo = ExtractPlayerInfoFromString(dataMsg);
                        _logger.AppendLine($"Player {playerInfo.username} connected with XUID: {playerInfo.xuid}");
                        _serverModifiedFlag = true;
                        _connectedPlayers.Add(_playerManager.PlayerConnected(playerInfo.username, playerInfo.xuid));
                        _configurator.SavePlayerDatabase(_serverConfiguration);
                    }
                    if (dataMsg.Contains("Player disconnected")) {
                        var playerInfo = ExtractPlayerInfoFromString(dataMsg);
                        _logger.AppendLine($"Player {playerInfo.username} disconnected with XUID: {playerInfo.xuid}");
                        _connectedPlayers.Remove(_playerManager.PlayerDisconnected(playerInfo.xuid));
                        _configurator.SavePlayerDatabase(_serverConfiguration);
                    }
                    if (dataMsg.Contains("Failed to load Vanilla")) {
                        AwaitableServerStop(false).Wait();
                        string deployedVersion = _serverConfiguration.GetSelectedVersion() == "Latest" 
                                                ? _serviceConfiguration.GetLatestBDSVersion() 
                                                : _serverConfiguration.GetSelectedVersion();
                        _configurator.ReplaceServerBuild(_serverConfiguration, deployedVersion).Wait();
                        AwaitableServerStart().Wait();
                    }
                    if (dataMsg.Contains("Version ")) {
                        int msgStartIndex = dataMsg.IndexOf(']') + 2;
                        string focusedMsg = dataMsg.Substring(msgStartIndex, dataMsg.Length - msgStartIndex);
                        int versionIndex = focusedMsg.IndexOf(' ') + 1;
                        string versionString = focusedMsg.Substring(versionIndex, focusedMsg.Length - versionIndex);
                        if(_serverConfiguration.GetSettingsProp("DeployedVersion").StringValue == "None") {
                            _logger.AppendLine("Service detected version, restarting server to apply correct configuration.");
                            _serverProcess.Kill();
                            _serverConfiguration.GetSettingsProp("DeployedVersion").SetValue(versionString);
                            _serverConfiguration.ValidateVersion(versionString);
                            _configurator.LoadServerConfigurations().Wait();
                            _configurator.SaveServerConfiguration(_serverConfiguration);
                            AwaitableServerStart().Wait();
                        }
                        string deployedVersion = _serverConfiguration.GetSettingsProp("SelectedServerVersion").StringValue == "Latest"
                        ? _serviceConfiguration.GetLatestBDSVersion()
                        : _serverConfiguration.GetSettingsProp("SelectedServerVersion").StringValue;
                        if (versionString.ToLower().Contains("-beta")) {
                        int betaTagLoc = versionString.ToLower().IndexOf("-beta");
                        int betaVer = int.Parse(versionString.Substring(betaTagLoc + 5, versionString.Length - (betaTagLoc + 5)));
                            versionString = versionString.Substring(0, betaTagLoc) + ".";
                            versionString = versionString + betaVer;
                        }
                        if (deployedVersion != versionString && !versionString.Contains("LiteLoader")) {
                            if (_serverConfiguration.GetSettingsProp("AutoDeployUpdates").GetBoolValue()) {
                                _logger.AppendLine($"Server {GetServerName()} decected incorrect or out of date version! Replacing build...");
                                AwaitableServerStop(false).Wait();
                                _configurator.ReplaceServerBuild(_serverConfiguration, deployedVersion).Wait();
                                AwaitableServerStart();
                            } else {
                                _logger.AppendLine($"Server {GetServerName()} is out of date, Enable AutoDeployUpdates option to update to latest!");
                            }
                        }
                    }
                    if (dataMsg.Contains("A previous save has not been completed.")) {
                        Task.Delay(1000).Wait();
                        WriteToStandardIn("save query");
                    }
                    if (dataMsg.Contains($@"{_serverConfiguration.GetProp("level-name")}/db/")) {
                        if (dataMsg.Contains("[Server]")) {
                            dataMsg = dataMsg.Substring(dataMsg.IndexOf(']') + 2);
                        }
                        _logger.AppendLine("Save data string detected! Performing backup now!");
                        if (_backupManager.PerformBackup(dataMsg)) {
                            _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Completed.");
                            _backupRunning = false;
                            if (_connectedPlayers.Count == 0) {
                                _serverModifiedFlag = false;
                            }
                            return;
                        }
                        _backupRunning = false;
                        _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Failed. Check logs!");
                    }
                }
            }
        }

        public void InitializeBackup() {
            if(!_backupRunning && _currentServerStatus == ServerStatus.Started) {
                _backupRunning = true;
                WriteToStandardIn("save hold");
                Task.Delay(1000).Wait();
                WriteToStandardIn("say Server backup started.");
                WriteToStandardIn("save query");
            }
        }

        private Task ApplicationWatchdogMonitor() {
            return new Task(() => {
                while (true) {
                    string exeName = _serverConfiguration.GetSettingsProp("ServerExeName").ToString();
                    string appName = exeName.Substring(0, exeName.Length - 4);
                    bool appExists = MonitoredAppExists(appName);
                    if (!appExists && _currentServerStatus == ServerStatus.Started && !_watchdogCanceler.IsCancellationRequested) {
                        _logger.AppendLine($"Started application {appName} was not found in running processes... Resarting.");
                        _currentServerStatus = ServerStatus.Stopped;
                        AwaitableServerStart().Wait();
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
                string exeName = _serverConfiguration.GetSettingsProp("ServerExeName").ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                _configurator.WriteJSONFiles(_serverConfiguration);
                MinecraftFileUtilities.WriteServerPropsFile(_serverConfiguration);

                try {
                    if (File.Exists($@"{_serverConfiguration.GetSettingsProp("ServerPath")}\{_serverConfiguration.GetSettingsProp("ServerExeName")}")) {
                        if (MonitoredAppExists(appName)) {
                            Process[] processList = Process.GetProcessesByName(appName);
                            if (processList.Length != 0) {
                                _logger.AppendLine($@"Application {appName} was found running! Killing to proceed.");
                                KillProcesses(processList);
                            }
                        }
                        // Fires up a new process to run inside this one
                        CreateProcess();
                    } else {
                        _logger.AppendLine($"The Bedrock Server is not accessible at {$@"{_serverConfiguration.GetSettingsProp("ServerPath")}\{_serverConfiguration.GetSettingsProp("ServerExeName")}"}\r\nCheck if the file is at that location and that permissions are correct.");
                    }
                } catch (Exception e) {
                    _logger.AppendLine($"Error Running Bedrock Server: {e.Message}\n{e.StackTrace}");
                }
            }, _serverCanceler.Token);
        }

        private void CreateProcess() {
            string fileName = $@"{_serverConfiguration.GetSettingsProp("ServerPath")}\{_serverConfiguration.GetSettingsProp("ServerExeName")}";
            ProcessStartInfo processStartInfo = new ProcessStartInfo {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = fileName
            };
            _serverProcess = Process.Start(processStartInfo);
            if (_serverProcess != null) {
                _serverProcess.PriorityClass = ProcessPriorityClass.High;
                _serverProcess.OutputDataReceived += StdOutToLog;
                _serverProcess.BeginOutputReadLine();
                _stdInStream = _serverProcess.StandardInput;
                _serverProcess.EnableRaisingEvents = false;
            }
        }

        private void KillProcesses(Process[] processList) {
            foreach (Process process in processList) {
                try {
                    process.Kill();
                    Thread.Sleep(1000);
                    _logger.AppendLine($@"App {_serverConfiguration.GetSettingsProp("ServerExeName")} killed!");
                } catch (Exception e) {
                    _logger.AppendLine($"Killing proccess resulted in error: {e.StackTrace}");
                }
            }
        }

        private bool MonitoredAppExists(string monitoredAppName) {
            try {
                Process[] processList = Process.GetProcessesByName(monitoredAppName);
                if (processList.Length == 0) {
                    return false;
                } else {
                    return true;
                }
            } catch (Exception ex) {
                _logger.AppendLine("ApplicationWatcher MonitoredAppExists Exception: " + ex.StackTrace);
                return true;
            }
        }

        private (string username, string xuid) ExtractPlayerInfoFromString(string dataMsg) {
            int msgStartIndex = dataMsg.IndexOf(']') + 2;
            int usernameStart = dataMsg.IndexOf(':', msgStartIndex) + 2;
            int usernameEnd = dataMsg.IndexOf(',', usernameStart);
            int usernameLength = usernameEnd - usernameStart;
            int xuidStart = dataMsg.IndexOf(':', usernameEnd) + 2;
            return (dataMsg.Substring(usernameStart, usernameLength), dataMsg.Substring(xuidStart, dataMsg.Length - xuidStart));
        }

        private void RunStartupCommands() {
            foreach (StartCmdEntry cmd in _serverConfiguration.GetStartCommands()) {
                _stdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        public bool IsServerLLCapable() => _LiteLoaderEnabledServer;
    }
}
