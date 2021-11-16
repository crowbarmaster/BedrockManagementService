using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BedrockService.Client.Management
{
    class LogManager
    {
        public Thread LogThread;
        public bool EnableFlag;
        public bool Working = false;
        public List<string> ServiceLogs = new List<string>();
        private IServiceConfiguration connectedHost;
        private readonly IBedrockLogger Logger;

        public LogManager(IBedrockLogger logger)
        {
            Logger = logger;
        }

        private void LogManagerTask()
        {
            while (FormManager.TCPClient.Connected)
            {
                try
                {
                    Working = true;
                    StringBuilder sendString = new StringBuilder();
                    foreach (ServerInfo server in connectedHost.GetServerList())
                    {
                        server.ConsoleBuffer = server.ConsoleBuffer ?? new List<string>();
                        sendString.Append($"{server.ServerName};{server.ConsoleBuffer.Count}|");
                    }
                    sendString.Append($"Service;{connectedHost.GetLog().Count}");
                    byte[] stringsToBytes = Encoding.UTF8.GetBytes(sendString.ToString());
                    FormManager.TCPClient.SendData(stringsToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.ConsoleLogUpdate);
                    Thread.Sleep(200);
                    int currentLogBoxLength = 0;

                    if (FormManager.MainWindow.selectedServer == null)
                    {
                        UpdateLogBoxInvoked("");
                    }
                    FormManager.MainWindow.LogBox.Invoke((MethodInvoker)delegate
                    {
                        currentLogBoxLength = FormManager.MainWindow.LogBox.TextLength;
                    });
                    if (FormManager.MainWindow.ShowsSvcLog && connectedHost.GetLog().Count != currentLogBoxLength)
                    {
                        UpdateLogBoxInvoked(string.Join("\r\n", connectedHost.GetLog()));
                    }
                    else if (!FormManager.MainWindow.ShowsSvcLog && FormManager.MainWindow.selectedServer.GetLog() != null && FormManager.MainWindow.selectedServer.GetLog().Count != currentLogBoxLength)
                    {
                        UpdateLogBoxInvoked(string.Join("", FormManager.MainWindow.selectedServer.GetLog()));
                    }

                }
                catch (Exception e)
                {
                    Logger.AppendLine($"LogManager Error! Stacetrace: {e.StackTrace}");
                }
            }
        }

        public bool InitLogThread(IServiceConfiguration host)
        {
            connectedHost = host;
            return StartLogThread();
        }

        public bool StartLogThread()
        {
            try
            {
                if (LogThread != null && LogThread.IsAlive)
                    LogThread.Abort();
                LogThread = new Thread(new ThreadStart(LogManagerTask));
                LogThread.Name = "LogThread";
                LogThread.IsBackground = true;
                EnableFlag = true;
                LogThread.Start();
                Logger.AppendLine("LogThread started");
                return true;
            }
            catch (Exception e)
            {
                Logger.AppendLine($"Error starting LogThread: {e.StackTrace}");
            }
            return false;
        }

        public bool StopLogThread()
        {
            if (LogThread == null)
            {
                return true;
            }
            try
            {
                LogThread.Abort();
            }
            catch (ThreadAbortException e)
            {
                Logger.AppendLine(e.StackTrace);
            }
            Logger.AppendLine("LogThread stopped");
            LogThread = null;
            return true;
        }

        private static void UpdateLogBoxInvoked(string contents)
        {
            FormManager.MainWindow.LogBox.Invoke((MethodInvoker)delegate
            {
                FormManager.MainWindow.UpdateLogBox(contents);
            });
        }
    }
}
