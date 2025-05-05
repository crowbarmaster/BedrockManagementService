using MinecraftService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MMS_Tests.SharedTests {
    public class ExportImportFileModelTests {

        [Fact]
        public void Can_Detect_Bedrock_Zip() {
            string _testFilePath = @"..\..\..\..\TestFiles\BedrockTest.zip";
            Assert.True(File.Exists(_testFilePath));
            ExportImportFileModel model = new(File.ReadAllBytes(_testFilePath));
            Assert.NotNull(model);
            Assert.Equal(FileTypes.BedrockServer, model.Manifest.FileType);
        }

    }
}
