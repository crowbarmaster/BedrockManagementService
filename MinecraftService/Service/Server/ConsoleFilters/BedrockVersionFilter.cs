using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class BedrockVersionFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public BedrockVersionFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration mineraftService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = mineraftService;
        }

        public void Filter(string input) {
            int msgStartIndex = input.IndexOf(']') + 2;
            string focusedMsg = input.Substring(msgStartIndex, input.Length - msgStartIndex);
            int versionIndex = focusedMsg.IndexOf(' ') + 1;
            string versionString = focusedMsg.Substring(versionIndex, focusedMsg.Length - versionIndex);
            if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerVersion).StringValue == "None") {
                _logger.AppendLine("Service detected version, restarting server to apply correct configuration.");

                _bedrockServer.ForceKillServer();
                _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerVersion).SetValue(versionString);
                _configurator.LoadServerConfigurations().Wait();
                _configurator.SaveServerConfiguration(_serverConfiguration);
                _bedrockServer.ServerStart().Wait();
            }
            if (versionString.ToLower().Contains("-beta")) {
                int betaTagLoc = versionString.ToLower().IndexOf("-beta");
                int betaVer = int.Parse(versionString.Substring(betaTagLoc + 5, versionString.Length - (betaTagLoc + 5)));
                versionString = versionString.Substring(0, betaTagLoc) + ".";
                versionString = versionString + betaVer;
            }
            if (_serverConfiguration.GetServerVersion() != versionString) {
                if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                    _logger.AppendLine($"Server {_serverConfiguration.GetServerName()} decected incorrect or out of date version! Replacing build...");
                    _bedrockServer.PerformOfflineServerTask(() => {
                        _bedrockServer.ForceKillServer();
                        _serverConfiguration.GetUpdater().ReplaceServerBuild().Wait();
                    });
                } else {
                    _logger.AppendLine($"Server {_serverConfiguration.GetServerName()} is out of date, Enable AutoDeployUpdates option to update to latest!");
                }
            }

        }
    }
}
