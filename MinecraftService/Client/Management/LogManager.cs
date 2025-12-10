// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Client.Management {
    public class LogManager {
        public System.Timers.Timer LogUpdateTimer = new(300);
        public System.Timers.Timer LogRequestTimer = new(1200);
        public bool Working = false;
        public bool DisplayTimestamps = false;
        public List<string> ServiceLogs = new();
        private CancellationTokenSource _logTaskCancelSource;
        private ServiceConfigurator _connectedHost;
        private readonly MmsLogger _logger;
        private int currentClientLogLength;

        public LogManager(MmsLogger logger) {
            _logger = logger;
            LogUpdateTimer.Elapsed += LogTimer_Elapsed;
            LogRequestTimer.Elapsed += LogRequestTimer_Elapsed; ;
        }

        private void LogRequestTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (_logTaskCancelSource.IsCancellationRequested) {
                LogRequestTimer.Stop();
                return;
            }
            if (_connectedHost != null) {
                RequestLogUpdates();
            } else if (FormManager.TCPClient.EstablishedLink) {
                FormManager.TCPClient.SendData(new() { Type = MessageTypes.Heartbeat });
            }
        }

        private void LogTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (_logTaskCancelSource.IsCancellationRequested) {
                LogUpdateTimer.Stop();
                return;
            }
                UpdateClientTextbox();
                UpdateStatusBox();
        }

        private void RequestLogUpdates() {
            List<ConsoleLogUpdateRequest> requests = []; 
            requests.Add(new(0xFF, _connectedHost.GetLog().Count));
            foreach (IServerConfiguration server in _connectedHost.GetServerList()) {
                if (server.GetLog() != null || server.GetLog().Any()) {
                    requests.Add(new(_connectedHost.GetServerIndex(server), server.GetLog().Count));
                }
            }
            byte[] stringsToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requests));
            FormManager.TCPClient.SendData(new() {
                Data = stringsToBytes,
                Type = MessageTypes.ConsoleLogUpdate
            });
            if(FormManager.MainWindow.SelectedServer != null) {
                FormManager.TCPClient.SendData(new() {
                    ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.SelectedServer),
                    Type = MessageTypes.ServerStatusRequest
                });
            }
        }

        private void UpdateClientTextbox() {
            int clientCount = FormManager.ClientLogContainer.GetLog() == null ? 0 : FormManager.ClientLogContainer.GetLog().Count;
            if (FormManager.ClientLogContainer.GetLog() != null && FormManager.ClientLogContainer.GetLog().Count != currentClientLogLength) {
                UpdateLogBoxInvoked(FormManager.MainWindow.clientLogBox, ProcessText(FormManager.ClientLogContainer.GetLog()));
                currentClientLogLength = FormManager.ClientLogContainer.GetLog().Count;
            }
            if (FormManager.MainWindow.connectedHost != null && FormManager.MainWindow.connectedHost.GetServerList().Count == 0) {
                UpdateLogBoxInvoked(FormManager.MainWindow.ServerInfoBox, ["There are no configured servers.\r\nPlease use \"Add new Server\" button."]);
            }
        }

        public void ProcessIncomingLogData(List<ConsoleLogUpdateCallback> callbacks) {
            if (_logTaskCancelSource.IsCancellationRequested) {
                return;
            }
            foreach (ConsoleLogUpdateCallback callback in callbacks) {
                if (callback.LogTarget == 0xFF) {
                    int logCount = _connectedHost.GetLog().Count;
                    if (logCount < callback.CurrentCount) { 
                        foreach (string entry in callback.LogEntries) {
                            _connectedHost.GetLog().Add(new(entry));
                        }
                    }
                    logCount = _connectedHost.GetLog().Count + 1;
                    int lines = FormManager.MainWindow.ServiceLogbox.Lines.Length;
                    if (lines < logCount) {
                        UpdateLogBoxInvoked(FormManager.MainWindow.ServiceLogbox, ProcessText(_connectedHost.GetLog()));
                    }
                } else {
                    IServerConfiguration server = _connectedHost.GetServerInfoByIndex(callback.LogTarget);
                    if (server != null) {
                        if(server.GetLog().Count < callback.CurrentCount) { 
                            foreach (string entry in callback.LogEntries) {
                                server.GetLog().Add(new(entry));
                            }
                        }
                        int serverLines = FormManager.MainWindow.ServerLogBox.Lines.Length;
                        if (server == FormManager.MainWindow.SelectedServer && serverLines < server.GetLog().Count) {
                            UpdateLogBoxInvoked(FormManager.MainWindow.ServerLogBox, ProcessText(server.GetLog()));
                        }
                    }
                }
            }
        }

        public void UpdateStatusBox() {
            if (_logTaskCancelSource.IsCancellationRequested) {
                return;
            }
            if (_connectedHost == null || FormManager.MainWindow.SelectedServer == null) {
                UpdateLogBoxInvoked(FormManager.MainWindow.ServerLogBox, []);
                return;
            }
            if (FormManager.MainWindow.SelectedServer.GetStatus() != null) {
                Task.Run(() => {
                    IServerConfiguration serverConfiguration = FormManager.MainWindow.SelectedServer;
                    bool autoStartEnabled = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerAutostartEnabled).GetBoolValue();
                    ServerStatusModel serverStatus = serverConfiguration.GetStatus();
                    ServiceStatusModel serviceStatus = FormManager.MainWindow.ServiceStatus;
                    List<string> statusMsg = [];
                    statusMsg.Add($"{serverConfiguration.GetServerName()} is {serverStatus.ServerStatus}");

                    if (FormManager.MainWindow.ServiceStatus != null) {
                        statusMsg.Add($"Started at: {serverStatus.ServerUptime:g}");
                        statusMsg.Add($"{serverStatus.ActivePlayerList.Count} players online");

                        if (!autoStartEnabled) {
                            statusMsg.Add($"AutoStart disabled for this server!");
                        }
                        if (!serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue() && serverStatus.DeployedVersion != null && serviceStatus.LatestVersion != null && serverStatus.DeployedVersion != "None") {
                            Version serverVersion = Version.Parse(serverStatus.DeployedVersion);
                            Version latestVersion = Version.Parse(serviceStatus.LatestVersion);
                            if (serverVersion < latestVersion) {
                                statusMsg.Add($"Update {latestVersion} available");
                            }
                        }
                        statusMsg.Add("  --------------------------------");
                        statusMsg.Add($"Total service-wide backups: {FormManager.MainWindow.ServiceStatus.TotalBackups}");
                        statusMsg.Add($"Total backup size: {FormManager.MainWindow.ServiceStatus.TotalBackupSize / 1000} MB");
                        statusMsg.Add($"Service started: {FormManager.MainWindow.ServiceStatus.ServiceUptime:g}");
                    }
                    UpdateLogBoxInvoked(FormManager.MainWindow.ServerInfoBox, statusMsg.ToArray());
                });
            }
        }

        private string[] ProcessText(List<LogEntry> targetLog) {
            if (FormManager.MainWindow.ConfigManager.DisplayTimestamps) {
                return targetLog.Select(x => $"[{x.TimeStamp:G}] {x.Text}").ToArray();
            }
            return targetLog.Select(x => x.Text).ToArray();
        }

        public bool InitLogThread() {
            _logger.AppendLine("Starting LogManager...");
            DisplayTimestamps = FormManager.MainWindow.ConfigManager.DisplayTimestamps;
            _logTaskCancelSource = new CancellationTokenSource();
            LogUpdateTimer.Start();
            LogRequestTimer.Start();
            return true;
        }

        public void SetConnectedHost(ServiceConfigurator host) => _connectedHost = host;

        public bool StopLogThread() {
            try {
                _logger.AppendLine("Stopping LogManager...");
                _logTaskCancelSource.Cancel();
            } catch (ThreadAbortException e) {
                _logger.AppendLine(e.StackTrace);
            }
            return true;
        }

        private static void UpdateLogBoxInvoked(TextBox targetBox, string[] contents) {
            FormManager.MainWindow.Invoke((MethodInvoker)delegate {
                FormManager.MainWindow.UpdateServerLogBox(targetBox, contents);
            });
        }
    }
}
