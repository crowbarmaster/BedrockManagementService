using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ConsoleLogUpdate(MmsLogger logger, ServiceConfigurator serviceConfiguration, MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            StringBuilder srvString = new();
            string[] split = stringData.Split("|?|");
            for (int i = 0; i < split.Length; i++) {
                string[] dataSplit = split[i].Split("|;|");
                string srvName = dataSplit[0];
                int srvTextLen;
                int clientCurLen;
                int loop;
                MmsLogger srvText;
                if (srvName != "Service") {
                    try {
                        srvText = service.GetServerByName(srvName).GetLogger();
                    } catch (NullReferenceException) {
                        break;
                    }
                    srvTextLen = srvText.Count();
                    clientCurLen = int.Parse(dataSplit[1]);
                    loop = clientCurLen;
                    while (loop < srvTextLen) {
                        srvString.Append($"{srvName}|;|{srvText.FromIndex(loop)}|;|{loop}|?|");
                        loop++;
                    }
                } else {
                    srvTextLen = serviceConfiguration.GetLog().Count;
                    clientCurLen = int.Parse(dataSplit[1]);
                    loop = clientCurLen;
                    while (loop < srvTextLen) {
                        srvString.Append($"{srvName}|;|{logger.FromIndex(loop)}|;|{loop}|?|");
                        loop++;
                    }
                }
            }
            if (srvString.Length > 3) {
                srvString.Remove(srvString.Length - 3, 3);
                return new(Encoding.UTF8.GetBytes(srvString.ToString()), 0, MessageTypes.ConsoleLogUpdate);
            }
            return new();
        }
    }
}

