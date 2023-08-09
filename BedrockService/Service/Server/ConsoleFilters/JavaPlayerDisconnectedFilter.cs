using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class JavaPlayerDisconnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public JavaPlayerDisconnectedFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} disconnected");
            _bedrockServer.GetActivePlayerList().Remove(_bedrockServer.GetPlayerManager().PlayerDisconnected(playerInfo.xuid));
            _configurator.SavePlayerDatabase(_serverConfiguration);
        }

        private (string username, string xuid) ExtractPlayerInfoFromString(string input) {
            int msgStartIndex = input.IndexOf("]:") + 3;
            int usernameEnd = input.IndexOf("joined", msgStartIndex) - 1;
            int usernameLength = usernameEnd - msgStartIndex;
            return (input.Substring(msgStartIndex, usernameLength), "");
        }
    }
}