using BedrockService.Service.Server.HostInfoClasses;
using BedrockService.Service.Server.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BedrockService.Client.Management
{
    class LogManager
    {
        public static Thread LogThread;
        public static bool EnableFlag;
        public static bool Working = false;
        public static List<string> ServiceLogs = new List<string>();
        private static HostInfo connectedHost;

        private static void LogManagerTask()
        {
            while (FormManager.GetTCPClient.Connected)
            {
                try
                {
                    Working = true;
                    StringBuilder sendString = new StringBuilder();
                    foreach (ServerInfo server in connectedHost.GetServerInfos())
                    {
                        server.ConsoleBuffer = server.ConsoleBuffer ?? new ServerLogger(server.ServerName);
                        sendString.Append($"{server.ServerName};{server.ConsoleBuffer.Count()}|");
                    }
                    sendString.Append($"Service;{connectedHost.ServiceLog.Count}");
                    byte[] stringsToBytes = Encoding.UTF8.GetBytes(sendString.ToString());
                    FormManager.GetTCPClient.SendData(stringsToBytes, Service.Networking.NetworkMessageSource.Client, Service.Networking.NetworkMessageDestination.Service, Service.Networking.NetworkMessageTypes.ConsoleLogUpdate);
                    Thread.Sleep(200);
                    int currentLogBoxLength = 0;

                    FormManager.GetMainWindow.LogBox.Invoke((MethodInvoker)delegate
                    {
                        currentLogBoxLength = FormManager.GetMainWindow.LogBox.TextLength;
                    });
                    if (FormManager.GetMainWindow.selectedServer == null)
                    {
                        UpdateLogBoxInvoked("");
                    }
                    if (FormManager.GetMainWindow.ShowsSvcLog && connectedHost.ServiceLog.Count != currentLogBoxLength)
                    {
                        UpdateLogBoxInvoked(connectedHost.ServiceLogToString());
                    }
                    else if (FormManager.GetMainWindow.selectedServer.ConsoleBuffer != null && FormManager.GetMainWindow.selectedServer.ConsoleBuffer.Count() != currentLogBoxLength)
                    {
                        UpdateLogBoxInvoked(FormManager.GetMainWindow.selectedServer.ConsoleBuffer.ToString());
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"LogManager Error! Stacetrace: {e.StackTrace}");
                }
            }
        }

        public static bool InitLogThread(HostInfo host)
        {
            connectedHost = host;
            return StartLogThread();
        }

        public static bool StartLogThread()
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
                Console.WriteLine("LogThread started");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error starting LogThread: {e.StackTrace}");
            }
            return false;
        }

        public static bool StopLogThread()
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
                Console.WriteLine(e.StackTrace);
            }
            Console.WriteLine("LogThread stopped");
            LogThread = null;
            return true;
        }

        private static void UpdateLogBoxInvoked(string contents)
        {
            FormManager.GetMainWindow.LogBox.Invoke((MethodInvoker)delegate
            {
                FormManager.GetMainWindow.UpdateLogBox(contents);
            });
        }
    }
}
