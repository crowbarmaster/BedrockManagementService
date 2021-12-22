using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public abstract class BaseJsonFile {
        public FileInfo FileInfo { get; set; }

        public BaseJsonFile(string fullPath) => FileInfo = new FileInfo(fullPath);

        public BaseJsonFile() { }

        public T LoadJsonFile<T>() {
            if (!FileInfo.Exists) {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(FileInfo.FullName));
        }

        public void SaveToFile<T>(T value) => File.WriteAllText(FileInfo.FullName, JsonConvert.SerializeObject(value, Formatting.Indented));
    }
}
