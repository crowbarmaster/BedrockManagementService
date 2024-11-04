using MinecraftService.Shared.Interfaces;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Server.Updaters
{
    public class UpdaterContainer
    {
        private Dictionary<MinecraftServerArch, IUpdater> _updatersByArch = [];

        public UpdaterContainer() { }

        public IUpdater GetUpdaterByArch(MinecraftServerArch arch) => _updatersByArch[arch];

        public IUpdater GetUpdaterByArch(string archName) => _updatersByArch[GetArchFromString(archName)];

        public Dictionary<MinecraftServerArch, IUpdater> GetUpdaterTable() => _updatersByArch;

        public void SetUpdaterTable(Dictionary<MinecraftServerArch, IUpdater> updaters) => _updatersByArch = updaters;
    }
}
