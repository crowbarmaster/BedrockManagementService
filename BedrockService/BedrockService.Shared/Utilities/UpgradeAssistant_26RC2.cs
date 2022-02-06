using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Utilities {
    public static class UpgradeAssistant_26RC2 {
        private static List<KeyValuePair<string, string>> _serviceUpgradeList = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string> (@"\Service\Globals.conf", @"\Service.conf"),
            new KeyValuePair<string, string> (@"\Server", @"\BmsConfig"),
            new KeyValuePair<string, string> (@"\BmsConfig\MCSFiles", @"\BmsConfig\BDSUpdates"),
            new KeyValuePair<string, string> (@"\BmsConfig\Configs", @"\BmsConfig\ServerConfigs"),
            new KeyValuePair<string, string> (@"\BmsConfig\ServerConfigs\RegisteredPlayers", @"\BmsConfig\ServerConfigs\PlayerRecords"),
            new KeyValuePair<string, string> (@"\BmsConfig\ServerConfigs\KnownPlayers", @"\BmsConfig\ServerConfigs\PlayerRecords"),
            new KeyValuePair<string, string> (@"\BmsConfig\Backups", @"\ServerBackups"),
        };

        private static List<string> _serviceDirectoryRemovalList = new List<string> {
            @"\BmsConfig\ServerConfigs\PlayerRecords\Backups",
            @"\BmsConfig\ServerConfigs\KnownPlayers",
            @"\Service",
            @"\Temp"
        };

        public static bool IsUpgradeRequired(string basePath) {
            if (File.Exists($@"{basePath}\Service\Globals.conf")) {
                return true;
            }
            return false;
        }

        public static bool IsClientUpgradeRequired(string basePath) {
            if (File.Exists($@"{basePath}\Client\Config.conf")) {
                return true;
            }
            return false;
        }

        public static void PerformUpgrade(string basePath) {
            foreach (KeyValuePair<string, string> keyValue in _serviceUpgradeList) {
                FileAttributes fileAttributes = File.GetAttributes($"{basePath}\\{keyValue.Key}");
                if (fileAttributes.HasFlag(FileAttributes.Directory)) {
                    DirectoryInfo keyDir = new DirectoryInfo($"{basePath}\\{keyValue.Key}");
                    if (keyDir.Exists && !Directory.Exists($"{basePath}\\{keyValue.Value}")) {
                        Task.Run(() => {
                            keyDir.MoveTo($"{basePath}\\{keyValue.Value}");
                            Task.Delay(500).Wait();
                        }).Wait();
                    }
                } else {
                    FileInfo keyFile = new FileInfo($"{basePath}\\{keyValue.Key}");
                    if (keyFile.Exists && !File.Exists($"{basePath}\\{keyValue.Value}")) {
                        Task.Run(() => {
                            keyFile.MoveTo($"{basePath}\\{keyValue.Value}");
                            Task.Delay(500).Wait();
                        }).Wait();
                    }
                }
            }
            CorrectFileTimeFormats(basePath);
            RemoveOldDirectories(basePath);
        }

        public static void PerformClientUpgrade(string basePath) {
            if (File.Exists($@"{basePath}\Client\Configs\Config.conf")) {
                File.Move($@"{basePath}\Client\Configs\Config.conf", $@"{basePath}\Client.conf");
            }
        }

        private static void CorrectFileTimeFormats(string basePath) {
            if (Directory.Exists($@"{basePath}\ServerBackups")) {
                DirectoryInfo dirInfo = new DirectoryInfo($@"{basePath}\ServerBackups");
                IEnumerable<DirectoryInfo> backupDirList = dirInfo.EnumerateDirectories();
                if (backupDirList.Count() > 0) {
                    foreach (DirectoryInfo dir in backupDirList) {
                        IEnumerable<DirectoryInfo> backupList = dir.EnumerateDirectories();
                        if (backupList.Count() > 0) {
                            DirectoryInfo backupFixTest = backupList.First();
                            if (long.TryParse(backupFixTest.Name.Substring(6, backupFixTest.Name.Length - 6), out _)) {
                                foreach (DirectoryInfo backup in backupList) {
                                    string dateInfo = backup.Name.Substring(6, backup.Name.Length - 6);
                                    long dateLongValue = long.Parse(dateInfo);
                                    DateTime newDateObject = new DateTime(dateLongValue);
                                    string updatedName = $"Backup_{newDateObject:yyyyMMdd_HHmmss}";
                                    backup.MoveTo($@"{basePath}\ServerBackups\{dir.Name}\{updatedName}");
                                }
                            }
                        }
                    }
                }
            }
            if (Directory.Exists($@"{basePath}\Logs")) {
                DirectoryInfo logDirInfo = new DirectoryInfo($@"{basePath}\Logs");
                IEnumerable<DirectoryInfo> logDirList = logDirInfo.EnumerateDirectories();
                if (logDirList.Count() > 0) {
                    foreach (DirectoryInfo logDir in logDirList) {
                        DirectoryInfo logSubDirInfo = new DirectoryInfo(logDir.FullName);
                        IEnumerable<FileInfo> logFileList = logSubDirInfo.EnumerateFiles();
                        if (logDir.Name == "Servers") {
                            IEnumerable<DirectoryInfo> serverLogDirList = logDir.EnumerateDirectories();
                            foreach (DirectoryInfo serverLogDir in serverLogDirList) {
                                IEnumerable<FileInfo> serverLogFileList = serverLogDir.EnumerateFiles();
                                ProcessDirectoryLogFiles(serverLogDir, serverLogFileList, true);
                            }
                            continue;
                        }
                        ProcessDirectoryLogFiles(logDir, logFileList);
                    }
                }
            }
        }

        private static void ProcessDirectoryLogFiles(DirectoryInfo logDir, IEnumerable<FileInfo> logFileList, bool isServer = false) {
            if (logFileList.Count() > 0) {
                foreach (FileInfo logFile in logFileList) {
                    int trimStart = logDir.Name.Length + 1;
                    int trimEnd = logFile.Name.Length - trimStart - 4;
                    string dateInfo = logFile.Name.Substring(trimStart, trimEnd);
                    DateTime newDateObject;
                    try {
                        newDateObject = DateTime.ParseExact(dateInfo, "MM-dd-yy_hh.mm.ss.ffff", CultureInfo.InvariantCulture);
                    } catch {
                        continue;
                    }
                    string serverNameOverride = isServer ? "Server" : logDir.Name;
                    string updatedName = $"{serverNameOverride}Log-{newDateObject:yyyyMMdd_HHmmss}.log";
                    logFile.MoveTo($@"{logFile.Directory.FullName}\{updatedName}");
                }
            }
        }

        private static void RemoveOldDirectories(string basePath) {
            foreach (string dir in _serviceDirectoryRemovalList) {
                if (Directory.Exists($@"{basePath}\{dir}")) {
                    Directory.Delete($@"{basePath}\{dir}", true);
                }
            }
            if (Directory.Exists(@$"{basePath}\BmsConfig\ServerConfigs\Backups") && Directory.EnumerateFileSystemEntries(@$"{basePath}\BmsConfig\ServerConfigs\Backups").Count() == 0) {
                Directory.Delete(@$"{basePath}\BmsConfig\ServerConfigs\Backups");
            }
        }
    }
}
