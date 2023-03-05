using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BedrockService.Shared.JsonModels.LiteLoaderJsonModels {
    public class LiteLoaderVersionManifest {
        public string Version { get; set; }
        public string BDSVersion { get; set; }
        public string ProtocolVer { get; set; }
        public string IsBeta { get; set; }
        public string LLUrl { get; set; }
        public string ModuleUrl { get; set; }
    }
}
