using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class BackupStringFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _minecraftServer;
        ServiceConfigurator _serviceConfiguration;

        public BackupStringFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController minecraftServer, ServiceConfigurator minecraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _minecraftServer = minecraftServer;
            _serviceConfiguration = minecraftService;
        }

        public void Filter(string input) {
            if (input.Contains("[Server]")) {
                input = input.Substring(input.IndexOf(']') + 2);
            }
            _logger.AppendLine($"Save data string {_serverConfiguration.GetServerName()} detected! Performing backup now!");
            if (_minecraftServer.GetBackupManager().PerformBackup(input)) {
                _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Completed.");
                if (_minecraftServer.GetActivePlayerList().Count == 0) {
                    _minecraftServer.SetServerModified(false);
                }
                return;
            }
            _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Failed. Check logs!");
        }
    }
}