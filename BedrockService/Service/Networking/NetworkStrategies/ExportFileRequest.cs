using BedrockService.Service.Management.Interfaces;
using BedrockService.Service.Networking.Interfaces;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class ExportFileRequest : IMessageParser {
        private readonly IServiceConfiguration _configuration;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        public ExportFileRequest(IConfigurator configurator, IProcessInfo processInfo, IServiceConfiguration configuration) {
            _configuration = configuration;
            _processInfo = processInfo;
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string jsonString = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            ExportFileModel exportFileInfo = JsonConvert.DeserializeObject<ExportFileModel>(jsonString);
            using MemoryStream ms = new();
            using ZipArchive packageFile = new(ms, ZipArchiveMode.Create);
            byte[]? exportData = null;
            if (serverIndex != 255 && exportFileInfo != null) {
                if (exportFileInfo.FileType == FileTypeFlags.Backup) {
                    IServerConfiguration server = _configuration.GetServerInfoByIndex(serverIndex);
                    string backupPath = $"{server.GetSettingsProp("BackupPath")}\\{server.GetServerName()}\\{exportFileInfo.Filename}";
                    exportFileInfo.Data = File.ReadAllBytes(backupPath);
                }
                if (exportFileInfo.FileType == FileTypeFlags.ServerPackage) {
                    IServerConfiguration server = _configuration.GetServerInfoByIndex(serverIndex);

                    PrepareServerFiles(serverIndex, exportFileInfo, server, packageFile);
                    packageFile.Dispose();
                    exportFileInfo.Data = ms.ToArray();
                }
                exportData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(exportFileInfo));
            }
            if (exportFileInfo.FileType == FileTypeFlags.ServicePackage) {
                packageFile.CreateEntryFromFile($"{_processInfo.GetDirectory()}\\Service.conf", "Service.conf");
            }
            return (exportData, 0, NetworkMessageTypes.ExportFile);
        }


        private void PrepareServerFiles(byte serverIndex, ExportFileModel exportFileInfo, IServerConfiguration server, ZipArchive packageFile) {
            if (exportFileInfo.PackageFlags >= PackageFlags.ConfigFile) {
                packageFile.CreateEntryFromFile($"{_processInfo.GetDirectory()}\\BMSConfig\\ServerConfigs\\{server.GetSettingsProp("FileName")}", server.GetSettingsProp("FileName").ToString());
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.LastBackup) {
                BackupInfoModel lastBackup = _configurator.EnumerateBackupsForServer(serverIndex).Result.FirstOrDefault();
                if (lastBackup != null) {
                    packageFile.CreateEntryFromFile($"{server.GetSettingsProp("BackupPath")}\\{server.GetServerName()}\\{lastBackup.Filename}", lastBackup.Filename);
                }
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.WorldPacks) {
                FileUtilities fileUtilities = new FileUtilities(_processInfo);
                fileUtilities.CreatePackBackupFiles(server.GetSettingsProp("ServerPath").ToString(), server.GetProp("level-name").ToString(), packageFile);
            }
            if (exportFileInfo.PackageFlags >= PackageFlags.PlayerDatabase) {
                if (File.Exists($"{_processInfo.GetDirectory()}\\BMSConfig\\ServerConfigs\\PlayerRecords\\{server.GetServerName()}.playerdb")) {
                    packageFile.CreateEntryFromFile($"{_processInfo.GetDirectory()}\\BMSConfig\\ServerConfigs\\PlayerRecords\\{server.GetServerName()}.playerdb", $"{server.GetServerName()}.playerdb");
                }
                if (File.Exists($"{_processInfo.GetDirectory()}\\BMSConfig\\ServerConfigs\\PlayerRecords\\{server.GetServerName()}.preg")) {
                    packageFile.CreateEntryFromFile($"{_processInfo.GetDirectory()}\\BMSConfig\\ServerConfigs\\PlayerRecords\\{server.GetServerName()}.preg", $"{server.GetServerName()}.preg");
                }
            }
        }
    }
}