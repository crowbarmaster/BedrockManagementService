using Topshelf.Runtime;

namespace BedrockService.Service.Core {
    public class Service : IService {
        private readonly IBedrockService _bedrockService;
        private Topshelf.Host _host;
        private readonly IBedrockLogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        TopshelfExitCode _exitCode;

        public Service(IBedrockLogger logger, IBedrockService bedrockService, NetworkStrategyLookup networkStrategyLookup, IHostApplicationLifetime appLifetime) {
            _logger = logger;
            _bedrockService = bedrockService;
            _applicationLifetime = appLifetime;
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnShutdown);
        }

        public async Task InitializeHost() {
            await Task.Run(() => {
                _host = HostFactory.New(hostConfig => {
                    hostConfig.SetStartTimeout(TimeSpan.FromSeconds(120));
                    hostConfig.SetStopTimeout(TimeSpan.FromSeconds(120));
                    hostConfig.UseAssemblyInfoForServiceInfo();
                    hostConfig.Service(settings => _bedrockService, s => {
                        s.BeforeStartingService(_ => _logger.AppendLine("Starting service..."));
                        s.BeforeStoppingService(_ => _logger.AppendLine("Stopping service..."));
                        s.AfterStoppingService(_ => _applicationLifetime.StopApplication());
                    });
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
            return Task.CompletedTask;
        }

        private void OnStarted() {
            Task.Run(() => {
                _exitCode = _host.Run();
            });
        }

        private void OnStopping() {
            if (Environment.UserInteractive) {
                _bedrockService.Stop(null);
                while (_bedrockService.GetServiceStatus() != ServiceStatus.Stopped) {
                    Task.Delay(100).Wait();
                }
            }
        }

        private void OnShutdown() {
            int exitCode = (int)Convert.ChangeType(_exitCode, _exitCode.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
