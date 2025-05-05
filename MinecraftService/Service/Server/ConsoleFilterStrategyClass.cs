using MinecraftService.Service.Server.ConsoleFilters;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Service.Server
{
    public class ConsoleFilterStrategyClass {
        IServerConfiguration _serverConfiguration;
        public Dictionary<string, IConsoleFilter> FilterList;
        public Dictionary<string, IConsoleFilter> JavaFilterList;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _bedrockServer;
        ServiceConfigurator _serviceConfiguration;

        public ConsoleFilterStrategyClass(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
            FilterList = new Dictionary<string, IConsoleFilter> {
                { "Server started", new StartupFlagFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player Spawned", new BedrockPlayerConnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Player disconnected", new BedrockPlayerDisconnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Failed to load Vanilla", new ServerRescErrorFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Version", new BedrockVersionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "A previous save has not been completed.", new SaveIncompleteFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "/level.dat:", new BackupStringFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) }
            };
            JavaFilterList = new Dictionary<string, IConsoleFilter> {
                { "Done (", new StartupFlagFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "[User Authenticator", new JavaPlayerConnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "left the game", new JavaPlayerDisconnectedFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { " lost connection:", new JavaPlayerLostConnectionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Failed to load Vanilla", new ServerRescErrorFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "minecraft server version", new JavaVersionFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "A previous save has not been completed.", new SaveIncompleteFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) },
                { "Saved the game", new JavaBackupDetectFilter(_logger, _configurator, _serverConfiguration, _bedrockServer, _serviceConfiguration ) }
            };
        }
    }
}
