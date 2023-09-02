using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Classes {
    public class SimpleVersionModel {
        public string Version { get; set; }
        public bool IsBeta { get; set; }

        public SimpleVersionModel(string version, bool isBeta) {
            Version = version;
            IsBeta = isBeta;
        }

        public override string ToString() {
            return Version;
        }
    }
}
