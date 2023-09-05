using Newtonsoft.Json;
using System;

namespace MinecraftService.Shared.Classes {
    public class Property {
        public string KeyName { get; set; } = string.Empty;
        public string StringValue { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;

        [JsonConstructor]
        public Property(string key, string defaultValue) {
            KeyName = key;
            StringValue = defaultValue;
            DefaultValue = defaultValue;
        }

        public Property(Property newProp) {
            KeyName = newProp.KeyName;
            StringValue = newProp.StringValue;
            DefaultValue = newProp.DefaultValue;
        }

        public override string ToString() => StringValue;
        

        public string PropFileFormatString() => $"{KeyName}={StringValue}"; 
        

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

        public override bool Equals(object obj) {
            return obj is Property property &&
                   KeyName == property.KeyName &&
                   StringValue == property.StringValue &&
                   DefaultValue == property.DefaultValue;
        }

        public override int GetHashCode() {
            return HashCode.Combine(KeyName, StringValue, DefaultValue);
        }
    }
}
