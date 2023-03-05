using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class StartupFlagFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IBedrockLogger _logger;
        IConfigurator _configurator;
        IBedrockServer _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public StartupFlagFilter(IBedrockLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IBedrockServer bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            _bedrockServer.SetStartupStatus(ServerStatus.Started);
            Task.Delay(3000).Wait();
            if (_serverConfiguration.GetStartCommands().Count > 0) {
                _bedrockServer.RunStartupCommands();
            }
        }
    }
}
