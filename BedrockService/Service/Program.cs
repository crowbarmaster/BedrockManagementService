using System;
using System.Threading;
using System.IO;
using System.Reflection;
using Topshelf;
using Topshelf.Runtime;
using BedrockService.Service.Server;
using System.Diagnostics;

namespace BedrockService.Service
{
    class Program
    {
        public static readonly string ServiceDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string ServiceExeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
        public static bool DebugModeEnabled = false;
        public static bool IsConsoleMode = false;
        public static bool IsExiting = false;

        static void Main(string[] args)
        {
            Process[] processList;
            if (args.Length == 0 && Environment.UserInteractive)
            {
                IsConsoleMode = true;
                InstanceProvider.ServiceLogger.AppendLine("BedrockService startup detected in Console mode.");
            }
            else
            {
                InstanceProvider.ServiceLogger.AppendLine("BedrockService startup detected in Service mode.");
                do
                {
                    processList = Process.GetProcessesByName(ServiceExeName.Substring(0, ServiceExeName.Length - 4));
                    Thread.Sleep(500);
                }
                while (processList.Length > 1);
            }

            Host host = HostFactory.New(x =>
            {
                x.SetStartTimeout(TimeSpan.FromSeconds(10));
                x.SetStopTimeout(TimeSpan.FromSeconds(10));
                x.UseAssemblyInfoForServiceInfo();
                x.Service(settings => InstanceProvider.BedrockService, s =>
                {
                    s.BeforeStartingService(_ => InstanceProvider.ServiceLogger.AppendLine("Starting service..."));
                    s.BeforeStoppingService(_ => 
                    {
                        InstanceProvider.ServiceLogger.AppendLine("Stopping service...");
                        IsExiting = true;
                        foreach (BedrockServer server in InstanceProvider.BedrockService.bedrockServers)
                        {
                            server.CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                            while (server.CurrentServerStatus != BedrockServer.ServerStatus.Stopped)
                                Thread.Sleep(100);
                        }
                        InstanceProvider.GetTcpListener().Stop();
                    });
                });

                x.RunAsLocalSystem();
                x.SetDescription("Windows Service Wrapper for Windows Bedrock Server");
                x.SetDisplayName("BedrockService");
                x.SetServiceName("BedrockService");
                x.UnhandledExceptionPolicy = UnhandledExceptionPolicyCode.LogErrorOnly;

                x.EnableServiceRecovery(src =>
                {
                    src.RestartService(delayInMinutes: 0);
                    src.RestartService(delayInMinutes: 1);
                    src.SetResetPeriod(days: 1);
                });

                x.OnException((ex) =>
                {
                    InstanceProvider.ServiceLogger.AppendLine("Exception occured Main : " + ex.ToString());
                });
            });

            TopshelfExitCode rc = host.Run();
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            if (DebugModeEnabled)
            {
                Console.Write("Program is force-quitting. Press any key to exit.");
                Console.Out.Flush();
                Console.ReadLine();
            }
            Environment.ExitCode = exitCode;
        }
    }
}
