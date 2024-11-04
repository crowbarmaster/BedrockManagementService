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
    public class JavaBackupDetectFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        MmsLogger _logger;
        UserConfigManager _configurator;
        IServerController _javaServer;
        ServiceConfigurator _serviceConfiguration;

        public JavaBackupDetectFilter(MmsLogger logger, UserConfigManager configurator, IServerConfiguration serverConfiguration, IServerController javaServer, ServiceConfigurator mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _javaServer = javaServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            _logger.AppendLine($"Save data signal for server {_serverConfiguration.GetServerName()} detected! Performing backup now!");
            if (_javaServer.GetBackupManager().PerformBackup(input)) {
                _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Completed.");
                if (_javaServer.GetActivePlayerList().Count == 0) {
                    _javaServer.SetServerModified(false);
                }
                return;
            }
            _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Failed. Check logs!");
        }
    }
}