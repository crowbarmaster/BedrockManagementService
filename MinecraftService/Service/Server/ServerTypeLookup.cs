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
        public static IServerController GetServerControllerByArch(MinecraftServerArch serverArch, IServerConfiguration server, IConfigurator configurator, IServerLogger logger, ServiceConfigurator service, IProcessInfo processInfo) {
            switch (serverArch) {
                case MinecraftServerArch.Bedrock:
                    return new BedrockServer(server, configurator, logger, service, processInfo);
                case MinecraftServerArch.LiteLoader:
                    return new LiteServer(server, configurator, logger, service, processInfo);
                case MinecraftServerArch.Java:
                    return new JavaServer(server, configurator, logger, service, processInfo);
            }
            return null;
        }
    }
}
