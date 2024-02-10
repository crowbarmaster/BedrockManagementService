using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using System.IO;
using Xunit;

namespace MMS_Tests.SharedTests
{
    public class JsonModelTests
    {

        [Fact]
        public void Can_Create_Pack_Manifest_File_Model()
        {
            string sampleJson = "{\"format_version\": 1, \"header\": { \"name\": \"Miniguns and Landmines (behavior)\"," +
                "\"description\": \"Miniguns and Landmines. Oh ho ho.\", \"uuid\": \"9edcd542-0ce1-4a00-b28d-c6d0be1fa0dc\"," +
                "\"version\": [2, 6, 31] }, \"modules\": [ { \"description\": \"Mech Suit\", \"type\": \"data\", \"uuid\":" +
                "\"5a159687-5d39-4449-b6a3-c2514a1962f3\", \"version\": [2, 6, 31] } ], \"dependencies\": [ { \"uuid\":" +
                "\"0a4ea708-dedc-4185-b1c1-398a9cea8a32\", \"version\": [2, 6, 31] }]}";
            File.WriteAllText(".\\sample_pack_model.json", sampleJson);
            PackManifestFileModel fileModel = new(".\\sample_pack_model.json");
            Assert.NotNull(fileModel.Contents);
        }

        [Fact]
        public void Can_Create_Permission_File_Model()
        {
            string sampleJson = "[{\"permission\": \"member\",\"xuid\": \"1234567801234567\"}," +
                                "{\"permission\": \"operator\",\"xuid\": \"1234567890123456\"}," +
                                "{\"permission\": \"operator\",\"xuid\": \"2098765432112345\"}]";
            File.WriteAllText(".\\sample_permissions_model.json", sampleJson);
            PermissionsFileModel fileModel = new(".\\sample_permissions_model.json");
            Assert.NotNull(fileModel.Contents);
        }

        [Fact]
        public void Can_Create_LiteLoader_Plugin_Json()
        {
            LLServerPluginRegistry reg = new();
            reg.ServerPluginList = new() {
                new() { MmsServerName = "TestServer" }
            };
            reg.ServerPluginList[0].InstalledPlugins = new() {
                new() { BedrockVersion = "1.19.41.01", LiteLoaderVersion = "2.8.1", PluginFileName = "TestPlugin.dll" }
            };
            File.WriteAllText("LiteLoaderPluginDatabase.json", System.Text.Json.JsonSerializer.Serialize(reg, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }));
        }

        [Fact]
        public void Can_Create_Plugin_Repo_Json()
        {
            LLPluginRepoJsonModel model = new();
            model.PluginRepo.Add(new()
            {
                Name = "CrowbarTools",
                Description = "A toolbox of commands for LLBDS",
                PluginVersion = "1.0",
                ProtoVersion = 557,
                ExtendedInfo = "Currently contains comands for inventory manipulation, multiple commands truncated to one, and tele2server control.",
                ProtoBlacklist = new int[1],
                RepoURL = "http://127.0.0.1/bms_files/CrowbarTools.zip"
            });
            File.WriteAllText("LiteLoaderPluginRepo.json", System.Text.Json.JsonSerializer.Serialize(model, new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
