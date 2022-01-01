using BedrockService.Service.Server.Management;
using BedrockService.Shared.Utilities;
using System.Text.RegularExpressions;

namespace BedrockService.Service.Server {
    public class BedrockServer : IBedrockServer {
        private Task? _serverTask;
        private Task? _watchdogTask;
        private CancellationTokenSource _serverCanceler = new CancellationTokenSource();
        private CancellationTokenSource _watchdogCanceler = new CancellationTokenSource();
        private StreamWriter _stdInStream;
        private Process _serverProcess;
        private HostControl _hostController;
        private ServerStatus _currentServerStatus;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IConfigurator _configurator;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private IBedrockLogger _serverLogger;
        private IPlayerManager _playerManager;
        private string _servicePath;
        private const string _startupMessage = "INFO] Server started.";
        public enum ServerStatus {
            Stopped,
            Starting,
            Stopping,
            Started
        }

        public BedrockServer(IServerConfiguration serverConfiguration, IConfigurator configurator, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo) {
            _serverConfiguration = serverConfiguration;
            _processInfo = processInfo;
            _serviceConfiguration = serviceConfiguration;
            _configurator = configurator;
            _logger = logger;
        }

        public void Initialize() {
            _servicePath = _processInfo.GetDirectory();
            _serverLogger = new BedrockLogger(_processInfo, _serviceConfiguration, _serverConfiguration);
            _serverLogger.Initialize();
            _playerManager = new PlayerManager(_serverConfiguration);
        }

        public void WriteToStandardIn(string command) {
            _stdInStream.WriteLine(command);
        }

        public void StartControl() {
            _serverCanceler = new CancellationTokenSource();
            _serverTask = RunServer();
            _serverTask.Start();
        }

        public void StopControl() {
            _serverCanceler.Cancel();
            if (_serverProcess != null) {
                _logger.AppendLine("Sending Stop to Bedrock. Process.HasExited = " + _serverProcess.HasExited.ToString());

                _serverProcess.CancelOutputRead();

                _stdInStream.WriteLine("stop");
                while (!_serverProcess.HasExited) { }
            }
            _serverProcess = null;
            _currentServerStatus = ServerStatus.Stopped;
        }

        public void StartWatchdog(HostControl hostControl) {
            _watchdogCanceler = new CancellationTokenSource();
            _hostController = hostControl;
            _watchdogTask = null;
            _watchdogTask = ApplicationWatchdogMonitor();
            _watchdogTask.Start();
        }

        public void InitializeBackup() {
            WriteToStandardIn("save hold");
            Task.Delay(1000).Wait();
            WriteToStandardIn("save query");
        }

        private bool PerformBackup(string queryString) {
            try {
                FileUtils fileUtils = new FileUtils(_servicePath);
                DirectoryInfo worldsDir = new DirectoryInfo($@"{_serverConfiguration.GetProp("ServerPath")}\worlds");
                DirectoryInfo backupDir = new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath").ToString()}\{_serverConfiguration.GetServerName()}");
                Dictionary<string, int> backupFileInfoPairs = new Dictionary<string, int>();
                string[] files = queryString.Split(", ");
                foreach (string file in files) {
                    string[] fileInfoSplit = file.Split(':');
                    string fileName = fileInfoSplit[0];
                    int fileSize = int.Parse(fileInfoSplit[1]);
                    backupFileInfoPairs.Add(fileName, fileSize);
                }
                PruneBackups(backupDir);
                DirectoryInfo targetDirectory = backupDir.CreateSubdirectory($"Backup_{DateTime.Now.Ticks}");
                _logger.AppendLine($"Backing up files for server {_serverConfiguration.GetServerName()}. Please wait!");
                string levelDir = @$"\{_serverConfiguration.GetProp("level-name")}";
                bool resuilt = fileUtils.BackupWorldFilesFromQuery(backupFileInfoPairs, worldsDir.FullName, $@"{targetDirectory.FullName}").Result;
                fileUtils.CopyFilesMatchingExtension(worldsDir.FullName + levelDir, targetDirectory.FullName + levelDir, ".json");
                WriteToStandardIn("save resume");
                return resuilt;

            } catch (Exception e) {
                _logger.AppendLine($"Error with Backup: {e.StackTrace}");
                return false;
            }
        }

