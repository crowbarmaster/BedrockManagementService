using Newtonsoft.Json;

namespace BedrockService.Service.Server.HostInfoClasses
{
    public class Property
    {
        public string KeyName { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }

        [JsonConstructor]
        public Property(string key, string defaultValue)
        {
            KeyName = key;
            Value = defaultValue;
            DefaultValue = defaultValue;
        }
    }
}
