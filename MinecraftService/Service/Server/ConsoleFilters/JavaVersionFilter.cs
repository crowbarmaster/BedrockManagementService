using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class JavaVersionFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _javaServer;
        IServiceConfiguration _serviceConfiguration;

        public JavaVersionFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController javaServer, IServiceConfiguration mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _javaServer = javaServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            int msgStartIndex = input.IndexOf("version ") + 8;
            string versionString = input.Substring(msgStartIndex, input.Length - msgStartIndex);
            if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerVersion).StringValue == "None") {
                _logger.AppendLine("Service detected version, restarting server to apply correct configuration.");

                _javaServer.ForceKillServer();
                _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerVersion).SetValue(versionString);
                _configurator.LoadServerConfigurations().Wait();
                _configurator.SaveServerConfiguration(_serverConfiguration);
                _javaServer.ServerStart().Wait();
            }
            if (_serverConfiguration.GetServerVersion() != versionString) {
                if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                    _logger.AppendLine($"Server {_serverConfiguration.GetServerName()} decected incorrect or out of date version! Replacing build...");
                    _javaServer.ServerStop(false).Wait();
                    _serverConfiguration.GetUpdater().ReplaceServerBuild().Wait();
                    _javaServer.ServerStart();
                } else {
                    _logger.AppendLine($"Server {_serverConfiguration.GetServerName()} is out of date, Enable AutoDeployUpdates option to update to latest!");
                }
            }

        }
    }
}
