﻿using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class PlayerConnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IBedrockLogger _logger;
        IConfigurator _configurator;
        IBedrockServer _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public PlayerConnectedFilter(IBedrockLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IBedrockServer bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} connected with XUID: {playerInfo.xuid}");
            _bedrockServer.SetServerModified(true);
            _bedrockServer.GetActivePlayerList().Add(_bedrockServer.GetPlayerManager().PlayerConnected(playerInfo.username, playerInfo.xuid));
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