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
        private IServiceThread ServerThread;
        private IServiceThread WatchdogThread;
        private StreamWriter StdInStream;
        private readonly IServerConfiguration ServerConfiguration;
        private readonly IServiceConfiguration serviceConfiguration;
        private readonly IPlayerManager playerManager;
        private readonly IConfigurator Configurator;
        private readonly ILogger Logger;
        private ILogger ServerLogger;
        private readonly string ServicePath;
        public enum ServerStatus
        {
            Stopped,
            Starting,
            Stopping,
            Started
        }
        public ServerStatus CurrentServerStatus;
        Process process;
        const string startupMessage = "[INFO] Server started.";
        HostControl hostController;

        public BedrockServer(IServerConfiguration serverConfiguration, IConfigurator configurator, ILogger logger, IServiceConfiguration serviceConfiguration, IProcessInfo processInfo)
        {
            ServerConfiguration = serverConfiguration;
            this.serviceConfiguration = serviceConfiguration;
            Configurator = configurator;
            Logger = logger;
            ServicePath = processInfo.GetDirectory();
            ServerLogger = new ServerLogger(processInfo, serviceConfiguration, serverConfiguration, serverConfiguration.GetServerName());
            playerManager = new PlayerManager(ServerConfiguration, Logger);
        }

        public void WriteToStandardIn(string command)
        {
            StdInStream.WriteLine(command);
        }

        public void StartControl()
        {
            ServerThread = new ServerProcessThread(new ThreadStart(RunServer));
        }

        public async Task AwaitServerStop()
        {
            await Task.Run(() =>
            {
                CurrentServerStatus = ServerStatus.Stopping;
                while(CurrentServerStatus != ServerStatus.Stopped)
                {
                    Thread.Sleep(250);
                }
                return;
            });
        }

        public void StopControl()
        {
            if (process != null)
            {
                Logger.AppendLine("Sending Stop to Bedrock. Process.HasExited = " + process.HasExited.ToString());

                process.CancelOutputRead();

                StdInStream.WriteLine("stop");
                while (!process.HasExited) { }
            }
            ServerThread.CloseThread();
            process = null;
            CurrentServerStatus = ServerStatus.Stopped;
        }

        public void StartWatchdog(HostControl hostControl)
        {
            hostController = hostControl;
            if (WatchdogThread == null)
            {
                WatchdogThread = new WatchdogThread(new ThreadStart(ApplicationWatchdogMonitor));
            }
        }

        private bool Backup()
        {
            try
            {
                FileInfo exe = new FileInfo($@"{ServerConfiguration.GetProp("ServerPath")}\{ServerConfiguration.GetProp("ServerExeName")}");
                string configBackupPath = serviceConfiguration.GetProp("BackupPath").ToString();
                DirectoryInfo backupDir = new DirectoryInfo($@"{configBackupPath}\{ServerConfiguration.GetServerName()}");
                DirectoryInfo serverDir = new DirectoryInfo(ServerConfiguration.GetProp("ServerPath").ToString());
                DirectoryInfo worldsDir = new DirectoryInfo($@"{ServerConfiguration.GetProp("ServerPath")}\worlds");
                if (!backupDir.Exists)
                {
                    backupDir.Create();
                }
                int dirCount = backupDir.GetDirectories().Length;
                try
                {
                    if (dirCount >= int.Parse(serviceConfiguration.GetProp("MaxBackupCount").ToString()))
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
                        Logger.AppendLine("Error in Config! MaxBackupCount must be nothing but a number!");
                    }
                }

                var targetDirectory = backupDir.CreateSubdirectory($"Backup_{DateTime.Now.Ticks}");
                Logger.AppendLine($"Backing up files for server {ServerConfiguration.GetServerName()}. Please wait!");
                if (serviceConfiguration.GetProp("EntireBackups").ToString() == "false")
                {
                    new FileUtils(ServicePath).CopyFilesRecursively(worldsDir, targetDirectory);
                    return true;
                }
                new FileUtils(ServicePath).CopyFilesRecursively(serverDir, targetDirectory);
                return true;
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error with Backup: {e.StackTrace}");
                return false;
            }
        }

        public void StopWatchdog()
        {
            if(WatchdogThread != null)
                WatchdogThread.CloseThread();
            WatchdogThread = null;
        }

        public ServerStatus GetServerStatus() => CurrentServerStatus;

        public void SetServerStatus(ServerStatus newStatus) => CurrentServerStatus = newStatus;

        private void ApplicationWatchdogMonitor()
        {
            while (WatchdogThread.IsAlive())
            {
                string exeName = ServerConfiguration.GetProp("ServerExeName").ToString();
                string appName = exeName.Substring(0, exeName.Length - 4);
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Starting)
                {
                    StartControl();
                    Logger.AppendLine($"Recieved start signal for server {ServerConfiguration.GetServerName()}.");
                    Thread.Sleep(15000);
                }
                while (MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Started)
                {
                    Thread.Sleep(5000);
                }
                if (MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopping)
                {
                    Logger.AppendLine($"BedrockService signaled stop to application {appName}.");
                    Logger.AppendLine("Stopping...");
                    StopControl();
                    while (CurrentServerStatus == ServerStatus.Stopping)
                    {
                        Thread.Sleep(250);
                    }
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Started)
                {
                    StopControl();
                    Logger.AppendLine($"Started application {appName} was not found in running processes... Resarting {appName}.");
                    StartControl();
                    Thread.Sleep(1500);
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopped)
                {
                    Logger.AppendLine("Server stopped successfully.");
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopping)
                {
                    Logger.AppendLine("Server stopped unexpectedly. Setting server status to stopped.");
                    CurrentServerStatus = ServerStatus.Stopped;
                }
                while (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopped && !Program.IsExiting)
                {
                    Thread.Sleep(1000);
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopped && Program.IsExiting)
                {
                    return;
                }

            }
        }

        public bool RestartServer(bool ShouldPerformBackup)
        {
            if (CurrentServerStatus == ServerStatus.Started)
            {
                CurrentServerStatus = ServerStatus.Stopping;
                while (CurrentServerStatus == ServerStatus.Stopping)
                {
                    Thread.Sleep(100);
                }
                if (ShouldPerformBackup)
                {
                    if (!Backup())
                        return false;
                }
                CurrentServerStatus = ServerStatus.Starting;
            }
            return false;
        }

        public string GetServerName() => ServerConfiguration.GetServerName();

        private void RunServer()
        {
            string exeName = ServerConfiguration.GetProp("ServerExeName").ToString();
            string appName = exeName.Substring(0, exeName.Length - 4);
            Configurator.WriteJSONFiles(ServerConfiguration);
            Configurator.SaveServerProps(ServerConfiguration, false);

            try
            {
                if (File.Exists($@"{ServerConfiguration.GetProp("ServerPath")}\{ServerConfiguration.GetProp("ServerExeName")}"))
                {
                    if (MonitoredAppExists(appName))
                    {
                        Process[] processList = Process.GetProcessesByName(appName);
                        if (processList.Length != 0)
                        {
                            Logger.AppendLine($@"Application {appName} was found running! Killing to proceed.");
                            KillProcess(processList);
                        }
                    }
                    // Fires up a new process to run inside this one
                    CreateProcess();
                }
                else
                {
                    Logger.AppendLine($"The Bedrock Server is not accessible at {$@"{ServerConfiguration.GetProp("ServerPath")}\{ServerConfiguration.GetProp("ServerExeName")}"}\r\nCheck if the file is at that location and that permissions are correct.");
                    hostController.Stop();
                }
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error Running Bedrock Server: {e.Message}\n{e.StackTrace}");
                hostController.Stop();

            }

        }

        private void CreateProcess()
        {
            string fileName = $@"{ServerConfiguration.GetProp("ServerPath")}\{ServerConfiguration.GetProp("ServerExeName")}";
            process = Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = fileName
            });
            process.PriorityClass = ProcessPriorityClass.High;
            process.OutputDataReceived += StdOutToLog;

            process.BeginOutputReadLine();
            StdInStream = process.StandardInput;
        }

        private void KillProcess(Process[] processList)
        {
            foreach (Process process in processList)
            {
                try
                {
                    process.Kill();
                    Thread.Sleep(1000);
                    Logger.AppendLine($@"App {ServerConfiguration.GetProp("ServerExeName")} killed!");
                }
                catch (Exception e)
                {
                    Logger.AppendLine($"Killing proccess resulted in error: {e.StackTrace}");
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
                Logger.AppendLine("ApplicationWatcher MonitoredAppExists Exception: " + ex.StackTrace);
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
                ServerLogger.AppendText($"{ServerConfiguration.GetServerName()}: {dataMsg}\r\n");
                if (e.Data != null)
                {

                    if (dataMsg.Contains(startupMessage))
                    {
                        CurrentServerStatus = ServerStatus.Started;
                        Thread.Sleep(1000);

                        if (ServerConfiguration.GetStartCommands().Count > 0)
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
                        playerManager.PlayerConnected(username, xuid);
                        Configurator.SaveKnownPlayerDatabase(ServerConfiguration);
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
                        playerManager.PlayerDisconnected(xuid);
                        Configurator.SaveKnownPlayerDatabase(ServerConfiguration);
                    }
                    if (dataMsg.Contains("Failed to load Vanilla"))
                    {
                        CurrentServerStatus = ServerStatus.Stopping;
                        while (CurrentServerStatus != ServerStatus.Stopped)
                            Thread.Sleep(200);
                        if (Configurator.ReplaceServerBuild(ServerConfiguration).Wait(30000))
                            CurrentServerStatus = ServerStatus.Starting;
                    }
                    if(dataMsg.Contains("Version "))
                    {
                        int msgStartIndex = dataMsg.IndexOf(']') + 2;
                        string focusedMsg = dataMsg.Substring(msgStartIndex, dataMsg.Length - msgStartIndex);
                        int versionIndex = focusedMsg.IndexOf(' ') + 1;
                        string versionString = focusedMsg.Substring(versionIndex, focusedMsg.Length - versionIndex);
                        string currentVersion = serviceConfiguration.GetServerVersion();
                        if (currentVersion != versionString)
                        {
                            Logger.AppendLine($"Server {GetServerName()} version found out-of-date! Now updating!");
                            AwaitServerStop().Wait();
                            Configurator.ReplaceServerBuild(ServerConfiguration).Wait();
                            StartControl();
                        }
                    }
                }
            }
        }

        private void RunStartupCommands()
        {
            foreach (StartCmdEntry cmd in ServerConfiguration.GetStartCommands())
            {
                StdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        public ILogger GetLogger() => ServerLogger;

        public IPlayerManager GetPlayerManager() => playerManager;

        public async Task StopServer()
        {
            await Task.Run(() =>
            {
                if (CurrentServerStatus == ServerStatus.Started)
                {
                    CurrentServerStatus = ServerStatus.Stopping;
                    while (CurrentServerStatus != ServerStatus.Stopped)
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }
    }
}
