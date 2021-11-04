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
        private readonly IBedrockService _bedrockService;
        private Host _host;
        private readonly ILogger _logger;

        public Service(ILogger logger, IBedrockService bedrockService)
        {
            _logger = logger;
            _bedrockService = bedrockService;
        }

        public async Task InitializeHost()
        {
            await Task.Run(() =>
            {
                _host = HostFactory.New(hostConfig =>
                {
                    hostConfig.SetStartTimeout(TimeSpan.FromSeconds(10));
                    hostConfig.SetStopTimeout(TimeSpan.FromSeconds(10));
                    hostConfig.UseAssemblyInfoForServiceInfo();
                    hostConfig.Service(settings => _bedrockService, s =>
                    {
                        s.BeforeStartingService(_ => _logger.AppendLine("Starting service..."));
                        s.BeforeStoppingService(_ =>
                        {
                            _logger.AppendLine("Stopping service...");
                            foreach (IBedrockServer server in _bedrockService.GetAllServers())
                            {
                                server.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                                while (server.GetServerStatus() != BedrockServer.ServerStatus.Stopped)
                                    Thread.Sleep(100);
                            }
                        });
                    });

                    hostConfig.RunAsLocalSystem();
                    hostConfig.SetDescription("Windows Service Wrapper for Windows Bedrock Server");
                    hostConfig.SetDisplayName("BedrockService");
                    hostConfig.SetServiceName("BedrockService");
                    hostConfig.UnhandledExceptionPolicy = UnhandledExceptionPolicyCode.LogErrorOnly;

                    hostConfig.EnableServiceRecovery(src =>
                    {
                        src.RestartService(delayInMinutes: 0);
                        src.RestartService(delayInMinutes: 1);
                        src.SetResetPeriod(days: 1);
                    });

                    hostConfig.OnException((ex) =>
                    {
                        _logger.AppendLine("Exception occured Main : " + ex.Message);
                    });
                });

            });
        }

        public TopshelfExitCode Run() => _host.Run();
    }
}
