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
        public static Thread LogThread;
        public static bool EnableFlag;
        public static bool Working = false;
        public static List<string> ServiceLogs = new List<string>();
        private static IServiceConfiguration connectedHost;

        private static void LogManagerTask()
        {
            while (FormManager.GetTCPClient.Connected)
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
                    FormManager.GetTCPClient.SendData(stringsToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.ConsoleLogUpdate);
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
                    if (FormManager.GetMainWindow.ShowsSvcLog && connectedHost.GetLog().Count != currentLogBoxLength)
                    {
                        UpdateLogBoxInvoked(string.Join("\r\n", connectedHost.GetLog()));
                    }
                    else if (FormManager.GetMainWindow.selectedServer.GetLog() != null && FormManager.GetMainWindow.selectedServer.GetLog().Count != currentLogBoxLength)
                    {
                        UpdateLogBoxInvoked(string.Join("", FormManager.GetMainWindow.selectedServer.GetLog()));
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine($"LogManager Error! Stacetrace: {e.StackTrace}");
                }
            }
        }

        public static bool InitLogThread(IServiceConfiguration host)
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
