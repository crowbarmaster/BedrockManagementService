using BedrockService.Service.Networking.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Networking.NetworkStrategies {
    public class ExportFileRequest : IMessageParser {
        private readonly IServiceConfiguration _configuration;
        public ExportFileRequest(IServiceConfiguration configuration) {
            _configuration = configuration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string jsonString = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            BackupInfoModel exportFileInfo = JsonConvert.DeserializeObject<BackupInfoModel>(jsonString);
            string fileName = exportFileInfo.Filename;
            if (serverIndex != 255) {
                IServerConfiguration server = _configuration.GetServerInfoByIndex(serverIndex);
                string backupPath = $"{server.GetSettingsProp("BackupPath")}\\{server.GetServerName()}\\{fileName}";
                byte[] fileBytes = File.ReadAllBytes(backupPath);
                return (fileBytes, (int)FileTypeFlags.Backup, NetworkMessageTypes.ExportFile);
            }
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}
