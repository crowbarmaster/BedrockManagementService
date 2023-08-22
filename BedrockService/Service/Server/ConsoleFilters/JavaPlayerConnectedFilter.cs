using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class JavaPlayerConnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public JavaPlayerConnectedFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} connected to server {_serverConfiguration.GetServerName()}.");
            _bedrockServer.SetServerModified(true);
            _bedrockServer.GetActivePlayerList().Add(_bedrockServer.GetPlayerManager().PlayerConnected(playerInfo.username, playerInfo.uuid));
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }

        private (string username, string uuid) ExtractPlayerInfoFromString(string input) {
            int msgStartIndex = input.IndexOf("]:") + 3;
            int userStart = msgStartIndex + "UUID of player ".Length;
            int uuidLength = 36;
            int uuidStart = input.Length - uuidLength;
            int userEnd = uuidStart - 4;
            int userLength = userEnd - userStart;
            return (input.Substring(userStart, userLength), input.Substring(uuidStart, uuidLength));
        }
    }
}