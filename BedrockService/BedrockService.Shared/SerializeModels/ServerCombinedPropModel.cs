using MinecraftService.Shared.Classes;
using System.Collections.Generic;

namespace MinecraftService.Shared.SerializeModels {
    public class ServerCombinedPropModel {
        public List<Property> ServerPropList { get; set; }
        public List<Property> ServicePropList { get; set; }
        public List<SimpleVersionModel> VersionList { get; set; }
    }
}
