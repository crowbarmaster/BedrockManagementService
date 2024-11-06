using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class BackupRollback(MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            byte[] sendBytes = new byte[1];

            if (service.GetServerByIndex(message.ServerIndex).RollbackToBackup(stringData)) {
                sendBytes[0] = 1;
            } else {
                sendBytes[0] = 0;
            }
            return new(sendBytes, message.ServerIndex, MessageTypes.BackupCallback);
        }
    }
}

