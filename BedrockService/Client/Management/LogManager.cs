using BedrockService.Client.Forms;
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
                    MainWindow mainWindow = FormManager.GetMainWindow;

                    if (MainWindow.ShowsSvcLog)
                    {
                        mainWindow.LogBox.Invoke((MethodInvoker)delegate
                        {
                            mainWindow.LogBox.Text = connectedHost.ServiceLogToString();
                            mainWindow.LogBox.Select(mainWindow.LogBox.Text.Length - 1, 0);
                            mainWindow.LogBox.Refresh();

                        });
                    }
                    else
                    {
                        if (MainWindow.selectedServer == null)
                        {
                            mainWindow.Invoke((MethodInvoker)delegate
                            {
                                mainWindow.LogBox.Text = "";
                                mainWindow.LogBox.Refresh();
                            });
                            return;
                        }
                        else if (MainWindow.selectedServer.ConsoleBuffer != null && MainWindow.selectedServer.ConsoleBuffer.Count() != 0)
                        {

                            mainWindow.LogBox.Invoke((MethodInvoker)delegate
                            {
                                mainWindow.LogBox.Text = MainWindow.selectedServer.ConsoleBuffer.ToString();
                                mainWindow.LogBox.Select(mainWindow.LogBox.Text.Length - 1, 0);
                                mainWindow.LogBox.Refresh();

                            });
                        }
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
                if (LogThread != null || (LogThread != null && LogThread.IsAlive))
                {
                    return false;
                }
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
    }
}
