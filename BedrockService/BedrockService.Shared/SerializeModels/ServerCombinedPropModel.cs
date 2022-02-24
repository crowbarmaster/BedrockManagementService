using BedrockService.Shared.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.SerializeModels {
    public class ServerCombinedPropModel {
        public List<Property> ServerPropList { get; set; }
        public List<Property> ServicePropList { get; set; }
    }
}
