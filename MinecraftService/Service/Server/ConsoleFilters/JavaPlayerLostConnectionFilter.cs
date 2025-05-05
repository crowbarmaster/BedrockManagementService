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
    public class JavaPlayerLostConnectionFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _bedrockServer;
        ServiceConfigurator _serviceConfiguration;

        public JavaPlayerLostConnectionFilter(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            if (_bedrockServer.GetActivePlayerList().Any()) {
                if(_bedrockServer.GetActivePlayerList().Remove(_bedrockServer.GetActivePlayerList().Where(x => x.GetPlayerID() == _bedrockServer.GetPlayerManager().PlayerDisconnected(playerInfo.username, playerInfo.xuid).GetPlayerID()).FirstOrDefault())) {
                    _logger.AppendLine($"Player {playerInfo.username} has lost connection from server {_serverConfiguration.GetServerName()}, and has been removed from active players.");
                }
            }
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }

        private (string username, string xuid) ExtractPlayerInfoFromString(string input) {
            int msgStartIndex = input.IndexOf("]:") + 3;
            int usernameEnd = 0;
            if (input.Contains(" (/")) {
                usernameEnd = input.IndexOf(" (/", msgStartIndex);
            } else {
                usernameEnd = input.IndexOf(" lost connection", msgStartIndex);
            }
            int usernameLength = usernameEnd - msgStartIndex;
            return (input.Substring(msgStartIndex, usernameLength), "");
        }
    }
}