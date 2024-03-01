using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Service.Server.ServerControllers;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Server {
    public static class ServerTypeLookup {
        public static IServerController GetServerControllerByArch(MinecraftServerArch serverArch, IServerConfiguration server, IConfigurator configurator, IServerLogger logger, IServiceConfiguration service, IProcessInfo processInfo, IPlayerManager playerManager) {
            switch (serverArch) {
                case MinecraftServerArch.Bedrock:
                    return new BedrockServer(server, configurator, logger, service, processInfo, playerManager);
                case MinecraftServerArch.LiteLoader:
                    return new LiteServer(server, configurator, logger, service, processInfo, playerManager);
                case MinecraftServerArch.Java:
                    return new JavaServer(server, configurator, logger, service, processInfo, playerManager);
            }
            return null;
        }
    }
}
