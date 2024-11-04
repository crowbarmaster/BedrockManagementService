using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Interfaces {
    public interface IBaseConfiguration {
        bool InitializeDefaults();
        void ProcessUserConfiguration(string[] configEntries);
        bool SetProp(Property propToSet);
        Property GetProp(string keyName);
        Property GetProp(ServicePropertyKeys key);
        Property GetSettingsProp(ServerPropertyKeys key);
        Property GetProp(MmsDependServerPropKeys key);
        void SetProp(MmsDependServerPropKeys key, string value);
        List<Property> GetAllProps();
        void SetAllProps(List<Property> newPropList);
        List<LogEntry> GetLog();
        void SetLog(List<LogEntry> newLog);
    }
}
