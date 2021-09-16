using Newtonsoft.Json.Linq;
using System;

namespace BedrockService.Utilities
{
    public class JsonParser
    {
        public Type Type { get; set; }
        public JToken Value { get; set; }

        public static JsonParser FromValue<T>(T value)
        {
            return new JsonParser { Type = typeof(T), Value = JToken.FromObject(value) };
        }

        public static string Serialize(JsonParser message)
        {
            return JToken.FromObject(message).ToString();
        }

        public static JsonParser Deserialize(string data)
        {
            return JToken.Parse(data).ToObject<JsonParser>();
        }
    }

}
