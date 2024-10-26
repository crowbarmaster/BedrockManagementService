using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class BedrockPlayerConnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        ServiceConfigurator _serviceConfiguration;

        public BedrockPlayerConnectedFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} connected with XUID: {playerInfo.xuid} to server {_serverConfiguration.GetServerName()}.");
            _bedrockServer.SetServerModified(true);
            _bedrockServer.GetActivePlayerList().Add(_bedrockServer.GetPlayerManager().PlayerConnected(playerInfo.username, playerInfo.xuid));
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }

        // Player Spawned: Crowbarmast3r xuid: 0123456789012345, pfid: 0a1b2c3d4e5f6g7h
        private (string username, string xuid) ExtractPlayerInfoFromString(string input) {
            string playerConnected = "Player Spawned: ";
            string xuid = "xuid: ";
            int usernameStart = input.IndexOf(playerConnected);
            int xuidStart = input.IndexOf(xuid);
            int usernameLength = (xuidStart - 1) - (usernameStart + playerConnected.Length);
            return (input.Substring(usernameStart + playerConnected.Length, usernameLength), input.Substring(xuidStart + xuid.Length, 16));
        }
    }
}