
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    class DeleteBackups(MmsLogger logger, ServiceConfigurator service) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data);
            FileInfo file = new FileInfo(stringData);   
            try {
                service.DeleteBackupForServer(message.ServerIndex, stringData);
                logger.AppendLine($"Deleted backup {file.Name}.");
            } catch (Exception ex) {
                logger.AppendLine(ex.Message);
            }
            return new();
        }
    }
}
