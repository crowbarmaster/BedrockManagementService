
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies {
    public class ExportFileRequest : IMessageParser {
        private readonly IServiceConfiguration _configuration;
        private readonly IConfigurator _configurator;
        private readonly IServerLogger _logger;
        public ExportFileRequest(IConfigurator configurator, IServiceConfiguration configuration, IServerLogger logger) {
            _configuration = configuration;
            _configurator = configurator;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string jsonString = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            ExportImportFileModel exportFileInfo = JsonConvert.DeserializeObject<ExportImportFileModel>(jsonString);
            if (exportFileInfo == null) {
                return (null, 0, 0);
            }
            using MemoryStream ms = new();
            using ZipArchive packageFile = new(ms, ZipArchiveMode.Create);
            byte[]? exportData = null;
            if (serverIndex != 255) {
                IServerConfiguration server = _configuration.GetServerInfoByIndex(serverIndex);
                exportFileInfo.Filename = $"{exportFileInfo.FileType}-{server.GetServerName()}-{DateTime.Now:yyyyMMdd_hhmmssff}.zip";
                if (exportFileInfo.FileType == FileTypeFlags.Backup) {
                    string backupPath = $"{server.GetSettingsProp(ServerPropertyKeys.BackupPath)}\\{server.GetServerName()}\\{exportFileInfo.Filename}";
                    packageFile.CreateEntryFromFile(backupPath, exportFileInfo.Filename);
                }
                if (exportFileInfo.FileType == FileTypeFlags.ServerPackage) {
                    PrepareServerFiles(serverIndex, exportFileInfo, server, packageFile);
                }
            } else {
                exportFileInfo.Filename = $"{exportFileInfo.FileType}-Service-{DateTime.Now:yyyyMMdd_hhmmssff}.zip";
                if (exportFileInfo.FileType == FileTypeFlags.ServicePackage) {
                    packageFile.CreateEntryFromFile(GetServiceFilePath(MmsFileNameKeys.ServiceConfig), GetServiceFilePath(MmsFileNameKeys.ServiceConfig));
                }
            }
            ZipArchiveEntry manifestEntry = packageFile.CreateEntry("manifest.json");
            using Stream zipStream = manifestEntry.Open();
            using MemoryStream byteStream = new(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ExportImportManifestModel(exportFileInfo))));
            byteStream.CopyTo(zipStream);
            zipStream.Dispose();
            packageFile.Dispose();


            exportFileInfo.Data = ms.ToArray();
            exportData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(exportFileInfo));
            return (exportData, 0, NetworkMessageTypes.ExportFile);
        }

        private void PrepareServerFiles(byte serverIndex, ExportImportFileModel exportFileInfo, IServerConfiguration server, ZipArchive packageFile) {
            if (exportFileInfo.PackageFlags >= PackageFlags.ConfigFile) {
                packageFile.CreateEntryFromFile(GetServiceFilePath(MmsFileNameKeys.ServerConfig_Name, server.GetServerName()), GetServiceFileName(MmsFileNameKeys.ServerConfig_Name, server.GetServerName()));
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.LastBackup) {
                BackupInfoModel lastBackup = _configurator.EnumerateBackupsForServer(serverIndex).Result.FirstOrDefault();
                if (lastBackup != null) {
                    packageFile.CreateEntryFromFile($"{server.GetSettingsProp(ServerPropertyKeys.BackupPath)}\\{server.GetServerName()}\\{lastBackup.Filename}", lastBackup.Filename);
                }
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.WorldPacks) {
                Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => _logger.AppendLine($"Creating pack archive. {p.Progress}% completed..."));
                FileUtilities.AppendServerPacksToArchive(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), server.GetProp(MmsDependServerPropKeys.LevelName).ToString(), packageFile, progress);
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.PlayerDatabase) {
                if (File.Exists(GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName()))) {
                    packageFile.CreateEntryFromFile(GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName()), GetServiceFileName(MmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName()));
                }
                if (File.Exists(GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName()))) {
                    packageFile.CreateEntryFromFile(GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName()), GetServiceFileName(MmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName()));
                }
            }
        }
    }
}