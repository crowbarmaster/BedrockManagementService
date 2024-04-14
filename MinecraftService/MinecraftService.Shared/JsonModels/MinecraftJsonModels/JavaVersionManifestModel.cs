using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.JsonModels.MinecraftJsonModels {
    public class JavaVersionManifestModel {
        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public string DownloadUrl { get; set; }
        public List<PropInfoEntry> PropList { get; set; } = new();
    }
}
