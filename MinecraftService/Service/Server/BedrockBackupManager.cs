﻿using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;
using System.IO.Compression;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Server
{
    public class BedrockBackupManager : IBackupManager {
        private readonly MmsLogger _logger;
        private readonly IServerController _server;
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IServerConfiguration _serverConfiguration;
        private bool _autoBackupsContainPacks = false;
        private bool _backupRunning = false;
        private const string _backupStringTemplate = "Backup-yyyyMMdd_HHmmssff.zip";

        public enum BackupType {
            Auto,
            Manual
        }

        public BedrockBackupManager(MmsLogger logger, IServerController server, IServerConfiguration serverConfiguration, ServiceConfigurator serviceConfiguration) {
            _logger = logger;
            _server = server;
            _serverConfiguration = serverConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _autoBackupsContainPacks = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoBackupsContainPacks).GetBoolValue();
        }

        public virtual bool BackupRunning() => _backupRunning;

        public virtual bool SetBackupComplete() => _backupRunning = false;

        public virtual void InitializeBackup() {
            if (!_backupRunning && _server.GetServerStatus().ServerStatus == ServerStatus.Started) {
                _backupRunning = true;
                _server.WriteToStandardIn("save hold");
                Task.Delay(1000).Wait();
                _server.WriteToStandardIn("say Server backup started.");
                _server.WriteToStandardIn("save query");
            }
        }

        public virtual bool PerformBackup(string queryString) {
            try {
                string serverPath = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
                string backupPath = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                string levelName = _serverConfiguration.GetProp(MmsDependServerPropKeys.LevelName).ToString();
                DirectoryInfo worldsDir = new($@"{serverPath}\worlds");
                DirectoryInfo backupDir = new($@"{backupPath}\{_serverConfiguration.GetServerName()}");
                Dictionary<string, int> backupFileInfoPairs = new();
                string[] files = queryString.Split(", ");
                foreach (string file in files) {
                    string[] fileInfoSplit = file.Split(':');
                    string fileName = fileInfoSplit[0];
                    int fileSize = int.Parse(fileInfoSplit[1]);
                    backupFileInfoPairs.Add(fileName, fileSize);
                }
                PruneBackups(backupDir);
                _logger.AppendLine($"Backing up files for server {_serverConfiguration.GetServerName()}. Please wait!");
                string levelDir = @$"\{_serverConfiguration.GetProp(MmsDependServerPropKeys.LevelName)}";
                using FileStream fs = File.Create($@"{backupDir.FullName}\Backup-{DateTime.Now:yyyyMMdd_HHmmssff}.zip");
                using ZipArchive backupZip = new(fs, ZipArchiveMode.Create);
                bool result = BackupWorldFilesFromQuery(backupFileInfoPairs, worldsDir.FullName, backupZip).Result;
                DirectoryInfo levelDirInfo = new(worldsDir.FullName + levelDir);
                List<FileInfo> levelFiles = levelDirInfo.GetFiles("*.json").ToList();
                levelFiles.AddRange(levelDirInfo.GetFiles("world_icon*"));
                int progressCallCount = levelFiles.Count / 6;
                int currentFileCount = 0;
                foreach (FileInfo levelFile in levelFiles) {
                    currentFileCount++;
                    try {
                        if (progressCallCount != 0 && currentFileCount % progressCallCount == 0) {
                            _logger.AppendLine($"Adding files to archive. {currentFileCount / 6}% complete...");
                        }
                    } catch {
                    }
                    backupZip.CreateEntryFromFile(levelFile.FullName, $"{_serverConfiguration.GetProp(MmsDependServerPropKeys.LevelName)}/{levelFile.Name}");
                }
                _server.WriteToStandardIn("save resume");
                _server.WriteToStandardIn("say Server backup complete.");
                if (_autoBackupsContainPacks) {
                    Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => _logger.AppendLine($"Adding packs to archive. {p.Progress}% complete..."));
                    FileUtilities.AppendServerPacksToArchive(serverPath, levelDirInfo.FullName, backupZip, progress);
                }
                _serviceConfiguration.CalculateTotalBackupsAllServers().Wait();
                return result;

            } catch (Exception e) {
                _logger.AppendLine($"Error with Backup: {e.Message} {e.StackTrace}");
                _server.WriteToStandardIn("save resume");
                _server.WriteToStandardIn("say Server backup failed! Contact admins!");
                return false;
            }
        }

        public virtual void PerformRollback(string zipFilePath) {
            try {
                DirectoryInfo worldsDir = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds");
                FileInfo backupZipFileInfo = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath)}\{_serverConfiguration.GetServerName()}\{zipFilePath}");
                DirectoryInfo backupPacksDir = new($@"{worldsDir.FullName}\{_serverConfiguration.GetProp(MmsDependServerPropKeys.LevelName)}\InstalledPacks");
                string currentMessage = $"Deleting server files for server {_server.GetServerName()}.";
                Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => {
                    _logger.AppendLine($"{currentMessage} {p.Progress}%");
                });
                FileUtilities.DeleteFilesFromDirectory(worldsDir, true, progress).Wait();
                _logger.AppendLine($"Deleted world folder \"{worldsDir.Name}\"");
                if (!backupPacksDir.Exists) {
                    backupPacksDir.Create();
                }
                currentMessage = $"Extracting backup files to server path.";
                ZipFile.ExtractToDirectory(backupZipFileInfo.FullName, worldsDir.FullName);
                _logger.AppendLine($"Copied files from backup \"{backupZipFileInfo.Name}\" to server worlds directory.");
                MinecraftPackParser parser = new(_logger, progress);
                foreach (FileInfo file in backupPacksDir.GetFiles()) {
                    string tempDir = SharedStringBase.GetNewTempDirectory("RollbackPacks");
                    ZipUtilities.ExtractToDirectory(file.FullName, tempDir, progress);
                    parser.FoundPacks.Clear();
                    parser.ParseDirectory(tempDir, 0);
                    if (parser.FoundPacks[0].ManifestType == "data") {
                        currentMessage = $"Clearing and extracting BP {parser.FoundPacks[0].FolderName}";
                        string folderPath = $@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds\{_serverConfiguration.GetProp(MmsDependServerPropKeys.LevelName)}\behavior_packs\{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}";
                        Task.Run(() => FileUtilities.DeleteFilesFromDirectory(folderPath, false, progress)).Wait();
                        ZipUtilities.ExtractToDirectory(file.FullName, folderPath, progress);
                    }
                    if (parser.FoundPacks[0].ManifestType == "resources") {
                        currentMessage = $"Clearing and extracting RP {parser.FoundPacks[0].FolderName}";
                        string folderPath = $@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\worlds\{_serverConfiguration.GetProp(MmsDependServerPropKeys.LevelName)}\53resource_packs\{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}";
                        Task.Run(() => FileUtilities.DeleteFilesFromDirectory(folderPath, false, progress)).Wait();
                        ZipUtilities.ExtractToDirectory(file.FullName, folderPath, progress);
                    }
                }
            } catch (Exception ex) {
                _logger.AppendLine($"Error with backup rollback: {ex.Message}");
            }
        }

        internal void PruneBackups(DirectoryInfo backupDir) {
            if (!backupDir.Exists) {
                backupDir.Create();
            }
            int fileCount = backupDir.GetFiles().Length;
            try {
                if (fileCount >= _serverConfiguration.GetSettingsProp(ServerPropertyKeys.MaxBackupCount).GetIntValue()) {
                    List<string> dates = new();
                    foreach (FileInfo file in backupDir.GetFiles()) {
                        string[] fileNameSplit = file.Name.Split('-');
                        dates.Add(fileNameSplit[1]);
                    }
                    dates.Sort();
                    while (fileCount >= _serverConfiguration.GetSettingsProp(ServerPropertyKeys.MaxBackupCount).GetIntValue()) {
                        File.Delete($@"{backupDir}\Backup-{dates.First()}");
                        _logger.AppendLine($"Removed Backup-{dates.First()}");
                        dates.Remove(dates.First());
                        fileCount--;
                    }
                }
            } catch (Exception e) {
                if (e.GetType() == typeof(FormatException)) {
                    _logger.AppendLine("Error in Config! MaxBackupCount must be nothing but a number!");
                }
            }
        }

        private Task<bool> BackupWorldFilesFromQuery(Dictionary<string, int> fileNameSizePairs, string worldPath, ZipArchive destinationArchive) {
            return Task.Run(() => {
                try {
                    foreach (KeyValuePair<string, int> file in fileNameSizePairs) {
                        string fileName = file.Key.Replace('/', '\\');
                        int fileSize = file.Value;
                        string filePath = $@"{worldPath}\{fileName}";
                        byte[]? fileData = null;
                        if (!File.Exists(filePath)) {
                            File.Create(filePath).Close();
                        }
                        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (MemoryStream ms = new()) {
                            fs.CopyTo(ms);
                            ms.Position = 0;
                            fileData = ms.ToArray();
                        }
                        ZipArchiveEntry newZippedFile = destinationArchive.CreateEntry(fileName, CompressionLevel.Optimal);
                        using Stream zipStream = newZippedFile.Open();
                        zipStream.Write(fileData, 0, fileSize);
                    }
                    return true;
                } catch (Exception ex) {
                    Console.WriteLine($"Error! {ex.Message}");
                }
                return false;
            });
        }
    }
}
