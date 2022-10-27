using BedrockService.Shared.MinecraftFileModels.FileAccessModels;
using System.IO;
using Xunit;

namespace ServiceTests {
    public class JsonModelTests {

        [Fact]
        public void Can_Create_Pack_Manifest_File_Model() {
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
        public void Can_Create_Permission_File_Model() {
            string sampleJson = "[{\"permission\": \"member\",\"xuid\": \"1234567801234567\"}," +
                                "{\"permission\": \"operator\",\"xuid\": \"1234567890123456\"}," +
                                "{\"permission\": \"operator\",\"xuid\": \"2098765432112345\"}]";
            File.WriteAllText(".\\sample_permissions_model.json", sampleJson);
            PermissionsFileModel fileModel = new(".\\sample_permissions_model.json");
        }
    }
}
