using MinecraftService.Service.Core;
using MinecraftService.Service.Networking.Interfaces;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using System.Text;

namespace MinecraftService.Service.Networking.NetworkStrategies
{
    public class ConsoleLogUpdate(MmsLogger logger, ServiceConfigurator serviceConfiguration, MmsService service) : IMessageParser {

        public Message ParseMessage(Message message) {
            string stringData = Encoding.UTF8.GetString(message.Data);
            List<ConsoleLogUpdateRequest> requests = JsonConvert.DeserializeObject<List<ConsoleLogUpdateRequest>>(stringData);
            List<ConsoleLogUpdateCallback> callbackList = [];
            int localCount = 0;
            List<LogEntry> log;
            foreach (ConsoleLogUpdateRequest request in requests) {
                ConsoleLogUpdateCallback callback = new ConsoleLogUpdateCallback();
                if (request.LogTarget == 0xFF) {
                    callback.LogTarget = 0xFF;
                    log = serviceConfiguration.GetLog();
                    localCount = log.Count;
                    while (request.CurrentCount < localCount) {
                        callback.LogEntries.Add(log[request.CurrentCount++].Text);
                    }
                    callback.CurrentCount = log.Count;
                    callbackList.Add(callback);
                    continue;
                }
                IServerConfiguration server = serviceConfiguration.GetServerInfoByIndex(request.LogTarget);
                log = server.GetLog();
                localCount = log.Count;
                callback.LogTarget = serviceConfiguration.GetServerIndex(server);
                while (request.CurrentCount < localCount) {
                    callback.LogEntries.Add(log[request.CurrentCount++].Text);
                }
                callback.CurrentCount = log.Count;
                callbackList.Add(callback);
            }
            string jsonData = JsonConvert.SerializeObject(callbackList);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
            return new(jsonBytes, 0, MessageTypes.ConsoleLogUpdate);
        }
    }
}

