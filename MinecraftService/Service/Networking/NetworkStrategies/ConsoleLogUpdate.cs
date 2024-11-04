using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ConsoleLogUpdate : IMessageParser {

        private readonly MmsLogger _logger;
        private readonly IMinecraftService _service;
        private readonly ServiceConfigurator _serviceConfiguration;

        public ConsoleLogUpdate(MmsLogger logger, ServiceConfigurator serviceConfiguration, IMinecraftService service) {
            _service = service;
            _logger = logger;

            _serviceConfiguration = serviceConfiguration;
        }

        public (byte[] data, byte srvIndex, NetworkMessageTypes type) ParseMessage(byte[] data, byte serverIndex) {
            string stringData = Encoding.UTF8.GetString(data, 5, data.Length - 5);
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
                        srvText = _service.GetServerByName(srvName).GetLogger();
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
                    srvTextLen = _serviceConfiguration.GetLog().Count;
                    clientCurLen = int.Parse(dataSplit[1]);
                    loop = clientCurLen;
                    while (loop < srvTextLen) {
                        srvString.Append($"{srvName}|;|{_logger.FromIndex(loop)}|;|{loop}|?|");
                        loop++;
                    }
                }
            }
            if (srvString.Length > 3) {
                srvString.Remove(srvString.Length - 3, 3);
                return (Encoding.UTF8.GetBytes(srvString.ToString()), 0, NetworkMessageTypes.ConsoleLogUpdate);
            }
            return (Array.Empty<byte>(), 0, 0);
        }
    }
}

