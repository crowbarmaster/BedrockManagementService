using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Management {
    class LogManager {
        public Task LogTask;
        public bool EnableFlag;
        public bool Working = false;
        public List<string> ServiceLogs = new List<string>();
        private CancellationTokenSource _logTaskCancelSource;
        private IServiceConfiguration _connectedHost;
        private readonly IBedrockLogger _logger;

        public LogManager(IBedrockLogger logger) {
            _logger = logger;
        }

        private void LogManagerTask() {
            while (!_logTaskCancelSource.Token.IsCancellationRequested) {
                try {
                    Working = true;
                    StringBuilder sendString = new StringBuilder();
                    foreach (ServerInfo server in _connectedHost.GetServerList()) {
                        server.ConsoleBuffer = server.ConsoleBuffer ?? new List<string>();
                        sendString.Append($"{server.ServerName};{server.ConsoleBuffer.Count}|");
                    }
                    sendString.Append($"Service;{_connectedHost.GetLog().Count}");
                    byte[] stringsToBytes = Encoding.UTF8.GetBytes(sendString.ToString());
                    FormManager.TCPClient.SendData(stringsToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.ConsoleLogUpdate);
                    Thread.Sleep(200);
                    int currentLogBoxLength = 0;

                    if (FormManager.MainWindow.selectedServer == null) {
                        UpdateLogBoxInvoked("");
                    }
                    FormManager.MainWindow.LogBox.Invoke((MethodInvoker)delegate {
                        currentLogBoxLength = FormManager.MainWindow.LogBox.TextLength;
                    });
                    if (FormManager.MainWindow.ShowsSvcLog && _connectedHost.GetLog().Count != currentLogBoxLength) {
                        UpdateLogBoxInvoked(string.Join("\r\n", _connectedHost.GetLog()));
                    }
                    else if (!FormManager.MainWindow.ShowsSvcLog && FormManager.MainWindow.selectedServer != null && FormManager.MainWindow.selectedServer.GetLog() != null && FormManager.MainWindow.selectedServer.GetLog().Count != currentLogBoxLength) {
                        UpdateLogBoxInvoked(string.Join("", FormManager.MainWindow.selectedServer.GetLog()));
                    }

                }
                catch (Exception e) {
                    _logger.AppendLine($"LogManager Error! Stacetrace: {e.StackTrace}");
                }
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
            }
            catch (Exception e) {
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
            }
            catch (ThreadAbortException e) {
                _logger.AppendLine(e.StackTrace);
            }
            _logger.AppendLine("LogThread stopped");
            LogTask = null;
            return true;
        }

        private static void UpdateLogBoxInvoked(string contents) {
            FormManager.MainWindow.LogBox.Invoke((MethodInvoker)delegate {
                FormManager.MainWindow.UpdateLogBox(contents);
            });
        }
    }
}
