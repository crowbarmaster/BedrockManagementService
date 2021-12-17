// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

global using BedrockService.Service.Logging;
global using BedrockService.Service.Management;
global using BedrockService.Service.Networking;
global using BedrockService.Shared.Classes;
global using BedrockService.Shared.Interfaces;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Threading;
global using System.Threading.Tasks;
global using Topshelf;
using BedrockService.Service.Core.Interfaces;

namespace BedrockService.Service {
    public class Program {
        public static bool IsExiting = false;
        private static bool _isDebugEnabled = false;
        private static bool _isConsoleMode = false;
        private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static CancellationToken token = CancellationTokenSource.Token;
        public static void Main(string[] args) {
            if (args.Length > 0) {
                Console.WriteLine(string.Join(" ", args));
                _isDebugEnabled = args[0].ToLower() == "-debug";
            }
            if (args.Length == 0 || Environment.UserInteractive) {
                _isConsoleMode = true;
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    IProcessInfo processInfo = new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, _isDebugEnabled, _isConsoleMode);
                    services.AddHostedService<Core.Service>()
                        .AddSingleton(processInfo)
                        .AddSingleton<NetworkStrategyLookup>()
                        .AddSingleton<IServiceConfiguration, ServiceInfo>()
                        .AddSingleton<ITCPListener, TCPListener>()
                        .AddSingleton<IBedrockLogger, ServiceLogger>()
                        .AddSingleton<IConfigurator, ConfigManager>()
                        .AddSingleton<IBedrockService, Core.BedrockService>()
                        .AddSingleton<IUpdater, Updater>();
                });
    }
}
