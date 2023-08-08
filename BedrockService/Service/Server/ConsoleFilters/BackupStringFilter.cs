using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class BackupStringFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public BackupStringFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            if (input.Contains("[Server]")) {
                input = input.Substring(input.IndexOf(']') + 2);
            }
            _logger.AppendLine("Save data string detected! Performing backup now!");
            if (_bedrockServer.GetBackupManager().PerformBackup(input)) {
                _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Completed.");
                if (_bedrockServer.GetActivePlayerList().Count == 0) {
                    _bedrockServer.SetServerModified(false);
                }
                return;
            }
            _logger.AppendLine($"Backup for server {_serverConfiguration.GetServerName()} Failed. Check logs!");
        }
    }
}