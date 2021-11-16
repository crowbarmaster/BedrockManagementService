using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Core.Threads;
using BedrockService.Service.Management;
using BedrockService.Service.Server.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace BedrockService.Service.Server
{
    public class BedrockServer : IBedrockServer
    {
        private IServiceThread _serverThread;
        private IServiceThread _watchdogThread;
        private StreamWriter _stdInStream;
        private Process _serverProcess;
        private HostControl _hostController;
        private ServerStatus _currentServerStatus;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IPlayerManager _playerManager;
        private readonly IConfigurator _configurator;
        private readonly IBedrockLogger _logger;
        private readonly IBedrockLogger _serverLogger;
        private readonly string _servicePath;
        private const string _startupMessage = "[INFO] Server started.";
        public enum ServerStatus
        {
            Stopped,
            Starting,
            Stopping,
            Started
        }

        public BedrockServer(IServerConfiguration serverConfiguration, IConfigurator configurator, IBedrockLogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo)
        {
            _serverConfiguration = serverConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _configurator = configurator;
            _logger = logger;
            _servicePath = processInfo.GetDirectory();
            _serverLogger = new ServerLogger(processInfo, serviceConfiguration, serverConfiguration, serverConfiguration.GetServerName());
            _playerManager = new PlayerManager(serverConfiguration, logger);
        }

        public void WriteToStandardIn(string command)
        {
            _stdInStream.WriteLine(command);
        }

        public void StartControl()
        {
            _serverThread = new ServerProcessThread(new ThreadStart(RunServer));
        }

        public void StopControl()
        {
            if (_serverProcess != null)
            {
                _logger.AppendLine("Sending Stop to Bedrock. Process.HasExited = " + _serverProcess.HasExited.ToString());

                _serverProcess.CancelOutputRead();

                _stdInStream.WriteLine("stop");
                while (!_serverProcess.HasExited) { }
            }
            _serverThread.CloseThread();
            _serverProcess = null;
            _currentServerStatus = ServerStatus.Stopped;
        }

        public void StartWatchdog(HostControl hostControl)
        {
            _hostController = hostControl;
            if (_watchdogThread == null)
            {
                _watchdogThread = new WatchdogThread(new ThreadStart(ApplicationWatchdogMonitor));
            }
        }

        private bool Backup()
        {
            try
            {
                FileInfo exe = new FileInfo($@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}");
                string configBackupPath = _serviceConfiguration.GetProp("BackupPath").ToString();
                DirectoryInfo backupDir = new DirectoryInfo($@"{configBackupPath}\{_serverConfiguration.GetServerName()}");
                DirectoryInfo serverDir = new DirectoryInfo(_serverConfiguration.GetProp("ServerPath").ToString());
                DirectoryInfo worldsDir = new DirectoryInfo($@"{_serverConfiguration.GetProp("ServerPath")}\worlds");
                if (!backupDir.Exists)
                {
                    backupDir.Create();
                }
                int dirCount = backupDir.GetDirectories().Length;
                try
                {
                    if (dirCount >= int.Parse(_serviceConfiguration.GetProp("MaxBackupCount").ToString()))
                    {
                        Regex reg = new Regex(@"Backup_(.*)$");

                        List<long> Dates = new List<long>();
                        foreach (DirectoryInfo dir in backupDir.GetDirectories())
                        {
                            if (reg.IsMatch(dir.Name))
                            {
                                Match match = reg.Match(dir.Name);
                                Dates.Add(Convert.ToInt64(match.Groups[1].Value));
                            }
                        }
                        long OldestDate = 0;
                        foreach (long date in Dates)
                        {
                            if (OldestDate == 0)
                            {
                                OldestDate = date;
                            }
                            else if (date < OldestDate)
                            {
                                OldestDate = date;
                            }
                        }
                        Directory.Delete($@"{backupDir}\Backup_{OldestDate}", true);
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(FormatException))
                    {
                        _logger.AppendLine("Error in Config! MaxBackupCount must be nothing but a number!");
                    }
                }

                DirectoryInfo targetDirectory = backupDir.CreateSubdirectory($"Backup_{DateTime.Now.Ticks}");
                _logger.AppendLine($"Backing up files for server {_serverConfiguration.GetServerName()}. Please wait!");
                if (_serviceConfiguration.GetProp("EntireBackups").ToString() == "false")
                {
                    new FileUtils(_servicePath).CopyFilesRecursively(worldsDir, targetDirectory);
                    return true;
                }
                new FileUtils(_servicePath).CopyFilesRecursively(serverDir, targetDirectory);
                return true;
            }
            catch (Exception e)
            {
                _logger.AppendLine($"Error with Backup: {e.StackTrace}");
                return false;
            }
        }

        public void StopWatchdog()
        {
            if(_watchdogThread != null)
                _watchdogThread.CloseThread();
            _watchdogThread = null;
        }

        public ServerStatus GetServerStatus() => _currentServerStatus;

        public void SetServerStatus(ServerStatus newStatus) => _currentServerStatus = newStatus;

        private void ApplicationWatchdogMonitor()
        {
            while (_watchdogThread.IsAlive())
            {
                string exeName = _serverConfiguration.GetProp("ServerExeName").ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Starting)
                {
                    StartControl();
                    _logger.AppendLine($"Recieved start signal for server {_serverConfiguration.GetServerName()}.");
                    Thread.Sleep(15000);
                }
                while (MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Started)
                {
                    Thread.Sleep(5000);
                }
                if (MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopping)
                {
                    _logger.AppendLine($"BedrockService signaled stop to application {appName}.");
                    _logger.AppendLine("Stopping...");
                    StopControl();
                    while (_currentServerStatus == ServerStatus.Stopping)
                    {
                        Thread.Sleep(250);
                    }
                }
                if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Started)
                {
                    StopControl();
                    _logger.AppendLine($"Started application {appName} was not found in running processes... Resarting {appName}.");
                    StartControl();
                    Thread.Sleep(1500);
                }
                if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopped)
                {
                    _logger.AppendLine("Server stopped successfully.");
                }
                if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopping)
                {
                    _logger.AppendLine("Server stopped unexpectedly. Setting server status to stopped.");
                    _currentServerStatus = ServerStatus.Stopped;
                }
                while (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopped && !Program.IsExiting)
                {
                    Thread.Sleep(1000);
                }
                if (!MonitoredAppExists(appName) && _currentServerStatus == ServerStatus.Stopped && Program.IsExiting)
                {
                    return;
                }

            }
        }

        public bool RestartServer(bool ShouldPerformBackup)
        {
            if (_currentServerStatus == ServerStatus.Started)
            {
                _currentServerStatus = ServerStatus.Stopping;
                while (_currentServerStatus == ServerStatus.Stopping)
                {
                    Thread.Sleep(100);
                }
                if (ShouldPerformBackup)
                {
                    if (!Backup())
                        return false;
                }
                _currentServerStatus = ServerStatus.Starting;
            }
            return false;
        }

        public string GetServerName() => _serverConfiguration.GetServerName();

        private void RunServer()
        {
            string exeName = _serverConfiguration.GetProp("ServerExeName").ToString();
            string appName = exeName.Substring(0, exeName.Length - 4);
            _configurator.WriteJSONFiles(_serverConfiguration);
            _configurator.SaveServerProps(_serverConfiguration, false);

            try
            {
                if (File.Exists($@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}"))
                {
                    if (MonitoredAppExists(appName))
                    {
                        Process[] processList = Process.GetProcessesByName(appName);
                        if (processList.Length != 0)
                        {
                            _logger.AppendLine($@"Application {appName} was found running! Killing to proceed.");
                            KillProcess(processList);
                        }
                    }
                    // Fires up a new process to run inside this one
                    CreateProcess();
                }
                else
                {
                    _logger.AppendLine($"The Bedrock Server is not accessible at {$@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}"}\r\nCheck if the file is at that location and that permissions are correct.");
                    _hostController.Stop();
                }
            }
            catch (Exception e)
            {
                _logger.AppendLine($"Error Running Bedrock Server: {e.Message}\n{e.StackTrace}");
                _hostController.Stop();

            }

        }

        private void CreateProcess()
        {
            string fileName = $@"{_serverConfiguration.GetProp("ServerPath")}\{_serverConfiguration.GetProp("ServerExeName")}";
            _serverProcess = Process.Start(new ProcessStartInfo
            {
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

        private void KillProcess(Process[] processList)
        {
            foreach (Process process in processList)
            {
                try
                {
                    process.Kill();
                    Thread.Sleep(1000);
                    _logger.AppendLine($@"App {_serverConfiguration.GetProp("ServerExeName")} killed!");
                }
                catch (Exception e)
                {
                    _logger.AppendLine($"Killing proccess resulted in error: {e.StackTrace}");
                }
            }
        }

        private bool MonitoredAppExists(string monitoredAppName)
        {
            try
            {
                Process[] processList = Process.GetProcessesByName(monitoredAppName);
                if (processList.Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.AppendLine("ApplicationWatcher MonitoredAppExists Exception: " + ex.StackTrace);
                return true;
            }
        }

        private void StdOutToLog(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && !e.Data.Contains("[INFO] Running AutoCompaction..."))
            {
                string dataMsg = e.Data;
                string logFileText = "NO LOG FILE! - ";
                if (dataMsg.StartsWith(logFileText))
                    dataMsg = dataMsg.Substring(logFileText.Length, dataMsg.Length - logFileText.Length);
                _serverLogger.AppendText($"{_serverConfiguration.GetServerName()}: {dataMsg}\r\n");
                if (e.Data != null)
                {

                    if (dataMsg.Contains(_startupMessage))
                    {
                        _currentServerStatus = ServerStatus.Started;
                        Thread.Sleep(1000);

                        if (_serverConfiguration.GetStartCommands().Count > 0)
                        {
                            RunStartupCommands();
                        }
                    }
                    if (dataMsg.StartsWith("[INFO] Player connected"))
                    {
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
                    if (dataMsg.StartsWith("[INFO] Player disconnected"))
                    {
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
                    if (dataMsg.Contains("Failed to load Vanilla"))
                    {
                        _currentServerStatus = ServerStatus.Stopping;
                        while (_currentServerStatus != ServerStatus.Stopped)
                            Thread.Sleep(200);
                        if (_configurator.ReplaceServerBuild(_serverConfiguration).Wait(30000))
                            _currentServerStatus = ServerStatus.Starting;
                    }
                    if(dataMsg.Contains("Version "))
                    {
                        int msgStartIndex = dataMsg.IndexOf(']') + 2;
                        string focusedMsg = dataMsg.Substring(msgStartIndex, dataMsg.Length - msgStartIndex);
                        int versionIndex = focusedMsg.IndexOf(' ') + 1;
                        string versionString = focusedMsg.Substring(versionIndex, focusedMsg.Length - versionIndex);
                        string currentVersion = _serviceConfiguration.GetServerVersion();
                        if (currentVersion != versionString)
                        {
                            _logger.AppendLine($"Server {GetServerName()} version found out-of-date! Now updating!");
                            StopServer().Wait();
                            _configurator.ReplaceServerBuild(_serverConfiguration).Wait();
                            StartControl();
                        }
                    }
                }
            }
        }

        private void RunStartupCommands()
        {
            foreach (StartCmdEntry cmd in _serverConfiguration.GetStartCommands())
            {
                _stdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        public bool RollbackToBackup(byte serverIndex, string folderName)
        {
            IServerConfiguration server = _serviceConfiguration.GetServerInfoByIndex(serverIndex);
            StopServer().Wait();
            try
            {
                foreach (DirectoryInfo dir in new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{server.GetServerName()}").GetDirectories())
                    if (dir.Name == folderName)
                    {
                        new FileUtils(_servicePath).DeleteFilesRecursively(new DirectoryInfo($@"{server.GetProp("ServerPath")}\worlds"), false);
                        _logger.AppendLine($"Deleted world folder contents.");
                        foreach (DirectoryInfo worldDir in new DirectoryInfo($@"{_serviceConfiguration.GetProp("BackupPath")}\{server.GetServerName()}\{folderName}").GetDirectories())
                        {
                            new FileUtils(_servicePath).CopyFilesRecursively(worldDir, new DirectoryInfo($@"{server.GetProp("ServerPath")}\worlds\{worldDir.Name}"));
                            _logger.AppendLine($@"Copied {worldDir.Name} to path {server.GetProp("ServerPath")}\worlds");
                        }
                        SetServerStatus(ServerStatus.Starting);
                        return true;
                    }
            }
            catch (IOException e)
            {
                _logger.AppendLine($"Error deleting selected backups! {e.Message}");
            }
            return false;
        }

        public IBedrockLogger GetLogger() => _serverLogger;

        public IPlayerManager GetPlayerManager() => _playerManager;

        public Task StopServer()
        {
           return Task.Run(() =>
            {
                _currentServerStatus = ServerStatus.Stopping;
                while (_currentServerStatus != ServerStatus.Stopped)
                {
                    Thread.Sleep(100);
                }

            });
        }
    }
}
