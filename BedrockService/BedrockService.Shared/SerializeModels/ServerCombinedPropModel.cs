using BedrockService.Shared.Classes;
using System.Collections.Generic;

namespace BedrockService.Shared.SerializeModels {
    public class ServerCombinedPropModel {
        public List<Property> ServerPropList { get; set; }
        public List<Property> ServicePropList { get; set; }
        public List<string> VersionList { get; set; }
    }
}
