
using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Service.Server.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ExportFileRequest(UserConfigManager configurator, MmsService minecraftService, ServiceConfigurator configuration, MmsLogger logger) : IMessageParser {

        public Message ParseMessage(Message message) {
            string jsonString = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            ExportImportFileModel exportFileInfo = JsonConvert.DeserializeObject<ExportImportFileModel>(jsonString);
            if (exportFileInfo == null) {
                return new();
            }
            using MemoryStream ms = new();
            using ZipArchive packageFile = new(ms, ZipArchiveMode.Create);
            byte[]? exportData = null;
            if (message.ServerIndex != 255) {
                IServerConfiguration server = configuration.GetServerInfoByIndex(message.ServerIndex);
                minecraftService.GetServerByIndex(message.ServerIndex).ServerStop(false).Wait();
                exportFileInfo.Filename = $"{exportFileInfo.FileType}-{server.GetServerName()}-{DateTime.Now:yyyyMMdd_hhmmssff}.zip";
                if (exportFileInfo.FileType == FileTypeFlags.Backup) {
                    string backupPath = $"{server.GetSettingsProp(ServerPropertyKeys.BackupPath)}\\{server.GetServerName()}\\{exportFileInfo.Filename}";
                    packageFile.CreateEntryFromFile(backupPath, exportFileInfo.Filename);
                }
                if (exportFileInfo.FileType == FileTypeFlags.ServerPackage) {
                    PrepareServerFiles(message.ServerIndex, exportFileInfo, server, packageFile);
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

            minecraftService.GetServerByIndex(message.ServerIndex).ServerStart();
            exportFileInfo.Data = ms.ToArray();
            exportData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(exportFileInfo));
            return new(exportData, 0, MessageTypes.ExportFile);
        }

        private void PrepareServerFiles(byte serverIndex, ExportImportFileModel exportFileInfo, IServerConfiguration server, ZipArchive packageFile) {
            if (exportFileInfo.PackageFlags >= PackageFlags.ConfigFile) {
                packageFile.CreateEntryFromFile(GetServiceFilePath(MmsFileNameKeys.ServerConfig_Name, server.GetServerName()), GetServiceFileName(MmsFileNameKeys.ServerConfig_Name, server.GetServerName()));
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.LastBackup) {
                BackupInfoModel lastBackup = configurator.EnumerateBackupsForServer(serverIndex).Result.FirstOrDefault();
                if (lastBackup != null) {
                    packageFile.CreateEntryFromFile($"{server.GetSettingsProp(ServerPropertyKeys.BackupPath)}\\{server.GetServerName()}\\{lastBackup.Filename}", lastBackup.Filename);
                }
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.WorldPacks) {
                Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => logger.AppendLine($"Creating pack archive. {p.Progress}% completed..."));
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
            if(exportFileInfo.PackageFlags == PackageFlags.Full) {
                string zipPath = $@"{SharedStringBase.GetNewTempDirectory("ServerPackage")}\ServerPackage.zip";
                Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => logger.AppendLine($"Creating server archive. {p.Progress}% completed..."));
                ZipUtilities.CreateFromDirectory(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), zipPath, progress).Wait();
                packageFile.CreateEntryFromFile(zipPath, "ServerPackage.zip");
            }
        }
    }
}