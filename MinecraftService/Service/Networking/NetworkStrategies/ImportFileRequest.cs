
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ImportFileRequest : IMessageParser {
        private readonly ServiceConfigurator _configuration;
        private readonly ProcessInfo _processInfo;
        private readonly UserConfigManager _configurator;
        public ImportFileRequest(UserConfigManager configurator, ProcessInfo processInfo, ServiceConfigurator configuration) {
            _configuration = configuration;
            _processInfo = processInfo;
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            ExportImportFileModel fileModel = JsonConvert.DeserializeObject<ExportImportFileModel>(Encoding.UTF8.GetString(data));
            switch (fileModel.FileType) {
                case FileTypeFlags.ServerPackage:
                    MemoryStream ms = new(fileModel.Data);
                    ZipArchive zipArchive = new(ms);
                    ZipArchiveEntry serverConfFile = null;
                    ZipArchiveEntry serverBackup = null;
                    ZipArchiveEntry playerDbFile = null;
                    ZipArchiveEntry playerRegFile = null;

                    foreach (ZipArchiveEntry entry in zipArchive.Entries) {
                        if (entry.Name.Contains(".conf") && !entry.Name.Contains("Service.conf")) {
                            serverConfFile = entry;
                        }
                        if (entry.Name.Contains("Backup-")) {
                            serverBackup = entry;
                        }
                        if (entry.Name.Contains(".playerdb")) {
                            playerDbFile = entry;
                        }
                        if (entry.Name.Contains(".preg")) {
                            playerRegFile = entry;
                        }
                    }
                    if (serverConfFile != null) {
                        // serverConfFile.ExtractToFile()
                        return (null, 0, NetworkMessageTypes.Heartbeat);
                    }
                    break;
                case FileTypeFlags.ServicePackage:
                    return (null, 0, NetworkMessageTypes.Heartbeat);
                    break;
            }
            return (null, 0, NetworkMessageTypes.Heartbeat);
        }
    }
}