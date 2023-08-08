using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

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

        public static void KillJarProcess(string jarName) {
            try {
                Process[] processList = Process.GetProcessesByName(GetServiceFileName(BmsFileNameKeys.Jdk17JavaMmsExe)[..^4]);
                foreach (Process process in processList) {
                    process.Kill();
                }
            } catch { }
        }

        public static bool JarProcessExists(string jarName) {
            try {
                Process[] processList = Process.GetProcessesByName(GetServiceFileName(BmsFileNameKeys.Jdk17JavaMmsExe)[..^4]);
                if (processList.Length > 0) {
                    return true;
                }
            } catch { }
            return false;
        }

        public static Task QuickLaunchJar(string jarPath) {
            return Task.Run(() => {
                if (string.IsNullOrEmpty(jarPath)) {
                    return;
                }
                FileInfo jarInfo = new FileInfo(jarPath);
                string jarName = $"\"{jarInfo.FullName}\"";
                ProcessStartInfo processStartInfo = new ProcessStartInfo() {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = GetServiceFilePath(BmsFileNameKeys.Jdk17JavaMmsExe),
                    WorkingDirectory = jarInfo.Directory.FullName,
                    Arguments = $@"-Xmx1024M -Xms1024M -jar {jarInfo.Name} nogui"
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit(10000);
            });
        }
    }
}
