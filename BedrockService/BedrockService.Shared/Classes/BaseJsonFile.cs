using Newtonsoft.Json;
using System.IO;

namespace BedrockService.Shared.Classes {
    public abstract class BaseJsonFile {
        public string FilePath { get; set; }

        public BaseJsonFile(string fullPath) => FilePath = fullPath;

        public BaseJsonFile() { }

        public T LoadJsonFile<T>() {
            if (!File.Exists(FilePath)) {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath));
        }

        public void SaveToFile<T>(T value) => File.WriteAllText(FilePath, JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));
    }
}
