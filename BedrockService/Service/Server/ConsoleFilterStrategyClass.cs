using BedrockService.Service.Server.ConsoleFilters;
using BedrockService.Service.Server.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Server {
    public class ConsoleFilterStrategyClass {
        IServerConfiguration _serverConfiguration;
        public Dictionary<string, IConsoleFilter> FilterList;
        IBedrockLogger _logger;
        IConfigurator _configurator;
        IBedrockServer _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public ConsoleFilterStrategyClass(IBedrockLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IBedrockServer bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
            FilterList = new Dictionary<string, IConsoleFilter> {
                { "IPv6 supported, port", new StartupFlagFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player connected", new PlayerConnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player disconnected", new PlayerDisconnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Failed to load Vanilla", new ServerRescErrorFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Version ", new VersionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "A previous save has not been completed.", new SaveIncompleteFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "/level.dat:", new BackupStringFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) }
            };
        }
    }
}
