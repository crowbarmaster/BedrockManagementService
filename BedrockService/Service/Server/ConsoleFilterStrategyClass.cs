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
        public Dictionary<string, IConsoleFilter> LLFilterList;
        public Dictionary<string, IConsoleFilter> JavaFilterList;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public ConsoleFilterStrategyClass(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
            FilterList = new Dictionary<string, IConsoleFilter> {
                { "Server started", new StartupFlagFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player connected", new BedrockPlayerConnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player disconnected", new BedrockPlayerDisconnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Failed to load Vanilla", new ServerRescErrorFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Version", new BedrockVersionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "A previous save has not been completed.", new SaveIncompleteFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "/level.dat:", new BackupStringFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) }
            };
            LLFilterList = new Dictionary<string, IConsoleFilter> {
                { "IPv6 supported, port", new StartupFlagFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player connected", new BedrockPlayerConnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player disconnected", new BedrockPlayerDisconnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Failed to load Vanilla", new ServerRescErrorFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Version", new LLVersionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "A previous save has not been completed.", new SaveIncompleteFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "/level.dat:", new BackupStringFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) }
            };
            JavaFilterList = new Dictionary<string, IConsoleFilter> {
                { "Done (", new StartupFlagFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "joined the game", new BedrockPlayerConnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "left the game", new BedrockPlayerDisconnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Failed to load Vanilla", new ServerRescErrorFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "minecraft server version", new JavaVersionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "A previous save has not been completed.", new SaveIncompleteFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "/level.dat:", new BackupStringFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) }
            };
        }
    }
}
