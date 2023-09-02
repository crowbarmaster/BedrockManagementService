using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class JavaBackupDetectFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _javaServer;
        IServiceConfiguration _serviceConfiguration;

        public JavaBackupDetectFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController javaServer, IServiceConfiguration mineraftService) {
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