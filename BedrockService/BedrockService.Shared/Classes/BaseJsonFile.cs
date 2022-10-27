using Newtonsoft.Json;
using System.IO;

namespace BedrockService.Shared.Classes {
    public abstract class BaseJsonFile {
        private readonly JsonSerializerSettings defaultJsonSettings = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
        public string FilePath { get; set; }

        public BaseJsonFile(string fullPath) => FilePath = fullPath;

        public BaseJsonFile() { }

        public T LoadJsonFile<T>() {
            if (!File.Exists(FilePath)) {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath));
        }

        public void SaveToFile<T>(T value, JsonSerializerSettings settings = null) {
            if (settings == null) {
                settings = defaultJsonSettings;
            }
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(value, Formatting.Indented, settings));
        }
    }
}
