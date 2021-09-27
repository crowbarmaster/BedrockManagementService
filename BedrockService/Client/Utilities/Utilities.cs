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

        public static void SendJsonMsg<T>(object obj, NetworkMessageDestination destination, NetworkMessageTypes type)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonParser.Serialize(JsonParser.FromValue((T)obj)));
            FormManager.GetTCPClient.SendData(bytes, NetworkMessageSource.Client, destination, type);
        }

        public static void SendJsonMsgToSrv<T>(object obj, NetworkMessageDestination destination, byte serverIndex, NetworkMessageTypes type)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JsonParser.Serialize(JsonParser.FromValue((T)obj)));
            FormManager.GetTCPClient.SendData(bytes, NetworkMessageSource.Client, destination, serverIndex, type);
        }
    }
}
