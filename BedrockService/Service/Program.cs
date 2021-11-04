using BedrockService.Service.Core;
using BedrockService.Service.Logging;
using BedrockService.Service.Management;
using BedrockService.Service.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Topshelf;

namespace BedrockService.Service
{
    class Program
    {
        public static bool IsExiting = false;
        private static bool _isDebugEnabled = false;
        private static bool _isConsoleMode = false;
        private static readonly IServiceCollection _services = new ServiceCollection();

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                _isDebugEnabled = args[0].ToLower() == "-debug";
            }
            ConfigureServices(_services);
            IServiceProvider serviceProvider = _services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IConfigurator>().LoadAllConfigurations().Wait();
            serviceProvider.GetRequiredService<IUpdater>().CheckUpdates().Wait();
            IService service = serviceProvider.GetRequiredService<IService>();
            ILogger Logger = serviceProvider.GetRequiredService<ILogger>();
            IProcessInfo ProcessInfo = serviceProvider.GetRequiredService<IProcessInfo>();
            serviceProvider.GetRequiredService<NetworkStrategyLookup>();
            if (args.Length == 0 || Environment.UserInteractive)
            {
                _isConsoleMode = true;
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
            service.InitializeHost().Wait();
            TopshelfExitCode rc = service.Run();
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            if (_isDebugEnabled)
            {
                Console.Write("Program is force-quitting. Press any key to exit.");
                Console.Out.Flush();
                Console.ReadLine();
            }
            Environment.ExitCode = exitCode;
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            IProcessInfo processInfo = new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, _isDebugEnabled, _isConsoleMode);
            services.AddSingleton(processInfo);
            services.AddSingleton<IService, Core.Service>();
            services.AddSingleton<IServiceConfiguration, ServiceInfo>();
            services.AddSingleton<ITCPListener, TCPListener>();
            services.AddSingleton<ILogger, ServiceLogger>();
            services.AddSingleton<NetworkStrategyLookup>();
            services.AddSingleton<IConfigurator, ConfigManager>();
            services.AddSingleton<IBedrockService, Core.BedrockService>();
            services.AddSingleton<IUpdater, Updater>();
        }
    }
}
