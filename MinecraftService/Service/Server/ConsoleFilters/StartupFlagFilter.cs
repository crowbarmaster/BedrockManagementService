using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters
{
    public class StartupFlagFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _bedrockServer;
        ServiceConfigurator _serviceConfiguration;

        public StartupFlagFilter(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
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
