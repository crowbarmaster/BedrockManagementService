using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server.ConsoleFilters {
    public class LLVersionFilter : IConsoleFilter {
        IServerConfiguration _serverConfiguration;
        IServerLogger _logger;
        IConfigurator _configurator;
        IServerController _bedrockServer;
        IServiceConfiguration _serviceConfiguration;

        public LLVersionFilter(IServerLogger logger, IConfigurator configurator, IServerConfiguration serverConfiguration, IServerController bedrockServer, IServiceConfiguration mineraftService) {
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
                _serverConfiguration.SetServerVersion(versionString);
                _configurator.LoadServerConfigurations().Wait();
                _configurator.SaveServerConfiguration(_serverConfiguration);
                _bedrockServer.ServerStart().Wait();
            }
            if (!input.Contains("LiteLoaderBDS")) {
                _bedrockServer.PerformOfflineServerTask(() => _serverConfiguration.GetUpdater().ReplaceBuild(_serverConfiguration).Wait());
            } else {
                int llVerIndex = input.IndexOf("LiteLoaderBDS ") + 14;
                string llVer = input.Substring(llVerIndex);
                if (llVer.Contains('+')) {
                    llVer = llVer.Substring(0, llVer.IndexOf('+'));
                }
                if (llVer != _serviceConfiguration.GetLatestVersion(MinecraftServerArch.LiteLoader) && _serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                    _bedrockServer.PerformOfflineServerTask(() => _serverConfiguration.GetUpdater().ReplaceBuild(_serverConfiguration).Wait());
                }

            }
        }
    }
}
