using BedrockService.Service.Core;
using BedrockService.Service.Management;
using BedrockService.Service.Networking;
using BedrockService.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Topshelf;

namespace BedrockService.Service
{
    class Program
    {
        public static bool IsExiting = false;
        static bool isDebugEnabled = false;
        static bool isConsoleMode = false;

        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup(isDebugEnabled, isConsoleMode);
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IConfigurator>().LoadAllConfigurations().Wait();
            serviceProvider.GetRequiredService<IUpdater>().CheckUpdates().Wait();
            IService service = serviceProvider.GetRequiredService<IService>();
            ILogger Logger = serviceProvider.GetRequiredService<ILogger>();
            IProcessInfo ProcessInfo = serviceProvider.GetRequiredService<IProcessInfo>();
            if (args.Length == 0 || Environment.UserInteractive)
            {
                isConsoleMode = true;
                Logger.AppendLine("BedrockService startup detected in Console mode.");
            }
            else
            {
                Logger.AppendLine("BedrockService startup detected in Service mode.");
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.Id != ProcessInfo.GetProcessPID() && process.ProcessName.StartsWith("BedrockService.") && process.ProcessName != "BedrockService.Client")
                    {
                        Logger.AppendLine($"Found additional running instance of {process.ProcessName} with ID {process.Id}");
                        Logger.AppendLine($"Killing process with id {process.Id}");
                        process.Kill();
                    }
                }
            }
            if (args.Length > 0)
            {
                isDebugEnabled = args[0].ToLower() == "-debug";
            }

            TopshelfExitCode rc = service.Run();
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            if (isDebugEnabled)
            {
                Console.Write("Program is force-quitting. Press any key to exit.");
                Console.Out.Flush();
                Console.ReadLine();
            }
            Environment.ExitCode = exitCode;
        }
    }
}