        private void PruneBackups(DirectoryInfo backupDir) {
            if (!backupDir.Exists) {
                backupDir.Create();
            }
            int dirCount = backupDir.GetDirectories().Length;
            try {
                if (dirCount >= int.Parse(_serviceConfiguration.GetProp("MaxBackupCount").ToString())) {
                    List<long> dates = new List<long>();
                    foreach (DirectoryInfo dir in backupDir.GetDirectories()) {
                        string[] folderNameSplit = dir.Name.Split('_');
                        dates.Add(Convert.ToInt64(folderNameSplit[1]));
                    }
                    dates.Sort();
                    Directory.Delete($@"{backupDir}\Backup_{dates[dates.Count - 1]}", true);
                }
            } catch (Exception e) {
                if (e.GetType() == typeof(FormatException)) {
                    _logger.AppendLine("Error in Config! MaxBackupCount must be nothing but a number!");
                }
            }
        }

        public ServerStatus GetServerStatus() => _currentServerStatus;

        public void SetServerStatus(ServerStatus newStatus) => _currentServerStatus = newStatus;

        private Task ApplicationWatchdogMonitor() {
            return new Task(() => {
                while (true) {
                    if (_watchdogCanceler.IsCancellationRequested) {
                        _logger.AppendLine("WatchDog Task was canceled. Stopping server!");
                        StopControl();
                        return;
                    }
                    string exeName = _serverConfiguration.GetProp("ServerExeName").ToString();
                    string appName = exeName.Substring(0, exeName.Length - 4);
                    if (MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Starting) {
                        _logger.AppendLine($"BedrockService found {appName} already running! Killing to proceed...");
                        KillProcesses(Process.GetProcessesByName(appName));
                    }
                    if (MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopping) {
                        _logger.AppendLine($"BedrockService signaled stop to application {appName}.");
                        _logger.AppendLine("Stopping...");
                        StopControl();
                        while (_currentServerStatus == ServerStatus.Stopping) {
                            Task.Delay(500).Wait();
                        }
                    }
                    if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Starting) {
                        StartControl();
                        _logger.AppendLine($"Recieved start signal for server {_serverConfiguration.GetServerName()}.");
                        Task.Delay(25000).Wait();
                    }
                    if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Started) {
                        StopControl();
                        _logger.AppendLine($"Started application {appName} was not found in running processes... Resarting {appName}.");
                        StartControl();
                        Task.Delay(1500).Wait();
                    }
                    if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopped) {
                        _logger.AppendLine("Server stopped successfully.");
                    }
                    if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopping) {
                        _logger.AppendLine("Server stopped unexpectedly. Setting server status to stopped.");
                        _currentServerStatus = ServerStatus.Stopped;
                    }
                    if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopped && Program.IsExiting) {
                        return;
                    }
                    Task.Delay(3000).Wait();
                }
            }, _watchdogCanceler.Token);
        }

        public bool RestartServer(bool ShouldPerformBackup) {
            if (_currentServerStatus == ServerStatus.Started) {
                _currentServerStatus = ServerStatus.Stopping;
                while (_currentServerStatus == ServerStatus.Stopping) {
                    Thread.Sleep(100);
                }
                _currentServerStatus = ServerStatus.Starting;
            }
            return false;
        }

        public string GetServerName() => _serverConfiguration.GetServerName();

        private Task RunServer() {
            return new Task(() => {
                string exeName = _serverConfiguration.GetProp("ServerExeName").ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                _configurator.WriteJSONFiles(_serverConfiguration);
                _configurator.SaveServerProps(_serverConfiguration, false);

                try {
                    if (File.Exists($@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}")) {
                        if (MonitoredAppExists(appName)) {
                            Process[] processList = Process.GetProcessesByName(appName);
                            if (processList.Length != 0) {
                                _logger.AppendLine($@"Application {appName} was found running! Killing to proceed.");
                                KillProcesses(processList);
                            }
                        }
                        // Fires up a new process to run inside this one
                        CreateProcess();
                    }
                    else {
                        _logger.AppendLine($"The Bedrock Server is not accessible at {$@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}"}\r\nCheck if the file is at that location and that permissions are correct.");
                        _hostController.Stop();
                    }
                }
                catch (Exception e) {
                    _logger.AppendLine($"Error Running Bedrock Server: {e.Message}\n{e.StackTrace}");
                    _hostController.Stop();
                }
            }, _serverCanceler.Token);
        }

        private void CreateProcess() {
            string fileName = $@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}";
            _serverProcess = Process.Start(new ProcessStartInfo {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = fileName
            });
            _serverProcess.PriorityClass = ProcessPriorityClass.High;
            _serverProcess.OutputDataReceived += StdOutToLog;

            _serverProcess.BeginOutputReadLine();
            _stdInStream = _serverProcess.StandardInput;
        }

        private void KillProcesses(Process[] processList) {
            foreach (Process process in processList) {
                try {
                    process.Kill();
                    Thread.Sleep(1000);
                    _logger.AppendLine($@"App {_serverConfiguration.GetProp("ServerExeName")} killed!");
                }
                catch (Exception e) {
                    _logger.AppendLine($"Killing proccess resulted in error: {e.StackTrace}");
                }
            }
        }

        private bool MonitoredAppExists(string monitoredAppName) {
            try {
                Process[] processList = Process.GetProcessesByName(monitoredAppName);
                if (processList.Length == 0) {
                    return false;
                }
                else {
                    return true;
                }
            }
            catch (Exception ex) {
                _logger.AppendLine("ApplicationWatcher MonitoredAppExists Exception: " + ex.StackTrace);
                return true;
            }
        }

        private void StdOutToLog(object sender, DataReceivedEventArgs e) {
            if (e.Data != null && !e.Data.Contains("INFO] Running AutoCompaction...")) {
                string dataMsg = e.Data;
                string logFileText = "NO LOG FILE! - ";
                if (dataMsg.StartsWith(logFileText))
                    dataMsg = dataMsg.Substring(logFileText.Length, dataMsg.Length - logFileText.Length);
                _serverLogger.AppendLine(dataMsg);
                if (e.Data != null) {

                    if (dataMsg.Contains(_startupMessage)) {
                        _currentServerStatus = ServerStatus.Started;
                        Task.Delay(3000).Wait();

                        if (_serverConfiguration.GetStartCommands().Count > 0) {
                            RunStartupCommands();
                        }
                    }
                    if (dataMsg.StartsWith("[INFO] Player connected")) {
                        int usernameStart = dataMsg.IndexOf(':') + 2;
                        int usernameEnd = dataMsg.IndexOf(',');
                        int usernameLength = usernameEnd - usernameStart;
                        int xuidStart = dataMsg.IndexOf(':', usernameEnd) + 2;
                        string username = dataMsg.Substring(usernameStart, usernameLength);
                        string xuid = dataMsg.Substring(xuidStart, dataMsg.Length - xuidStart);
                        Console.WriteLine($"Player {username} connected with XUID: {xuid}");
                        _playerManager.PlayerConnected(username, xuid);
                        _configurator.SaveKnownPlayerDatabase(_serverConfiguration);
                    }
                    if (dataMsg.StartsWith("[INFO] Player disconnected")) {
                        int usernameStart = dataMsg.IndexOf(':') + 2;
                        int usernameEnd = dataMsg.IndexOf(',');
                        int usernameLength = usernameEnd - usernameStart;
                        int xuidStart = dataMsg.IndexOf(':', usernameEnd) + 2;
                        string username = dataMsg.Substring(usernameStart, usernameLength);
                        string xuid = dataMsg.Substring(xuidStart, dataMsg.Length - xuidStart);
                        Console.WriteLine($"Player {username} disconnected with XUID: {xuid}");
                        _playerManager.PlayerDisconnected(xuid);
                        _configurator.SaveKnownPlayerDatabase(_serverConfiguration);
                    }
                    if (dataMsg.Contains("Failed to load Vanilla")) {
                        _currentServerStatus = ServerStatus.Stopping;
                        while (_currentServerStatus != ServerStatus.Stopped)
                            Thread.Sleep(200);
                        if (_configurator.ReplaceServerBuild(_serverConfiguration).Wait(30000))
                            _currentServerStatus = ServerStatus.Starting;
                    }
                    if (dataMsg.Contains("Version ")) {
                        int msgStartIndex = dataMsg.IndexOf(']') + 2;
                        string focusedMsg = dataMsg.Substring(msgStartIndex, dataMsg.Length - msgStartIndex);
                        int versionIndex = focusedMsg.IndexOf(' ') + 1;
                        string versionString = focusedMsg.Substring(versionIndex, focusedMsg.Length - versionIndex);
                        string currentVersion = _serviceConfiguration.GetServerVersion();
                        if (currentVersion != versionString) {
                            _logger.AppendLine($"Server {GetServerName()} version found out-of-date! Now updating!");
                            StopServer(false).Wait();
                            _configurator.ReplaceServerBuild(_serverConfiguration).Wait();
                            StartControl();
                        }
                    }
                    if (dataMsg.Contains("A previous save has not been completed.")) {
                        Task.Delay(1000).Wait();
                        WriteToStandardIn("save query");
                    }
                    if (dataMsg.Contains($@"{_serverConfiguration.GetProp("level-name")}/db/")) {
                        _logger.AppendLine("Save data string detected! Performing backup now!");
                        if (PerformBackup(dataMsg)) {
                            _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Completed.");
                            return;
                        }
                        _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Failed. Check logs!");
                    }
                }
            }
        }

        private void RunStartupCommands() {
            foreach (StartCmdEntry cmd in _serverConfiguration.GetStartCommands()) {
                _stdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        public bool RollbackToBackup(byte serverIndex, string folderName) {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            FileUtils fileUtils = new FileUtils(_servicePath);
            DirectoryInfo worldsDir = new DirectoryInfo($@"{server.GetProp("ServerPath")}\worlds\{server.GetProp("level-name")}");
            DirectoryInfo backupDir = new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{server.GetServerName()}\{folderName}\{server.GetProp("level-name")}");
            StopServer(false).Wait();
            try {
                fileUtils.DeleteFilesRecursively(worldsDir, true);
                _logger.AppendLine($"Deleted world folder \"{worldsDir.Name}\"");
                fileUtils.CopyFilesRecursively(backupDir, worldsDir);
                _logger.AppendLine($"Copied files from backup \"{backupDir.Name}\" to server worlds directory.");
                _currentServerStatus = ServerStatus.Starting;
                return true;
            }
            catch (IOException e) {
                _logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
        }

        public IBedrockLogger GetLogger() => _serverLogger;

        public IPlayerManager GetPlayerManager() => _playerManager;

        public Task StopServer(bool stopWatchdog) {
            return Task.Run(() => {
                _currentServerStatus = ServerStatus.Stopping;
                while (_currentServerStatus != ServerStatus.Stopped) {
                    Thread.Sleep(100);
                }
                if (stopWatchdog) {
                    _watchdogCanceler.Cancel();
                    while (!_watchdogTask.IsCompleted) {
                        Task.Delay(200);
                    }
                }
            });
        }
    }
}
