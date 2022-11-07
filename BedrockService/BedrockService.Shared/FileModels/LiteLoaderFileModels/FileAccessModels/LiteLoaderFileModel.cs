using BedrockService.Shared.Classes;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using System.Collections.Generic;

namespace BedrockService.Shared.FileModels.LiteLoaderFileModels.FileAccessModels {
    public class LiteLoaderFileModel : BaseJsonFile {
        public LiteLoaderConfigJsonModel Contents { get; set; } = new();

        public LiteLoaderFileModel(string fullPath) : base(fullPath) {
            Contents = LoadJsonFile<LiteLoaderConfigJsonModel>();
        }

        public LiteLoaderFileModel() { }

        public void SaveToFile() => SaveToFile(Contents, new Newtonsoft.Json.JsonSerializerSettings { DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include });
    }
}
