using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.SerializeModels {
    public class ConsoleLogUpdateCallback {
        public byte LogTarget { get; set; } = 0xFE;
        public List<string> LogEntries { get; set; }
        public int CurrentCount { get; set; }

        public ConsoleLogUpdateCallback() {
            LogEntries = [];
        }
    }
}
