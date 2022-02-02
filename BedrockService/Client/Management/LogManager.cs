using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Management {
    class LogManager {
        public Task LogTask;
        public bool Working = false;
        public List<string> ServiceLogs = new List<string>();
        private CancellationTokenSource _logTaskCancelSource;
        private IServiceConfiguration _connectedHost;
        private readonly IBedrockLogger _logger;
        private int currentServerLogLength;
        private int currentServiceLogLength;

        public LogManager(IBedrockLogger logger) {
            _logger = logger;
        }

        private void LogManagerTask() {
            while (!_logTaskCancelSource.Token.IsCancellationRequested) {
                try {
                    Working = true;
                    StringBuilder sendString = new StringBuilder();
                    foreach (ServerConfigurator server in _connectedHost.GetServerList()) {
                        server.ConsoleBuffer = server.ConsoleBuffer ?? new List<LogEntry>();
                        sendString.Append($"{server.ServerName};{server.ConsoleBuffer.Count}|");
                    }
                    sendString.Append($"Service;{_connectedHost.GetLog().Count}");
                    byte[] stringsToBytes = Encoding.UTF8.GetBytes(sendString.ToString());
                    FormManager.TCPClient.SendData(stringsToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.ConsoleLogUpdate);
                    Task.Delay(300).Wait();

                    if (FormManager.MainWindow.selectedServer == null) {
                        UpdateLogBoxInvoked(FormManager.MainWindow.LogBox, "");
                    }
                    if (_connectedHost.GetLog().Count != currentServiceLogLength) {
                        UpdateLogBoxInvoked(FormManager.MainWindow.serviceTextbox, string.Join("\r\n", _connectedHost.GetLog().Select(x => x.Text).ToList()));
                    }
                    if (FormManager.MainWindow.selectedServer != null && FormManager.MainWindow.selectedServer.GetLog() != null && FormManager.MainWindow.selectedServer.GetLog().Count != currentServerLogLength) {
                        UpdateLogBoxInvoked(FormManager.MainWindow.LogBox, string.Join("\r\n", FormManager.MainWindow.selectedServer.GetLog().Select(x => x.Text).ToList()));
                    }
                    FormManager.MainWindow.LogBox.Invoke((MethodInvoker)delegate {
                        currentServerLogLength = FormManager.MainWindow.LogBox.TextLength;
                        currentServiceLogLength = FormManager.MainWindow.serviceTextbox.TextLength;
                    });
                    if (FormManager.MainWindow.selectedServer != null && FormManager.MainWindow.selectedServer.GetStatus() != null) {
                        IServerConfiguration serverConfiguration = FormManager.MainWindow.selectedServer;
                        bool autoStartEnabled = bool.Parse(serverConfiguration.GetProp("ServerAutostartEnabled").ToString());
                        ServerStatusModel status = serverConfiguration.GetStatus();
                        string statusMsg = $"{serverConfiguration.GetServerName()} {status.ServerStatus}\r\n{status.ActivePlayerList.Count} players online";
                        if (!autoStartEnabled) {
                            statusMsg = statusMsg + $"\r\nAutoStart disabled for this server!";
                        }
                        UpdateLogBoxInvoked(FormManager.MainWindow.ServerInfoBox, statusMsg);
                    }
                    if(FormManager.MainWindow.selectedServer != null) {
                        FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Server, FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.ServerStatusRequest);
                    }
                } catch (Exception e) {
                    _logger.AppendLine($"LogManager Error! Stacetrace: {e.StackTrace}");
                }
                Task.Delay(1000).Wait();
            }
        }

        public bool InitLogThread(IServiceConfiguration host) {
            _connectedHost = host;
            _logTaskCancelSource = new CancellationTokenSource();
            return StartLogThread();
        }

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
            FormManager.MainWindow.LogBox.Invoke((MethodInvoker)delegate {
                FormManager.MainWindow.UpdateServerLogBox(targetBox, contents);
            });
        }
    }
}
