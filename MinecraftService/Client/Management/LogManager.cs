using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Client.Management {
    class LogManager {
        public Task LogTask;
        public bool Working = false;
        public bool DisplayTimestamps = false;
        public List<string> ServiceLogs = new();
        private CancellationTokenSource _logTaskCancelSource;
        private IServiceConfiguration _connectedHost;
        private readonly IServerLogger _logger;
        private int currentServerLogLength;
        private int currentServiceLogLength;
        private int currentClientLogLength;

        public LogManager(IServerLogger logger) {
            _logger = logger;
        }

        private void LogManagerTask() {
            while (!_logTaskCancelSource.Token.IsCancellationRequested) {
                try {
                    Working = true;
                    int clientCount = FormManager.ClientLogContainer.GetLog() == null ? 0 : FormManager.ClientLogContainer.GetLog().Count;
                    if (FormManager.ClientLogContainer.GetLog() != null && FormManager.ClientLogContainer.GetLog().Count != currentClientLogLength) {
                        UpdateLogBoxInvoked(FormManager.MainWindow.clientLogBox, ProcessText(FormManager.ClientLogContainer.GetLog()));
                    }
                    if (_connectedHost != null) {
                        StringBuilder sendString = new();
                        foreach (IServerConfiguration server in _connectedHost.GetServerList()) {
                            server.SetLog(server.GetLog() ?? new List<LogEntry>());
                            sendString.Append($"{server.GetSettingsProp(ServerPropertyKeys.ServerName)}|;|{server.GetLog().Count}|?|");
                        }
                        sendString.Append($"Service|;|{_connectedHost.GetLog().Count}");
                        int serverCount = FormManager.MainWindow.selectedServer == null || FormManager.MainWindow.selectedServer.GetLog() == null ? 0 : FormManager.MainWindow.selectedServer.GetLog().Count;
                        int serviceCount = _connectedHost.GetLog() == null ? 0 : _connectedHost.GetLog().Count;
                        byte[] stringsToBytes = Encoding.UTF8.GetBytes(sendString.ToString());
                        FormManager.TCPClient.SendData(stringsToBytes, NetworkMessageTypes.ConsoleLogUpdate);
                        FormManager.TCPClient.SendData(FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.ServerStatusRequest);
                        Task.Delay(300).Wait();

                        if (FormManager.MainWindow.selectedServer == null) {
                            UpdateLogBoxInvoked(FormManager.MainWindow.LogBox, "");
                        } else {
                            if(FormManager.MainWindow.selectedServer.GetStatus() != null) {
                                Task.Run(() => {
                                    IServerConfiguration serverConfiguration = FormManager.MainWindow.selectedServer;
                                    bool autoStartEnabled = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerAutostartEnabled).GetBoolValue();
                                    ServerStatusModel status = serverConfiguration.GetStatus();
                                    ServiceStatusModel serviceStatus = FormManager.MainWindow.ServiceStatus;
                                    string statusMsg = $"{serverConfiguration.GetServerName()} is {status.ServerStatus}";

                                    if (FormManager.MainWindow.ServiceStatus != null) {
                                        statusMsg += $"\r\nStarted at: {status.ServerUptime:g}";
                                        statusMsg += $"\r\n{status.ActivePlayerList.Count} players online";

                                        if (!autoStartEnabled) {
                                            statusMsg += $"\r\nAutoStart disabled for this server!";
                                        }
                                        if (!serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() && status.DeployedVersion != null && serviceStatus.LatestVersion != null && status.DeployedVersion != "None") {
                                            Version serverVersion = Version.Parse(status.DeployedVersion);
                                            Version latestVersion = Version.Parse(serviceStatus.LatestVersion);
                                            if (serverVersion < latestVersion) {
                                                statusMsg += $"\r\nUpdate {latestVersion} available";
                                            }
                                        }
                                        statusMsg += "\r\n  --------------------------------";
                                        statusMsg += $"\r\nTotal service-wide backups: {FormManager.MainWindow.ServiceStatus.TotalBackups}\r\nTotal backup size: {FormManager.MainWindow.ServiceStatus.TotalBackupSize / 1000} MB\r\nService started: {FormManager.MainWindow.ServiceStatus.ServiceUptime:g}";
                                    }
                                    UpdateLogBoxInvoked(FormManager.MainWindow.ServerInfoBox, statusMsg);
                                });
                            }
                        }
                        if (_connectedHost.GetLog().Count != currentServiceLogLength) {
                            UpdateLogBoxInvoked(FormManager.MainWindow.serviceTextbox, ProcessText(_connectedHost.GetLog()));
                        }
                        if (FormManager.MainWindow.selectedServer != null && FormManager.MainWindow.selectedServer.GetLog() != null && FormManager.MainWindow.selectedServer.GetLog().Count != currentServerLogLength) {
                            UpdateLogBoxInvoked(FormManager.MainWindow.LogBox, ProcessText(FormManager.MainWindow.selectedServer.GetLog()));
                        }
                        currentServerLogLength = serverCount;
                        currentServiceLogLength = serviceCount;
                        currentClientLogLength = clientCount;
                    }
                } catch (Exception e) {
                    _logger.AppendLine($"LogManager Error! Stacetrace: {e.StackTrace}");
                }
            }
        }

        private string ProcessText(List<LogEntry> targetLog) {
            if (FormManager.MainWindow.ConfigManager.DisplayTimestamps) {
                return string.Join("\r\n", targetLog.Select(x => $"[{x.TimeStamp:G}] {x.Text}").ToList());
            }
            return string.Join("\r\n", targetLog.Select(x => x.Text).ToList());
        }

        public bool InitLogThread() {
            DisplayTimestamps = FormManager.MainWindow.ConfigManager.DisplayTimestamps;
            _logTaskCancelSource = new CancellationTokenSource();
            return StartLogThread();
        }

        public void SetConnectedHost(IServiceConfiguration host) => _connectedHost = host;

        public bool StartLogThread() {
            try {
                if (LogTask != null && !_logTaskCancelSource.IsCancellationRequested)
                    _logTaskCancelSource.Cancel();
                Thread.Sleep(500);
                _logTaskCancelSource = new CancellationTokenSource();
                LogTask = Task.Factory.StartNew(new Action(LogManagerTask), _logTaskCancelSource.Token);
                _logger.AppendLine("LogThread started");
                return true;
            } catch (Exception e) {
                _logger.AppendLine($"Error starting LogThread: {e.StackTrace}");
            }
            return false;
        }

        public bool StopLogThread() {
            if (LogTask == null) {
                return true;
            }
            try {
                _logTaskCancelSource.Cancel();
            } catch (ThreadAbortException e) {
                _logger.AppendLine(e.StackTrace);
            }
            _logger.AppendLine("LogThread stopped");
            LogTask = null;
            return true;
        }

        private static void UpdateLogBoxInvoked(TextBox targetBox, string contents) {
            FormManager.MainWindow.Invoke((MethodInvoker)delegate {
                FormManager.MainWindow.UpdateServerLogBox(targetBox, contents);
            });
        }
    }
}
