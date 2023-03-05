using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class ServerRescErrorFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IBedrockLogger _logger;
        IConfigurator _configurator;
        IBedrockServer _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public ServerRescErrorFilter(IBedrockLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IBedrockServer bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            throw new NotImplementedException();
        }
    }
}