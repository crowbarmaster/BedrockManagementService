using Newtonsoft.Json;

namespace BedrockService.Shared.Classes
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

        public override string ToString()
        {
            return Value;
        }

        public void SetValue(string newValue)
        {
            Value = newValue;
        }
    }
}
