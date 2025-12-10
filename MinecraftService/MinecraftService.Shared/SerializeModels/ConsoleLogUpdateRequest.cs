using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.SerializeModels {
    public class ConsoleLogUpdateRequest {
        public byte LogTarget { get; set; } = 0xFE;
        public int CurrentCount { get; set; }

        public ConsoleLogUpdateRequest(byte logTarget, int currentCount) {
            LogTarget = logTarget;
            CurrentCount = currentCount;
        }   
    }
}
