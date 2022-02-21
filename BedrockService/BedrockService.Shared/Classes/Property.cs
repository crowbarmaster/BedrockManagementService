using Newtonsoft.Json;
using System;

namespace BedrockService.Shared.Classes {
    public class Property {
        public string KeyName { get; set; }
        public string StringValue { get; set; }
        public string DefaultValue { get; set; }

        [JsonConstructor]
        public Property(string key, string defaultValue) {
            KeyName = key;
            StringValue = defaultValue;
            DefaultValue = defaultValue;
        }

        public override string ToString() {
            return StringValue;
        }

        public void SetValue(string newValue) {
            StringValue = newValue;
        }

        public void SetValue(int newValue) {
            StringValue = newValue.ToString();
        }

        public void SetValue(bool newValue) {
            StringValue = newValue.ToString();
        }

        public bool GetBoolValue() {
            try {
                if (bool.TryParse(StringValue, out bool result)) {
                    return result;
                }
            } catch (Exception e) {
                throw new FormatException($"Value for property {KeyName} tried to parse as a bool and failed! Check configs!", e);
            }
            return false;
        }

        public int GetIntValue() {
            try {
                if (int.TryParse(StringValue, out int result)) {
                    return result;
                }
            } catch (Exception e) {
                throw new FormatException($"Value for property {KeyName} tried to parse as a bool and failed! Check configs!", e);
            }
            return 0;
        }
    }
}
