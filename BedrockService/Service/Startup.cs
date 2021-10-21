using BedrockService.Service.Core;
using BedrockService.Service.Logging;
using BedrockService.Service.Management;
using BedrockService.Service.Networking;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BedrockService.Service
{
    public class Startup
    {
        bool isDebug = false;
        bool isConsole = false;

        public Startup(bool isDebug, bool isConsole)
        {
            this.isDebug = isDebug;
            this.isConsole = isConsole;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            IProcessInfo processInfo = new ServiceProcessInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(Assembly.GetExecutingAssembly().Location), Process.GetCurrentProcess().Id, isDebug, isConsole);
            services.AddSingleton(processInfo);
            services.AddSingleton<IService, Core.Service>();
            services.AddSingleton<IServiceConfiguration, ServiceInfo>();
            services.AddSingleton<ITCPListener, TCPListener>();
            services.AddSingleton<ILogger, ServiceLogger>();
            services.AddSingleton<NetworkStrategyLookup>();
            services.AddTransient<IConfigurator, ConfigManager>();
            services.AddSingleton<IBedrockService, Core.BedrockService>();
            services.AddTransient<IUpdater, Updater>();
        }
    }
}
