
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
    public class ImportFileRequest : IMessageParser {
        private readonly IServiceConfiguration _configuration;
        private readonly IProcessInfo _processInfo;
        private readonly IConfigurator _configurator;
        public ImportFileRequest(IConfigurator configurator, IProcessInfo processInfo, IServiceConfiguration configuration) {
            _configuration = configuration;
            _processInfo = processInfo;
            _configurator = configurator;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            ExportImportFileModel fileModel = JsonConvert.DeserializeObject<ExportImportFileModel>(Encoding.UTF8.GetString(data));
            switch (fileModel.FileType) {
                case FileTypeFlags.ServerPackage:
                    MemoryStream ms = new MemoryStream(fileModel.Data);
                    ZipArchive zipArchive = new ZipArchive(ms);
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