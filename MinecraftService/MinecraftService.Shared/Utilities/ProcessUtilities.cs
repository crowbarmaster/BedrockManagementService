using MinecraftService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Utilities
{
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

        public static void KillJarProcess(string serverName) {
            try {
                List<RunningJavaProcessInfo> processList = GetJpsInfo().Result;
                foreach (RunningJavaProcessInfo process in processList) {
                    if (process.ProcessName.Contains($"MinecraftService_{serverName}")) {
                        Process jarProc = Process.GetProcessById(process.ProcessId);
                        if (jarProc != null) {
                            jarProc.Kill();
                        }
                    }
                }
            } catch { }
        }

        public static int JarProcessExists(string serverName) {
            try {
                List<RunningJavaProcessInfo> processList = GetJpsInfo().Result;
                foreach (RunningJavaProcessInfo process in processList) {
                    if (process.ProcessName.Contains($"MinecraftService_{serverName}")) {
                        return process.ProcessId;
                    }
                }
            } catch { }
            return 0;
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
                    FileName = GetServiceFilePath(MmsFileNameKeys.JavaMmsExe),
                    WorkingDirectory = jarInfo.Directory.FullName,
                    Arguments = $@"-Xmx1024M -Xms1024M -jar {jarInfo.Name} nogui"
                };
                Process process = Process.Start(processStartInfo);
                Stream propFile = null;
                int failCount = 0;
                Task.Delay(3000).Wait();
                while (!TryOpenFile($"{jarInfo.Directory.FullName}\\server.properties", out propFile)) {
                    Task.Delay(1000).Wait();
                    failCount++;
                    if(failCount > 15) {
                        process.Kill();
                        process.Dispose();
                        return;
                    }
                }
                propFile.Close();
                propFile.Dispose();
                process.Kill();
                process.Dispose();
            });
        }

        private static Process CreateProcess(string exePath) {
            ProcessStartInfo processStartInfo = new() {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = exePath
            };
            return new Process() { StartInfo = processStartInfo };
        }


        public static string BerockVersionFromExe(string exePath) {
            if (string.IsNullOrEmpty(exePath)) { throw new ArgumentNullException(nameof(exePath)); }
            Process serverProcess = CreateProcess(exePath);
            string versionOutput = "";
            if (serverProcess != null) {
                serverProcess.PriorityClass = ProcessPriorityClass.High;
                serverProcess.OutputDataReceived += (s, e) => {
                    if (e.Data != null) {
                        if (e.Data.Contains("Version")) {
                            int msgStartIndex = e.Data.IndexOf(']') + 2;
                            string focusedMsg = e.Data[msgStartIndex..];
                            int versionIndex = focusedMsg.IndexOf(' ') + 1;
                            versionOutput = focusedMsg[versionIndex..];
                            serverProcess.Kill();
                        }
                    }
                };
                serverProcess.BeginOutputReadLine();
                serverProcess.EnableRaisingEvents = false;
                serverProcess.WaitForExit(10000);
                if (string.IsNullOrEmpty(versionOutput)) {
                    throw new Exception("Attempted to quick-launch bedrock server at:\r\n{exepath}\r\nLaunch failed! Verify path!");
                }
            }
            return versionOutput;
        }

        static bool TryOpenFile(string path, out Stream stream) {
            try {
                FileStream fileStream = File.Open(path, FileMode.Open);
                if(fileStream.Length != 0) {
                    stream = fileStream;
                    return true;

                }
                fileStream.Close();
                stream = null;
                return false;
            } catch {
                stream = null;
                return false;
            }
        }


        public static Task<List<RunningJavaProcessInfo>> GetJpsInfo() => Task.Run(() => {
            ProcessStartInfo processStartInfo = new ProcessStartInfo() {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                WorkingDirectory = GetServiceDirectory(ServiceDirectoryKeys.Jdk21BinPath),
                FileName = @"Jps.exe",
                Arguments = $@"-mv"
            };
            Process process = Process.Start(processStartInfo);
            List<RunningJavaProcessInfo> output = new();
            process.OutputDataReceived += (s, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    output.Add(new(e.Data));
                }
            };
            process.BeginOutputReadLine();
            process.WaitForExit(1000);
            return output;
        });
    }
}
