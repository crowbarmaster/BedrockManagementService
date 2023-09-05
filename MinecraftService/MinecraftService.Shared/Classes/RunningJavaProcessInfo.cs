using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Classes {
    public class RunningJavaProcessInfo {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }

        public RunningJavaProcessInfo(string input) {
            if (!string.IsNullOrEmpty(input)) {
                string pid = input[..input.IndexOf(' ')];
                string name = input[input.IndexOf(' ')..];
                int outputNum;
                if (int.TryParse(pid, out outputNum)) {
                    ProcessId = outputNum;
                    ProcessName = name;
                }
            }
        }
    }
}
