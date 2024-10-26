using MinecraftService.Shared.Classes;
using System.Collections.Generic;

namespace MinecraftService.Shared.Interfaces
{
    public interface IEnumTypeLookup
    {
        Dictionary<SharedStringBase.MinecraftServerArch, IUpdater> GetAllUpdaters();
        IUpdater GetUpdaterByArch(SharedStringBase.MinecraftServerArch arch);
        IUpdater GetUpdaterByArch(string archName);
        IServerConfiguration PrepareNewServerByArch(SharedStringBase.MinecraftServerArch archType, IProcessInfo processInfo, IServerLogger logger, ServiceConfigurator service);
        IServerConfiguration PrepareNewServerByArch(string archName, IProcessInfo processInfo, IServerLogger logger, ServiceConfigurator service);
    }
}