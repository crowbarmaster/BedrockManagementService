using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters
{
    public class JavaPlayerDisconnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _bedrockServer;
        ServiceConfigurator _serviceConfiguration;

        public JavaPlayerDisconnectedFilter(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} disconnected from server {_serverConfiguration.GetServerName()}.");
            _bedrockServer.GetActivePlayerList().Remove(_bedrockServer.GetActivePlayerList().Where(x => x.GetPlayerID() == _bedrockServer.GetPlayerManager().PlayerDisconnected(null, playerInfo.xuid).GetPlayerID()).First());
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }

        private (string username, string xuid) ExtractPlayerInfoFromString(string input) {
            int msgStartIndex = input.IndexOf("]:") + 3;
            int usernameEnd = input.IndexOf(" left the game", msgStartIndex);
            int usernameLength = usernameEnd - msgStartIndex;
            return (input.Substring(msgStartIndex, usernameLength), "");
        }
    }
}