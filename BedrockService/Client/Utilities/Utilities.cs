using BedrockService.Client.Management;
using BedrockService.Service.Networking;
using BedrockService.Utilities;
using System.Text;

namespace BedrockService.Client.Utilities
{
    public static class JsonUtilities
    {
        public static string GetJsonString<T>(object obj)
        {
            return JsonParser.Serialize(JsonParser.FromValue((T)obj));
        }

        public static bool SendJsonMsg<T>(object obj, NetworkMessageDestination destination, NetworkMessageTypes type)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonParser.Serialize(JsonParser.FromValue((T)obj)));
            return FormManager.GetTCPClient.SendData(bytes, NetworkMessageSource.Client, destination, type);
        }

        public static bool SendJsonMsgToSrv<T>(string serverName, object obj, NetworkMessageDestination destination, NetworkMessageTypes type)
        {
            byte[] bytes = Encoding.UTF8.GetBytes($"{serverName};{JsonParser.Serialize(JsonParser.FromValue((T)obj))}");
            return FormManager.GetTCPClient.SendData(bytes, NetworkMessageSource.Client, destination, type);
        }
    }
}
