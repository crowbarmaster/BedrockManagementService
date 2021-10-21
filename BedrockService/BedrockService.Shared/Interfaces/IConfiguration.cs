using BedrockService.Shared.Classes;
using System.Collections.Generic;

namespace BedrockService.Shared.Interfaces
{
    public interface IConfiguration
    {

        void InitializeDefaults();

        void ProcessConfiguration(string[] configEntries);

        bool SetProp(Property propToSet);

        Property GetProp(string keyName);

        List<Property> GetAllProps();

        void SetAllProps(List<Property> newPropList);

        List<string> GetLog();

        void SetLog(List<string> newLog);
    }
}
