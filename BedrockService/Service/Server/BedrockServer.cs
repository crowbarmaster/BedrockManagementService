using BedrockService.Service.Server.HostInfoClasses;
using BedrockService.Service.Server.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Topshelf;

namespace BedrockService.Service.Server
{
    public class BedrockServer
    {
        public Thread ServerThread { get; set; }
        public StreamWriter StdInStream;
        public ServerInfo serverInfo;
        public enum ServerStatus
        {
            Stopped,
            Starting,
            Stopping,
            Started
        }
        public ServerStatus CurrentServerStatus;

        Thread WatchdogThread;
        Process process;
        const string startupMessage = "[INFO] Server started.";
        HostControl hostController;

        public BedrockServer(ServerInfo serverToSet)
        {
            serverInfo = serverToSet;
        }

        public void StartControl(HostControl hostControl)
        {
            ServerThread = new Thread(new ParameterizedThreadStart(RunServer));
            ServerThread.Name = "ServerThread";
            ServerThread.IsBackground = true;
            ServerThread.Start(hostControl);
            CurrentServerStatus = ServerStatus.Started;
        }

        public bool StopControl()
        {
            if (process != null)
            {
                InstanceProvider.ServiceLogger.AppendLine("Sending Stop to Bedrock . Process.HasExited = " + process.HasExited.ToString());

                process.CancelOutputRead();

                StdInStream.WriteLine("stop");
                while (!process.HasExited) { }
            }
            if (ServerThread != null && ServerThread.IsAlive)
                ServerThread.Abort();
            ServerThread = null;
            process = null;
            GC.Collect();
            CurrentServerStatus = ServerStatus.Stopped;
            return true;
        }

        public void RunServer(object hostControl)
        {
            hostController = (HostControl)hostControl;
            string appName = serverInfo.ServerExeName.Value.Substring(0, serverInfo.ServerExeName.Value.Length - 4);
            InstanceProvider.ConfigManager.WriteJSONFiles(serverInfo);
            InstanceProvider.ConfigManager.SaveServerProps(serverInfo, false);

            try
            {
                if (File.Exists(serverInfo.ServerPath.Value + "\\" + serverInfo.ServerExeName.Value))
                {
                    if (MonitoredAppExists(appName))
                    {
                        Process[] processList = Process.GetProcessesByName(appName);
                        if (processList.Length != 0)
                        {
                            InstanceProvider.ServiceLogger.AppendLine($@"Application {appName} was found running! Killing to proceed.");
                            KillProcess(processList);
                        }
                    }
                    // Fires up a new process to run inside this one
                    CreateProcess();
                }
                else
                {
                    InstanceProvider.ServiceLogger.AppendLine("The Bedrock Server is not accessible at " + serverInfo.ServerPath.Value + "\\" + serverInfo.ServerExeName.Value + "\r\nCheck if the file is at that location and that permissions are correct.");
                    hostController.Stop();
                }
            }
            catch (Exception e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error Running Bedrock Server: {e.StackTrace}");
                hostController.Stop();

            }

        }

        public void StartWatchdog(HostControl hostControl)
        {
            hostController = hostControl;
            if (WatchdogThread == null)
            {
                WatchdogThread = new Thread(new ThreadStart(ApplicationWatchdogMonitor));
                WatchdogThread.IsBackground = true;
                WatchdogThread.Name = "WatchdogMonitor";
                WatchdogThread.Start();
            }
        }

