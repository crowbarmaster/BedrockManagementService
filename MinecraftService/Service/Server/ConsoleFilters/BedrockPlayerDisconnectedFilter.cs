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
    public class BedrockPlayerDisconnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _bedrockServer;
        ServiceConfigurator _serviceConfiguration;

        public BedrockPlayerDisconnectedFilter(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} disconnected from server {_serverConfiguration.GetServerName()}.");
            _bedrockServer.GetActivePlayerList().Remove(_bedrockServer.GetActivePlayerList().Where(x => x.GetPlayerID() == _bedrockServer.GetPlayerManager().PlayerDisconnected(null, playerInfo.xuid).GetPlayerID()).FirstOrDefault());
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }


        private (string username, string xuid) ExtractPlayerInfoFromString(string input) {
            string playerDisconnected = "Player disconnected: ";
            string xuid = "xuid: ";
            int usernameStart = input.IndexOf(playerDisconnected);
            int xuidStart = input.IndexOf(xuid);
            int usernameLength = (xuidStart - 1) - (usernameStart + playerDisconnected.Length);
            return (input.Substring(usernameStart + playerDisconnected.Length, usernameLength), input.Substring(xuidStart + xuid.Length, 16));
        }
    }
}