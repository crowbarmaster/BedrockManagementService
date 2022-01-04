using BedrockService.Shared.Classes;
using System.Collections.Generic;

namespace BedrockService.Shared.Interfaces {
    public interface IBedrockConfiguration {
        bool InitializeDefaults();
        void ProcessConfiguration(string[] configEntries);
        bool SetProp(Property propToSet);
        Property GetProp(string keyName);
        List<Property> GetAllProps();
        void SetAllProps(List<Property> newPropList);
        List<LogEntry> GetLog();
        void SetLog(List<LogEntry> newLog);
    }
}
