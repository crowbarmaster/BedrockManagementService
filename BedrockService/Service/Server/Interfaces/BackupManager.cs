using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Server {
    public class BackupManager {
        private readonly IProcessInfo _processInfo;
        private readonly IBedrockLogger _logger;
        private readonly IBedrockServer _server;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly FileUtilities _fileUtils;
        private bool _autoBackupsContainPacks = false;
        private bool _autoBackupsEnabled = false;
        private int _autoMaxBackupCount = 25;
        private string _autoBackupCronString = "* 1 * * *";

        public enum BackupType {
            Auto,
            Manual
        }

        public BackupManager(IProcessInfo processInfo, IBedrockLogger logger, IBedrockServer server, IServerConfiguration serverConfiguration, IServiceConfiguration serviceConfiguration) {
            _processInfo = processInfo;
            _logger = logger;
            _server = server;
            _serverConfiguration = serverConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _fileUtils = new FileUtilities(_processInfo);
            _autoBackupsContainPacks = _serverConfiguration.GetSettingsProp("AutoBackupsContainPacks").GetBoolValue();
        }

        public bool PerformBackup(string queryString) {
            try {
                string serverPath = _serverConfiguration.GetSettingsProp("ServerPath").ToString();
                string backupPath = _serverConfiguration.GetSettingsProp("BackupPath").ToString();
                string levelName = _serverConfiguration.GetProp("level-name").ToString();
                DirectoryInfo worldsDir = new DirectoryInfo($@"{serverPath}\worlds");
                DirectoryInfo backupDir = new DirectoryInfo($@"{backupPath}\{_serverConfiguration.GetServerName()}");
                Dictionary<string, int> backupFileInfoPairs = new Dictionary<string, int>();
                string[] files = queryString.Split(", ");
                foreach (string file in files) {
                    string[] fileInfoSplit = file.Split(':');
                    string fileName = fileInfoSplit[0];
                    int fileSize = int.Parse(fileInfoSplit[1]);
                    backupFileInfoPairs.Add(fileName, fileSize);
                }
                PruneBackups(backupDir);
                _logger.AppendLine($"Backing up files for server {_serverConfiguration.GetServerName()}. Please wait!");
                string levelDir = @$"\{_serverConfiguration.GetProp("level-name")}";
                using FileStream fs = File.Create($@"{backupDir.FullName}\Backup-{DateTime.Now:yyyyMMdd_HHmmssff}.zip");
                using ZipArchive backupZip = new ZipArchive(fs, ZipArchiveMode.Create);
                bool resuilt = BackupWorldFilesFromQuery(backupFileInfoPairs, worldsDir.FullName, backupZip).Result;
                DirectoryInfo levelDirInfo = new DirectoryInfo(worldsDir.FullName + levelDir);
                List<FileInfo> levelFiles = levelDirInfo.GetFiles("*.json").ToList();
                levelFiles.AddRange(levelDirInfo.GetFiles("world_icon*"));
                foreach (FileInfo levelFile in levelFiles) {
                    backupZip.CreateEntryFromFile(levelFile.FullName, $"{_serverConfiguration.GetProp("level-name")}/{levelFile.Name}");
                }
                _server.WriteToStandardIn("save resume");
                _server.WriteToStandardIn("say Server backup complete.");
                if (_autoBackupsContainPacks) {
                    CreatePackBackupFiles(serverPath, levelName, backupZip);
                    if (Directory.Exists(levelDirInfo.FullName + "\\resource_packs")) {
                        _fileUtils.ClearTempDir().Wait();
                        ZipFile.CreateFromDirectory(levelDirInfo.FullName + "\\resource_packs", $@"{Path.GetTempPath()}\BMSTemp\resource_packs.zip");
                        backupZip.CreateEntryFromFile($@"{Path.GetTempPath()}\BMSTemp\resource_packs.zip", "resource_packs.zip");
                    }
                    if (Directory.Exists(levelDirInfo.FullName + "\\behavior_packs")) {
                        _fileUtils.ClearTempDir().Wait();
                        ZipFile.CreateFromDirectory(levelDirInfo.FullName + "\\behavior_packs", $@"{Path.GetTempPath()}\BMSTemp\behavior_packs.zip");
                        backupZip.CreateEntryFromFile($@"{Path.GetTempPath()}\BMSTemp\behavior_packs.zip", "behavior_packs.zip");
                    }
                }
                _serviceConfiguration.CalculateTotalBackupsAllServers().Wait();
                return resuilt;

            } catch (Exception e) {
                _logger.AppendLine($"Error with Backup: {e.Message} {e.StackTrace}");
                _server.WriteToStandardIn("save resume");
                _server.WriteToStandardIn("say Server backup failed! Contact admins!");
                return false;
            }
        }

        private void PruneBackups(DirectoryInfo backupDir) {
            if (!backupDir.Exists) {
                backupDir.Create();
            }
            int dirCount = backupDir.GetDirectories().Length;
            try {
                if (dirCount >= _serverConfiguration.GetSettingsProp("MaxBackupCount").GetIntValue()) {
                    List<string> dates = new List<string>();
                    foreach (DirectoryInfo dir in backupDir.GetDirectories()) {
                        string[] folderNameSplit = dir.Name.Split('-');
                        dates.Add(folderNameSplit[1]);
                    }
                    dates.Sort();
                    Directory.Delete($@"{backupDir}\Backup-{dates.First()}", true);
                }
            } catch (Exception e) {
                if (e.GetType() == typeof(FormatException)) {
                    _logger.AppendLine("Error in Config! MaxBackupCount must be nothing but a number!");
                }
            }
        }

        private void CreatePackBackupFiles(string serverPath, string levelName, ZipArchive destinationArchive) {
            string resouceFolderPath = $@"{serverPath}\resource_packs";
            string behaviorFolderPath = $@"{serverPath}\behavior_packs";
            string behaviorFilePath = $@"{serverPath}\worlds\{levelName}\world_behavior_packs.json";
            string resoruceFilePath = $@"{serverPath}\worlds\{levelName}\world_resource_packs.json";
            MinecraftPackParser packParser = new(_processInfo);
            packParser.ParseDirectory(resouceFolderPath);
            packParser.ParseDirectory(behaviorFolderPath);
            WorldPackFileModel worldPacks = new(resoruceFilePath);
            worldPacks.Contents.AddRange(new WorldPackFileModel(behaviorFilePath).Contents);
            _fileUtils.ClearTempDir().Wait();
            string packBackupFolderPath = $@"{Path.GetTempPath()}\BMSTemp\InstalledPacks";
            Directory.CreateDirectory(packBackupFolderPath);
            if (worldPacks.Contents.Count > 0) {
                foreach (WorldPackEntryJsonModel model in worldPacks.Contents) {
                    MinecraftPackContainer container = packParser.FoundPacks.FirstOrDefault(x => x.JsonManifest.header.uuid == model.pack_id);
                    if (container == null) {
                        continue;
                    }
                    string packLocation = container.PackContentLocation;
                    ZipFile.CreateFromDirectory(packLocation, $@"{packBackupFolderPath}\{packLocation[packLocation.LastIndexOf('\\')..]}.zip");
                    destinationArchive.CreateEntryFromFile($@"{packBackupFolderPath}\{packLocation[packLocation.LastIndexOf('\\')..]}.zip", $@"InstalledPacks/{packLocation[packLocation.LastIndexOf('\\')..]}.zip");
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
                        byte[] fileData = null;
                        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (MemoryStream ms = new MemoryStream()) {
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
