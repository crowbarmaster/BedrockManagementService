using MinecraftService.Shared.SerializeModels;
using Topshelf.Runtime;

namespace MinecraftService.Service.Core {
    public class WindowsService : IService {
        private readonly IMinecraftService _mineraftService;
        private Topshelf.Host? _host;
        private readonly IProcessInfo _processInfo;
        private readonly IServerLogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        TopshelfExitCode _exitCode;

        public WindowsService(IProcessInfo processInfo, IServerLogger logger, IMinecraftService mineraftService, NetworkStrategyLookup networkStrategyLookup, IHostApplicationLifetime appLifetime) {
            _logger = logger;
            _processInfo = processInfo;
            _mineraftService = mineraftService;
            _applicationLifetime = appLifetime;
            _ = networkStrategyLookup;
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnShutdown);
        }

        public async Task InitializeHost() {
            await Task.Run(() => {
                _host = HostFactory.New(hostConfig => {
                    hostConfig.AddCommandLineSwitch("debug", new Action<bool>((x) => { hostConfig.UseNLog(_logger.GetNLogFactory()); }));
                    hostConfig.Service(settings => _mineraftService, s => {
                        s.AfterStartingService(_ => _logger.AppendLine("Service started Successfully."));
                        s.BeforeStoppingService(_ => _logger.AppendLine("Stopping service..."));
                        s.AfterStoppingService(_ => _applicationLifetime.StopApplication());
                    }).RunAsLocalService().AfterInstall(() => {
                        _logger.AppendLine("Service install completed... Exiting!");
                        Task.Delay(1000).Wait();
                        _applicationLifetime.StopApplication();
                    }).AfterUninstall(() => {
                        _logger.AppendLine("Service uninstall completed... Exiting!");
                        Task.Delay(1000).Wait();
                        _applicationLifetime.StopApplication();
                    }).EnableServiceRecovery(src => {
                        src.RestartService(delayInMinutes: 0);
                        src.RestartService(delayInMinutes: 1);
                        src.SetResetPeriod(days: 1);
                    });
                    hostConfig.SetStartTimeout(TimeSpan.FromSeconds(120));
                    hostConfig.SetStopTimeout(TimeSpan.FromSeconds(120));
                    hostConfig.UseAssemblyInfoForServiceInfo();
                    hostConfig.SetDescription("Minecraft Management Service for Windows 10");
                    hostConfig.SetDisplayName("MinecraftService");
                    hostConfig.SetServiceName("MinecraftService");
                    hostConfig.UnhandledExceptionPolicy = UnhandledExceptionPolicyCode.LogErrorOnly;
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
            if (_host != null) {
                Task.Run(() => {
                    _exitCode = _host.Run();
                });
            }
        }

        private void OnStopping() {
            if (Environment.UserInteractive) {
                _mineraftService.Stop(null);
                while (_mineraftService.GetServiceStatus().ServiceStatus != ServiceStatus.Stopped) {
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
