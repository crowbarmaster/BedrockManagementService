using Newtonsoft.Json;
using System.IO;

namespace MinecraftService.Shared.FileModels
{
    public abstract class BaseJsonFile
    {
        private readonly JsonSerializerSettings defaultJsonSettings = new() { DefaultValueHandling = DefaultValueHandling.Ignore };
        public string FilePath { get; set; }

        public BaseJsonFile(string fullPath) => FilePath = fullPath;

        public BaseJsonFile() { }

        public T LoadJsonFile<T>()
        {
            if (!File.Exists(FilePath))
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath));
        }

        public void SaveToFile<T>(T value, JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = defaultJsonSettings;
            }
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(value, Formatting.Indented, settings));
        }
    }
}
