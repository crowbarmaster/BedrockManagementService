using System;
using System.Diagnostics;
using System.Threading;

namespace BedrockService.Shared.Utilities {
    public static class ProcessUtilities {
        public static bool MonitoredAppExists(string monitoredAppName) {
            try {
                Process[] processList = Process.GetProcessesByName(monitoredAppName);
                if (processList.Length == 0) {
                    return false;
                } else {
                    return true;
                }
            } catch (Exception) {
                return true;
            }
        }

        public static bool MonitoredAppExists(int monitoredAppId) {
            try {
                Process process = Process.GetProcessById(monitoredAppId);
                if (process == null || process.HasExited) {
                    return false;
                } else {
                    return true;
                }
            } catch (InvalidOperationException) {
                return false;
            } catch (Exception) {
                return false;
            }
        }

        public static void KillProcessList(Process[] processList) {
            foreach (Process process in processList) {
                try {
                    process.Kill();
                    Thread.Sleep(1000);
                } catch (Exception) {
                }
            }
        }
    }
}
