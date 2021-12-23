using BedrockService.Service.Core.Interfaces;
using BedrockService.Service.Server;
using Topshelf.Runtime;

namespace BedrockService.Service.Core {
    public class Service : IService {
        private readonly IBedrockService _bedrockService;
        private Topshelf.Host _host;
        private readonly IBedrockLogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public Service(IBedrockLogger logger, IBedrockService bedrockService, NetworkStrategyLookup networkStrategyLookup, IHostApplicationLifetime appLifetime) {
            _logger = logger;
            _bedrockService = bedrockService;
            _applicationLifetime = appLifetime;
            appLifetime.ApplicationStarted.Register(OnStarted);
        }

        public async Task InitializeHost() {
            await Task.Run(() => {
                _host = HostFactory.New(hostConfig => {
                    hostConfig.SetStartTimeout(TimeSpan.FromSeconds(10));
                    hostConfig.SetStopTimeout(TimeSpan.FromSeconds(10));
                    hostConfig.UseAssemblyInfoForServiceInfo();
                    hostConfig.Service(settings => _bedrockService, s => {
                        s.BeforeStartingService(_ => _logger.AppendLine("Starting service..."));
                        s.BeforeStoppingService(_ => {
                            _logger.AppendLine("Stopping service...");
                            foreach (IBedrockServer server in _bedrockService.GetAllServers()) {
                                server.SetServerStatus(BedrockServer.ServerStatus.Stopping);
                                while (server.GetServerStatus() != BedrockServer.ServerStatus.Stopped)
                                    Thread.Sleep(100);
                            }
                            Environment.Exit(0);
                        });
                    });
                    hostConfig.EnableShutdown();
                    hostConfig.EnableHandleCtrlBreak();
                    hostConfig.RunAsLocalSystem();
                    hostConfig.SetDescription("Windows Service Wrapper for Windows Bedrock Server");
                    hostConfig.SetDisplayName("BedrockService");
                    hostConfig.SetServiceName("BedrockService");
                    hostConfig.UnhandledExceptionPolicy = UnhandledExceptionPolicyCode.LogErrorOnly;
                    hostConfig.AfterInstall(() => {
                        _logger.AppendLine("Service install completed... Exiting!");
                        Task.Delay(1000).Wait();
                        _applicationLifetime.StopApplication();
                    });
                    hostConfig.AfterUninstall(() => {
                        _logger.AppendLine("Service uninstall completed... Exiting!");
                        Task.Delay(1000).Wait();
                        _applicationLifetime.StopApplication();
                    });
                    hostConfig.EnableServiceRecovery(src => {
                        src.RestartService(delayInMinutes: 0);
                        src.RestartService(delayInMinutes: 1);
                        src.SetResetPeriod(days: 1);
                    });
                    hostConfig.OnException((ex) => {
                        _logger.AppendLine("Exception occured Main : " + ex.Message);
                    });
                });

            });
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            return InitializeHost();
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.Run(() => Environment.Exit(0));
        }

        private void OnStarted() {
            Task.Run(() => { 
                _host.Run();
            });
        }
    }
}
