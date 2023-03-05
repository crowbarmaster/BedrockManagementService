using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server.ConsoleFilters {
    public class VersionFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IBedrockLogger _logger;
        IConfigurator _configurator;
        IBedrockServer _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public VersionFilter(IBedrockLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IBedrockServer bedrockServer, IServiceConfiguration bedrockService) {
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _configurator = configurator;
            _bedrockServer = bedrockServer;
            _serviceConfiguration = bedrockService;
        }

        public void Filter(string input) {
            int msgStartIndex = input.IndexOf(']') + 2;
            string focusedMsg = input.Substring(msgStartIndex, input.Length - msgStartIndex);
            int versionIndex = focusedMsg.IndexOf(' ') + 1;
            string versionString = focusedMsg.Substring(versionIndex, focusedMsg.Length - versionIndex);
            if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.DeployedVersion).StringValue == "None") {
                _logger.AppendLine("Service detected version, restarting server to apply correct configuration.");
                
                _bedrockServer.ForceKillServer();
                _serverConfiguration.GetSettingsProp(ServerPropertyKeys.DeployedVersion).SetValue(versionString);
                _serverConfiguration.ValidateVersion(versionString);
                _configurator.LoadServerConfigurations().Wait();
                _configurator.SaveServerConfiguration(_serverConfiguration);
                _bedrockServer.AwaitableServerStart().Wait();
            }
            string userSelectedBdsVersion = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.SelectedServerVersion).StringValue == "Latest"
            ? _serviceConfiguration.GetLatestBDSVersion()
                : _serverConfiguration.GetSettingsProp(ServerPropertyKeys.SelectedServerVersion).StringValue;
            string userSelectedLLVersion = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.SelectedLiteLoaderVersion).StringValue == "Latest"
            ? _serviceConfiguration.GetLatestLLVersion()
                : _serverConfiguration.GetSettingsProp(ServerPropertyKeys.SelectedLiteLoaderVersion).StringValue;
            if (versionString.ToLower().Contains("-beta")) {
                int betaTagLoc = versionString.ToLower().IndexOf("-beta");
                int betaVer = int.Parse(versionString.Substring(betaTagLoc + 5, versionString.Length - (betaTagLoc + 5)));
                versionString = versionString.Substring(0, betaTagLoc) + ".";
                versionString = versionString + betaVer;
            }
            if (!input.Contains("LiteLoaderBDS")) {
                if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.LiteLoaderEnabled).GetBoolValue() && !_bedrockServer.LiteLoadedServer()) {
                    _bedrockServer.AwaitableServerStop(false).Wait();
                    _configurator.ReplaceServerBuild(_serverConfiguration, userSelectedBdsVersion).Wait();
                    _bedrockServer.AwaitableServerStart();
                }
            } else {
                int llVerIndex = input.IndexOf("LiteLoaderBDS ") + 14;
                string llVer = input.Substring(llVerIndex);
                if (llVer.EndsWith('+')) {
                    llVer = llVer.Substring(0, llVer.Length - 1);
                }
                if (llVer != userSelectedLLVersion && _serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                    _bedrockServer.AwaitableServerStop(false).Wait();
                    _configurator.ReplaceServerBuild(_serverConfiguration, userSelectedBdsVersion).Wait();
                    _bedrockServer.AwaitableServerStart();
                }
            }
            if (userSelectedBdsVersion != versionString && !_serverConfiguration.GetLiteLoaderStatus()) {
                if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                    _logger.AppendLine($"Server {_serverConfiguration.GetServerName()} decected incorrect or out of date version! Replacing build...");
                    _bedrockServer.AwaitableServerStop(false).Wait();
                    _configurator.ReplaceServerBuild(_serverConfiguration, userSelectedBdsVersion).Wait();
                    _bedrockServer.AwaitableServerStart();
                } else {
                    _logger.AppendLine($"Server {_serverConfiguration.GetServerName()} is out of date, Enable AutoDeployUpdates option to update to latest!");
                }
            }

        }
    }
}
