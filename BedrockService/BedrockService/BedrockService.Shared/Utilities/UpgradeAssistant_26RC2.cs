using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace BedrockService.Shared.Utilities {
    public static class UpgradeAssistant_26RC2 {
        private static List<KeyValuePair<string, string>> _serviceUpgradeList = new() {
            new KeyValuePair<string, string>(@"\Service\Globals.conf", @"\Service.conf"),
            new KeyValuePair<string, string>(@"\Server", @"\BmsConfig"),
            new KeyValuePair<string, string>(@"\BmsConfig\MCSFiles", @"\BmsConfig\BDSBuilds"),
            new KeyValuePair<string, string>(@"\BmsConfig\Configs", @"\BmsConfig\ServerConfigs"),
            new KeyValuePair<string, string>(@"\BmsConfig\ServerConfigs\RegisteredPlayers", @"\BmsConfig\ServerConfigs\PlayerRecords"),
            new KeyValuePair<string, string>(@"\BmsConfig\ServerConfigs\KnownPlayers", @"\BmsConfig\ServerConfigs\PlayerRecords"),
            new KeyValuePair<string, string>(@"\BmsConfig\Backups", @"\ServerBackups"),
        };

        private static List<string> _serviceDirectoryRemovalList = new() {
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
                    DirectoryInfo keyDir = new($"{basePath}\\{keyValue.Key}");
                    if (keyDir.Exists && !Directory.Exists($"{basePath}\\{keyValue.Value}")) {
                        Task.Run(() => {
                            keyDir.MoveTo($"{basePath}\\{keyValue.Value}");
                            Task.Delay(500).Wait();
                        }).Wait();
                    }
                } else {
                    FileInfo keyFile = new($"{basePath}\\{keyValue.Key}");
                    if (keyFile.Exists && !File.Exists($"{basePath}\\{keyValue.Value}")) {
                        Task.Run(() => {
                            keyFile.MoveTo($"{basePath}\\{keyValue.Value}");
                            Task.Delay(500).Wait();
                        }).Wait();
                    }
                }
            }
            DirectoryInfo buildsDirInfo = new($@"{basePath}\BmsConfig\BDSBuilds");
            IEnumerable<FileInfo> fileInfos = buildsDirInfo.GetFiles();
            Directory.CreateDirectory($"{buildsDirInfo.FullName}\\BuildArchives");
            Directory.CreateDirectory($"{buildsDirInfo.FullName}\\CoreFiles");
            foreach (FileInfo file in fileInfos) {
                file.MoveTo($"{buildsDirInfo.FullName}\\BuildArchives\\{file.Name}");
            }
            File.Delete($"{basePath}\\BMSConfig\\bedrock_ver.ini");
            File.Delete($"{basePath}\\BMSConfig\\stock_props.conf");
            File.Delete($"{basePath}\\BMSConfig\\stock_packs.json");
            CorrectFileTimeFormats(basePath);
            RemoveOldDirectories(basePath);
            UpdateServerConfigurations(basePath);
        }

        public static void PerformClientUpgrade(string basePath) {
            if (File.Exists($@"{basePath}\Client\Configs\Config.conf")) {
                File.Move($@"{basePath}\Client\Configs\Config.conf", $@"{basePath}\Client.conf");
            }
            CorrectLogTimeFormats(basePath);
            RemoveOldClientDirectory(basePath);
        }

        private static void CorrectFileTimeFormats(string basePath) {
            if (Directory.Exists($@"{basePath}\ServerBackups")) {
                DirectoryInfo dirInfo = new($@"{basePath}\ServerBackups");
                IEnumerable<DirectoryInfo> backupDirList = dirInfo.EnumerateDirectories();
                if (backupDirList.Count() > 0) {
                    foreach (DirectoryInfo dir in backupDirList) {
                        IEnumerable<DirectoryInfo> backupList = dir.EnumerateDirectories();
                        if (backupList.Count() > 0) {
                            foreach (DirectoryInfo backup in backupList) {
                                string dateInfo = backup.Name.Substring(7, backup.Name.Length - 7);
                                try {
                                    long dateLongValue = long.Parse(dateInfo);
                                    DateTime newDateObject = new(dateLongValue);
                                    string updatedName = $"Backup-{newDateObject:yyyyMMdd_HHmmssff}.zip";
                                    ZipFile.CreateFromDirectory(backup.FullName, $@"{dir.FullName}\{updatedName}");
                                    backup.Delete(true);
                                } catch {
                                    continue;
                                }
                            }

                        }
                    }
                }
            }
            CorrectLogTimeFormats(basePath);
        }

        private static void CorrectLogTimeFormats(string basePath) {
            if (Directory.Exists($@"{basePath}\Logs")) {
                DirectoryInfo logDirInfo = new($@"{basePath}\Logs");
                IEnumerable<DirectoryInfo> logDirList = logDirInfo.EnumerateDirectories();
                if (logDirList.Count() > 0) {
                    foreach (DirectoryInfo logDir in logDirList) {
                        DirectoryInfo logSubDirInfo = new(logDir.FullName);
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
                    string updatedName = $"{serverNameOverride}Log-{newDateObject:yyyyMMdd_HHmmssff}.log";
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

        private static void RemoveOldClientDirectory(string basePath) => Directory.Delete($@"{basePath}\Client", true);

        private static void UpdateServerConfigurations(string basePath) {
            string serviceConfPath = $@"{basePath}\Service.conf";
            string configPath = $@"{basePath}\BmsConfig\ServerConfigs";
            List<string> removeEntries = new();
            DirectoryInfo configDirInfo = new(configPath);
            List<FileInfo> serverConfigurations = configDirInfo.GetFiles("*.conf").ToList();
            List<string> serviceEntries = new(File.ReadAllLines(serviceConfPath));
            List<string> entriesToAdd = new();
            foreach (string entry in serviceEntries) {
                string[] splitEntry = entry.Split('=');
                switch (splitEntry[0]) {
                    case "BackupEnabled":
                    case "BackupPath":
                    case "BackupCron":
                    case "UpdateCron":
                    case "MaxBackupCount":
                    case "IgnoreInactiveBackups":
                        removeEntries.Add(entry);
                        entriesToAdd.Add(entry);
                        break;
                    case "CheckUpdates":
                        entriesToAdd.Add(entry);
                        removeEntries.Add(entry);
                        if (bool.TryParse(splitEntry[1], out bool result)) {
                            entriesToAdd.Add($"AutoDeployUpdates={result.ToString().ToLower()}");
                        }
                        break;
                    case "EntireBackups":
                        removeEntries.Add(entry);
                        break;
                }
            }
            foreach (string entry in removeEntries) {
                serviceEntries.Remove(entry);
            }
            File.WriteAllLines(serviceConfPath, serviceEntries.ToArray());
            entriesToAdd.AddRange(new string[] { "ServerAutostartEnabled=true", "AutoBackupsContainPacks=false", "SelectedServerVersion=Latest", "DeployedVersion=None" });
            entriesToAdd.Insert(0, "#Service");
            entriesToAdd.Insert(1, "#These entries were added as part of an automated update.");
            entriesToAdd.Insert(2, "#Pllease note the order may change!");
            foreach (FileInfo file in serverConfigurations) {
                List<string> confLines = new(entriesToAdd);
                confLines.AddRange(File.ReadAllLines(file.FullName));
                File.WriteAllLines(file.FullName, confLines.ToArray());
            }
        }
    }
}