        public void ApplicationWatchdogMonitor()
        {
            while (WatchdogThread.IsAlive)
            {
                string appName = serverInfo.ServerExeName.Value.Substring(0, serverInfo.ServerExeName.Value.Length - 4);
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Starting)
                {
                    StartControl(hostController);
                    InstanceProvider.ServiceLogger.AppendLine($"Recieved start signal for server {serverInfo.ServerName}.");
                    Thread.Sleep(5000);
                }
                while (MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Started)
                {
                    Thread.Sleep(5000);
                }
                if (MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopping)
                {
                    InstanceProvider.ServiceLogger.AppendLine($"BedrockService signaled stop to application {appName}.");
                    InstanceProvider.ServiceLogger.AppendLine("Stopping...");
                    StopControl();
                    while (CurrentServerStatus == ServerStatus.Stopping)
                    {
                        Thread.Sleep(250);
                    }
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Started)
                {
                    StopControl();
                    InstanceProvider.ServiceLogger.AppendLine($"Started application {appName} was not found in running processes... Resarting {appName}.");
                    StartControl(hostController);
                    Thread.Sleep(1500);
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopped)
                {
                    InstanceProvider.ServiceLogger.AppendLine("Server stopped successfully.");
                }
                if (!MonitoredAppExists(appName) && CurrentServerStatus == ServerStatus.Stopping)
                {
                    InstanceProvider.ServiceLogger.AppendLine("Server stopped unexpectedly. Setting server status to stopped.");
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

        public bool Backup()
        {
            try
            {
                FileInfo exe = new FileInfo(serverInfo.ServerPath.Value + serverInfo.ServerExeName.Value);
                string configBackupPath = InstanceProvider.HostInfo.GetGlobalValue("BackupPath");
                DirectoryInfo backupDir = new DirectoryInfo($@"{configBackupPath}\{serverInfo.ServerName}");
                DirectoryInfo serverDir = new DirectoryInfo(serverInfo.ServerPath.Value);
                DirectoryInfo worldsDir = new DirectoryInfo($@"{serverInfo.ServerPath.Value}\worlds");
                if (!Directory.Exists(backupDir.FullName))
                {
                    Directory.CreateDirectory($@"{InstanceProvider.HostInfo.GetGlobalValue("BackupPath")}\{serverInfo.ServerName}");
                }
                int dirCount = backupDir.GetDirectories().Length; // this line creates a new int with a value derived from the number of directories found in the backups folder.
                try // use a try catch any time you know an error could occur.
                {
                    if (dirCount >= int.Parse(InstanceProvider.HostInfo.GetGlobalValue("MaxBackupCount"))) // Compare the directory count with the value set in the config. Values from config are stored as strings, and therfore must be converted to integer first for compare.
                    {
                        Regex reg = new Regex(@"Backup_(.*)$"); // Creates a new Regex class with our pattern loaded.

                        List<long> Dates = new List<long>(); // creates a new list long integer array named Dates, and initializes it.
                        foreach (DirectoryInfo dir in backupDir.GetDirectories()) // Loop through the array of directories in backup folder. In this "foreach" loop, we name each entry in the array "dir" and then do something to it.
                        {
                            if (reg.IsMatch(dir.Name)) // Using regex.IsMatch will return true if the pattern matches the name of the folder we are working with. 
                            {
                                Match match = reg.Match(dir.Name); // creates an instance of the match to work with.
                                Dates.Add(Convert.ToInt64(match.Groups[1].Value)); // if it was a match, we then pull the number we saved in the (.*) part of the pattern from the groups method in the match. Groups saves the entire match first, followed by anthing saved in parentheses. Because we need to compare dates, we must convert the string to an integer.
                            }
                        }
                        long OldestDate = 0; // Create a new int to store the oldest date in.
                        foreach (long date in Dates) // for each date in the Dates array....
                        {
                            if (OldestDate == 0) // if this is the first entry in Dates, OldestDate will still be 0. Set it to a date so compare can happen.
                            {
                                OldestDate = date; // OldestDate now equals date.
                            }
                            else if (date < OldestDate) // If now the next entry in Dates is a smaller number than the previously set OldestDate, reset OldestDate to date.
                            {
                                OldestDate = date; // OldestDate now equals date.
                            }
                        }
                        Directory.Delete($@"{backupDir}\Backup_{OldestDate}", true); // After running through all directories, this string $@"{backupDir}\Backup_{OldestDate}" should now represent the folder that has the lowest/oldest date. Delete it. Supply the "true" after the directory string to enable recusive mode, removing all files and folders.
                    }
                }
                catch (Exception e) // catch all exceptions here.
                {
                    if (e.GetType() == typeof(FormatException)) // if the exception is equal a type of FormatException, Do the following... if this was a IOException, they would not match.
                    {
                        InstanceProvider.ServiceLogger.AppendLine("Error in Config! MaxBackupCount must be nothing but a number!"); // this exception will be thrown if the string could not become a number (i.e. of there was a letter in the mix).
                    }
                }

                var targetDirectory = backupDir.CreateSubdirectory($"Backup_{DateTime.Now.Ticks}");
                InstanceProvider.ServiceLogger.AppendLine($"Backing up files for server {serverInfo.ServerName}. Please wait!");
                if (InstanceProvider.HostInfo.GetGlobalValue("EntireBackups") == "false")
                {
                    CopyFilesRecursively(worldsDir, targetDirectory);
                }
                else if (InstanceProvider.HostInfo.GetGlobalValue("EntireBackups") == "true")
                {
                    CopyFilesRecursively(serverDir, targetDirectory);
                }
                return true;

            }
            catch (Exception e)
            {
                InstanceProvider.ServiceLogger.AppendLine($"Error with Backup: {e.StackTrace}");
                return false;
            }
        }

        private void CreateProcess()
        {
            process = Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = serverInfo.ServerPath.Value + "\\" + serverInfo.ServerExeName.Value
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
                    InstanceProvider.ServiceLogger.AppendLine($@"App {serverInfo.ServerExeName.Value.Substring(0, serverInfo.ServerExeName.Value.Length - 4)} killed!");
                }
                catch (Exception e)
                {
                    InstanceProvider.ServiceLogger.AppendLine($"Killing proccess resulted in error: {e.StackTrace}");
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
                InstanceProvider.ServiceLogger.AppendLine("ApplicationWatcher MonitoredAppExists Exception: " + ex.StackTrace);
                return true;
            }
        }

        private void StdOutToLog(object sender, DataReceivedEventArgs e)
        {
            serverInfo.ConsoleBuffer = serverInfo.ConsoleBuffer ?? new ServerLogger(InstanceProvider.HostInfo.GetGlobalValue("LogServersToFile") == "true", serverInfo.ServerName);
            if (e.Data != null && !e.Data.Contains("[INFO] Running AutoCompaction..."))
            {
                string dataMsg = e.Data;
                string logFileText = "NO LOG FILE! - ";
                if (dataMsg.StartsWith(logFileText))
                    dataMsg = dataMsg.Substring(logFileText.Length, dataMsg.Length - logFileText.Length);
                serverInfo.ConsoleBuffer.Append($"{serverInfo.ServerName}: {dataMsg}\r\n");
                if (e.Data != null)
                {

                    if (dataMsg.Contains(startupMessage))
                    {
                        CurrentServerStatus = ServerStatus.Started;
                        Thread.Sleep(1000);

                        if (serverInfo.StartCmds.Count > 0)
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
                        InstanceProvider.PlayerManager.PlayerConnected(username, xuid, serverInfo);
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
                        InstanceProvider.PlayerManager.PlayerDisconnected(xuid, serverInfo);
                    }
                    if (dataMsg.Contains("Failed to load Vanilla"))
                    {
                        CurrentServerStatus = ServerStatus.Stopping;
                        while (CurrentServerStatus != ServerStatus.Stopped)
                            Thread.Sleep(200);
                        if (InstanceProvider.BedrockService.ReplaceBuild(serverInfo).Wait(30000))
                            CurrentServerStatus = ServerStatus.Starting;
                    }
                }
            }
        }

        private void RunStartupCommands()
        {
            foreach (StartCmdEntry cmd in serverInfo.StartCmds)
            {
                StdInStream.WriteLine(cmd.Command.Trim());
                Thread.Sleep(1000);
            }
        }

        private void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        private void DeleteFilesRecursively(DirectoryInfo source)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                DeleteFilesRecursively(dir);
            }

            foreach (FileInfo file in source.GetFiles())
            {
                file.Delete();
                Directory.Delete(source.FullName);
            }
        }
    }
}
