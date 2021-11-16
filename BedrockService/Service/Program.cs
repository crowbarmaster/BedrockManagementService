// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BedrockService.Service.Core;
using BedrockService.Service.Logging;
using BedrockService.Service.Management;
using BedrockService.Service.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topshelf;

namespace BedrockService.Service
{
    public class Program
    {
        public static bool IsExiting = false;
        private static bool _isDebugEnabled;
        private static bool _isConsoleMode = false;
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                _isDebugEnabled = args[0].ToLower() == "-debug";
            }
            if (args.Length == 0 || Environment.UserInteractive)
            {
                _isConsoleMode = true;
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
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
