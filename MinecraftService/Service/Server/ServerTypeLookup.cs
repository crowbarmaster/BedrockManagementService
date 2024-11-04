using MinecraftService.Service.Server.ServerControllers;
using MinecraftService.Shared.Classes.Server;
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

namespace MinecraftService.Shared.Classes
{
    public static class ServerTypeLookup {
        public static IServerController PrepareNewServerController(MinecraftServerArch serverArch, IServerConfiguration server, UserConfigManager configurator, MmsLogger logger, ServiceConfigurator service, ProcessInfo processInfo) {
            switch (serverArch) {
                case MinecraftServerArch.Bedrock:
                    return new BedrockServer(server, configurator, logger, service, processInfo);
                case MinecraftServerArch.Java:
                    return new JavaServer(server, configurator, logger, service, processInfo);
            }
            return null;
        }
    }
}
