using System;
using System.IO;
using System.Reflection;
using Topshelf;
using Topshelf.Runtime;

namespace BedrockService.Service
{
    class Program
    {
        public static readonly string ServiceDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool DebugModeEnabled = false;
        public static bool IsConsoleMode = false;
        static void Main(string[] args)
        {
            if (args.Length == 0 && Environment.UserInteractive)
            {
                IsConsoleMode = true;
                InstanceProvider.GetServiceLogger().AppendLine("BedrockService startup detected in Console mode."); ;
            }
            Host host = HostFactory.New(x =>
            {
                x.SetStartTimeout(TimeSpan.FromSeconds(10));
                x.SetStopTimeout(TimeSpan.FromSeconds(10));
                //x.UseLog4Net();
                x.UseAssemblyInfoForServiceInfo();
                x.Service(settings => InstanceProvider.GetBedrockService(), s =>
                {
                    s.BeforeStartingService(_ => InstanceProvider.GetServiceLogger().AppendLine("Starting service..."));
                    s.BeforeStoppingService(_ => InstanceProvider.GetServiceLogger().AppendLine("Stopping service..."));

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
                    InstanceProvider.GetServiceLogger().AppendLine("Exception occured Main : " + ex.ToString());
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

        private static void Program_Exited(object sender, EventArgs e)
        {
            InstanceProvider.GetServiceLogger().AppendLine("Program exit detected... shutting down!");
            InstanceProvider.GetBedrockService().Stop(InstanceProvider.GetBedrockService()._hostControl);
        }
    }
}
