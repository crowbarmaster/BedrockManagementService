
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    class DeleteBackups : IMessageParser {
        private readonly MmsLogger _logger;
        private readonly ServiceConfigurator _serviceConfigurator;

        public DeleteBackups(MmsLogger logger, ServiceConfigurator service) {
            _serviceConfigurator = service;
            _logger = logger;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
            FileInfo file = new FileInfo(stringData);
            try {
                _serviceConfigurator.DeleteBackupForServer(serverIndex, stringData);
                _logger.AppendLine($"Deleted backup {file.Name}.");
            } catch (Exception ex) {
                _logger.AppendLine(ex.Message);
            }
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}
