using MinecraftService.Shared.Classes.Configurations;
using MinecraftService.Shared.Classes.Updaters;
using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes {
    public class EnumTypeLookup {
        private readonly IServerLogger _logger;
        private readonly IServiceConfiguration _service;
        public Dictionary<MinecraftServerArch, IUpdater> UpdatersByArch;

        public EnumTypeLookup(IServerLogger logger, IServiceConfiguration service) {
            _logger = logger;
            _service = service;
            UpdatersByArch = new Dictionary<MinecraftServerArch, IUpdater> {
                { MinecraftServerArch.Bedrock, new BedrockUpdater(_logger, _service) },
                { MinecraftServerArch.LiteLoader, new LiteUpdater(_logger, _service) },
                { MinecraftServerArch.Java, new JavaUpdater(_logger, _service) },
            };
        }

        public IUpdater GetUpdaterByArch (MinecraftServerArch arch) => UpdatersByArch[arch];

        public IUpdater GetUpdaterByArchName(string archName) => UpdatersByArch[GetArchFromString(archName)];

        public IServerConfiguration PrepareNewServerByArchName(string archName, IProcessInfo processInfo, IServerLogger logger, IServiceConfiguration service) {
            switch (archName) {
                case "Bedrock":
                    return new BedrockConfiguration(processInfo, logger, service);
                case "LiteLoader":
                    return new LiteLoaderConfiguration(processInfo, logger, service);
                case "Java":
                    return new JavaConfiguration(processInfo, logger, service);
            }
            return null;
        }
    }
}
