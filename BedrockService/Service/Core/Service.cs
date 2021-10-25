using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Core.Threads;
using BedrockService.Service.Networking;
using BedrockService.Service.Server;
using BedrockService.Shared.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Runtime;

namespace BedrockService.Service.Core
{
    public class Service : IService
    {
        private IBedrockService BedrockService { get; }
        private Host host;
        private readonly IServiceConfiguration serviceConfiguration;
        private readonly ILogger logger;

        public Service(ILogger logger, IBedrockService bedrockService, IServiceConfiguration serviceConfiguration)
        {
            this.logger = logger;
            this.serviceConfiguration = serviceConfiguration;
            BedrockService = bedrockService;
        }

        public async Task InitializeHost()
        {
            await Task.Run(() =>
            {
                host = HostFactory.New(x =>
                {
                    x.SetStartTimeout(TimeSpan.FromSeconds(10));
                    x.SetStopTimeout(TimeSpan.FromSeconds(10));
                    x.UseAssemblyInfoForServiceInfo();
                    x.Service(settings => BedrockService, s =>
                    {
                        s.BeforeStartingService(_ => logger.AppendLine("Starting service..."));
                        s.BeforeStoppingService(_ =>
                        {
                            logger.AppendLine("Stopping service...");
                            foreach (BedrockServer server in BedrockService.GetAllServers())
                            {
                                server.CurrentServerStatus = BedrockServer.ServerStatus.Stopping;
                                while (server.CurrentServerStatus != BedrockServer.ServerStatus.Stopped)
                                    Thread.Sleep(100);
                            }
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
                        logger.AppendLine("Exception occured Main : " + ex.Message);
                    });
                });

            });
        }

        public TopshelfExitCode Run() => host.Run();
    }
}
