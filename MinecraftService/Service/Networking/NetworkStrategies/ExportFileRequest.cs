
using Microsoft.AspNetCore.Hosting.Server;
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
using System;
using System.IO.Compression;
using System.Text;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ExportFileRequest(UserConfigManager configurator, MmsService minecraftService, ServiceConfigurator configuration, MmsLogger logger) : IMessageParser {

        public Message ParseMessage(Message message) {
            string jsonString = Encoding.UTF8.GetString(message.Data);
            ExportImportManifestModel requestManifest = JsonConvert.DeserializeObject<ExportImportManifestModel>(jsonString);
            if (requestManifest == null) {
                return new();
            }
            using MemoryStream ms = new();
            using ZipArchive packageFile = new(ms, ZipArchiveMode.Create);
            byte[]? exportData = null;
            ExportImportFileModel exportFileModel = new() {
                Manifest = requestManifest
            };
            if (requestManifest.FileType.HasFlag(FileTypes.ServerConfig)) {
                string serverName = minecraftService.GetServerByIndex(message.ServerIndex).GetServerName();
                string configFileName = GetServiceFileName(MmsFileNameKeys.ServerConfig_ServerName, serverName);
                PackFileContentsToZip(packageFile, configFileName, File.ReadAllBytes(GetServiceFilePath(MmsFileNameKeys.ServerConfig_ServerName, serverName)));
            }
            if (requestManifest.FileType.HasFlag(FileTypes.ServiceConfig)) {
                PackFileContentsToZip(packageFile, GetServiceFileName(MmsFileNameKeys.ServiceConfig), File.ReadAllBytes(GetServiceFilePath(MmsFileNameKeys.ServiceConfig)));
            }
            if (requestManifest.FileType.HasFlag(FileTypes.PlayerDB)) {
                IServerController server = minecraftService.GetServerByIndex(message.ServerIndex);
                if (configuration.GetProp(ServicePropertyKeys.GlobalizedPlayerDatabase).GetBoolValue()) {
                    PackFileContentsToZip(packageFile, GetServiceFilePath(MmsFileNameKeys.GlobalPlayerRegistry), File.ReadAllBytes(GetServiceFilePath(MmsFileNameKeys.GlobalPlayerRegistry)));
                    PackFileContentsToZip(packageFile, GetServiceFilePath(MmsFileNameKeys.GlobalPlayerTelem), File.ReadAllBytes(GetServiceFilePath(MmsFileNameKeys.GlobalPlayerTelem)));
                } else {
                    PackFileContentsToZip(packageFile, GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName()), File.ReadAllBytes(GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, server.GetServerName())));
                    PackFileContentsToZip(packageFile, GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName()), File.ReadAllBytes(GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, server.GetServerName())));
                }
            }
            if (requestManifest.FileType.HasFlag(FileTypes.WorldBackup)) {
                IServerConfiguration server = configuration.GetServerInfoByIndex(message.ServerIndex);
                BackupInfoModel lastBackup = configurator.EnumerateBackupsForServer(message.ServerIndex).Result.FirstOrDefault();
                PackFileContentsToZip(packageFile, lastBackup.Filename, File.ReadAllBytes($"{configuration.GetServerInfoByIndex(message.ServerIndex).GetSettingsProp(ServerPropertyKeys.BackupPath)}\\{lastBackup.Filename}"));
                if (requestManifest.FileType.HasFlag(FileTypes.ServerPacks)) {
                    Progress<ProgressModel> progress = new Progress<ProgressModel>((p) => logger.AppendLine($"Creating pack archive. {p.Progress}% completed..."));
                    FileUtilities.AppendServerPacksToArchive(server.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), server.GetProp(MmsDependServerPropKeys.LevelName).ToString(), packageFile, progress);
                }
            }
            PackFileContentsToZip(packageFile, "manifest.json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestManifest)));
            packageFile.Dispose();
            minecraftService.GetServerByIndex(message.ServerIndex).ServerStart();
            ExportImportFileModel fileModel = new ExportImportFileModel() {
                Manifest = requestManifest,
                Data = ms.ToArray(),
            };
            ms.Close();
            ms.Dispose();
            exportData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fileModel));
            return new() {
                Data = exportData,
                Type = MessageTypes.ExportFile
            };
        }

        private void PackFileContentsToZip(ZipArchive targetZip, string fileName, byte[] fileData) {
            ZipArchiveEntry manifestEntry = targetZip.CreateEntry(fileName);
            using Stream zipStream = manifestEntry.Open();
            using MemoryStream byteStream = new(fileData);
            byteStream.CopyTo(zipStream);
            zipStream?.Close();
            zipStream?.Dispose();
            byteStream?.Close();
            byteStream?.Dispose();
        }
    }
}