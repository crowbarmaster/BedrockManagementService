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
    public class JavaPlayerConnectedFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _javaServer;
        ServiceConfigurator _serviceConfiguration;

        public JavaPlayerConnectedFilter(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController javaServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _javaServer = javaServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            var playerInfo = ExtractPlayerInfoFromString(input);
            _logger.AppendLine($"Player {playerInfo.username} connected to server {_serverConfiguration.GetServerName()}.");
            _javaServer.SetServerModified(true);
            _javaServer.GetActivePlayerList().Add(_javaServer.GetPlayerManager().PlayerConnected(playerInfo.username, playerInfo.uuid));
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