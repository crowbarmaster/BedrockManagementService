using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class BedrockPlayerDisconnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public BedrockPlayerDisconnectedFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} disconnected from server {_serverConfiguration.GetServerName()}.");
            _bedrockServer.GetActivePlayerList().Remove(_bedrockServer.GetPlayerManager().PlayerDisconnected(null, playerInfo.xuid));
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }

        private (string username, string xuid) ExtractPlayerInfoFromString(string input) {
            int msgStartIndex = input.IndexOf(']') + 2;
            int usernameStart = input.IndexOf(':', msgStartIndex) + 2;
            int usernameEnd = input.IndexOf(',', usernameStart);
            int usernameLength = usernameEnd - usernameStart;
            int xuidStart = input.IndexOf(':', usernameEnd) + 2;
            return (input.Substring(usernameStart, usernameLength), input.Substring(xuidStart, input.Length - xuidStart));
        }

    }
}