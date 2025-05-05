
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ImportFileRequest(UserConfigManager configurator, MmsLogger logger, ServiceConfigurator serviceConfig) : IMessageParser {

        public Message ParseMessage(Message message) {
            ExportImportFileModel fileModel = JsonConvert.DeserializeObject<ExportImportFileModel>(Encoding.UTF8.GetString(message.Data));
            ExportImportManifestModel manifest = fileModel.Manifest;
            if (fileModel == null || manifest == null) return new();
            MemoryStream memStream = new(fileModel.Data);
            ZipArchive zip = new(memStream);

            if (manifest.FileType.HasFlag(FileTypes.BedrockWorld)) {

            }
            return message;
        }
    }
}