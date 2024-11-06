
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
    public class ImportFileRequest(UserConfigManager configurator, ProcessInfo processInfo, ServiceConfigurator configuration) : IMessageParser {

        public Message ParseMessage(Message message) {
            ExportImportFileModel fileModel = JsonConvert.DeserializeObject<ExportImportFileModel>(Encoding.UTF8.GetString(message.Data));
            switch (fileModel.FileType) {
                case FileTypeFlags.ServerPackage:
                    MemoryStream ms = new(fileModel.Data);
                    ZipArchive zipArchive = new(ms);
                    ZipArchiveEntry serverConfFile = null;
                    ZipArchiveEntry serverBackup = null;
                    ZipArchiveEntry playerDbFile = null;
                    ZipArchiveEntry playerRegFile = null;

                    foreach (ZipArchiveEntry entry in zipArchive.Entries) {
                        if (entry.Name.Contains(".conf") && !entry.Name.Equals("Service.conf")) {
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
                        return new Message { Type = MessageTypes.Heartbeat };
                    }
                    break;
                case FileTypeFlags.ServicePackage:
                    return new Message { Type = MessageTypes.Heartbeat };
                    break;
            }
            return new Message { Type = MessageTypes.Heartbeat };
        }
    }
}