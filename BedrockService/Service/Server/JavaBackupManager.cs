using BedrockService.Service.Server.Interfaces;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.SerializeModels;
using BedrockService.Shared.Utilities;
using System.IO.Compression;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Service.Server {
    public class JavaBackupManager : BedrockBackupManager {
        private readonly IServerLogger _logger;
        private readonly IServerController _server;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly FileUtilities FileUtilities;
        private bool _autoBackupsContainPacks = false;
        private bool _backupRunning = false;

        public enum BackupType {
            Auto,
            Manual
        }

        public JavaBackupManager(IServerLogger logger, IServerController server, IServerConfiguration serverConfiguration, IServiceConfiguration serviceConfiguration) : base(logger, server, serverConfiguration, serviceConfiguration) {
            _logger = logger;
            _server = server;
            _serverConfiguration = serverConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _autoBackupsContainPacks = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoBackupsContainPacks).GetBoolValue();
        }

        public override void InitializeBackup() {
            if (!_backupRunning && _server.GetServerStatus().ServerStatus == ServerStatus.Started) {
                _backupRunning = true;
                _server.WriteToStandardIn("say Server backup started.");
                _server.WriteToStandardIn("save-off");
                _server.WriteToStandardIn("save-all flush");
                Task.Delay(2000).Wait();
            }
        }

        public override bool BackupRunning() => _backupRunning;

        public override bool SetBackupComplete() => _backupRunning = false;

        public override bool PerformBackup(string unused) {
            try {
                string serverPath = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
                string backupPath = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath).ToString();
                string levelName = _serverConfiguration.GetProp(BmsDependServerPropKeys.LevelName).ToString();
                DirectoryInfo levelDir = new($@"{serverPath}\{levelName}");
                DirectoryInfo backupDir = new($@"{backupPath}\{_serverConfiguration.GetServerName()}");
                base.PruneBackups(backupDir);
                _logger.AppendLine($"Backing up files for server {_serverConfiguration.GetServerName()}. Please wait!");
                using FileStream fs = File.Create($@"{backupDir.FullName}\Backup-{DateTime.Now:yyyyMMdd_HHmmssff}.zip");
                using ZipArchive backupZip = new(fs, ZipArchiveMode.Create);
                AppendBackupToArchive(serverPath, levelDir, backupZip).Wait();
                _server.WriteToStandardIn("save-on");
                if (_autoBackupsContainPacks) {
                    FileUtilities.AppendServerPacksToArchive(serverPath, backupZip, levelDir);
                }
                _serviceConfiguration.CalculateTotalBackupsAllServers().Wait();
                return true;

            } catch (Exception e) {
                _logger.AppendLine($"Error with Backup: {e.Message} {e.StackTrace}");
                _server.WriteToStandardIn("save-on");
                _server.WriteToStandardIn($"say Server backup for {_serverConfiguration.GetServerName()} failed! Contact support!");
                return false;
            }
        }

        public override void PerformRollback(string zipFilePath) {
            string serverPath = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString();
            DirectoryInfo worldsDir = new($@"{serverPath}\{_serverConfiguration.GetProp(BmsDependServerPropKeys.LevelName)}");
            FileInfo backupZipFileInfo = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.BackupPath)}\{_serverConfiguration.GetServerName()}\{zipFilePath}");
            FileUtilities.DeleteFilesFromDirectory(worldsDir, true).Wait();
            _logger.AppendLine($"Deleted world folder \"{worldsDir.Name}\"");
            ZipFile.ExtractToDirectory(backupZipFileInfo.FullName, serverPath);
            _logger.AppendLine($"Copied files from backup \"{backupZipFileInfo.Name}\" to server worlds directory.");
        }

        private Task AppendBackupToArchive(string serverPath, DirectoryInfo currentDirectory, ZipArchive backupZip) =>
            Task.Run(() => {
                var fileList = currentDirectory.EnumerateFileSystemInfos();
                foreach (FileSystemInfo fsi in fileList) {
                    if (fsi.Extension == ".lock") {
                        continue;
                    }
                    if ((fsi.Attributes & FileAttributes.Directory) != FileAttributes.Directory) {
                        string archivePath = fsi.FullName.Substring(serverPath.Length + 1).Replace('\\', '/');
                        FileUtilities.AppendFileToArchive(fsi.FullName, archivePath, backupZip).Wait();
                    } else {
                        AppendBackupToArchive(serverPath, (DirectoryInfo)fsi, backupZip).Wait();
                    }
                }
            });
    }
}
