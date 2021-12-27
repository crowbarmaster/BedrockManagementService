using BedrockService.Service.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using System.IO.Compression;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;

namespace ServiceTests
{
    public class MinecraftUpdatePackageProcessorTests {
        const string _testFilePath = @"..\..\..\..\..\TestFiles";
        [Fact]
        public void Can_Extract_Zip_File() {
            IProcessInfo processInfo = new ServiceProcessInfo("TestHost", _testFilePath, 0, true, true);
            MinecraftUpdatePackageProcessor processor = new MinecraftUpdatePackageProcessor(new BedrockLogger(processInfo, new ServiceInfo(processInfo)), processInfo, _testFilePath, "1.0", @$"{_testFilePath}\Output");
            processor.ExtractToDirectory();
            Assert.True(File.Exists(@$"{_testFilePath}\Temp\ServerFileTemp\server.properties"));
            Assert.True(File.Exists(@$"{_testFilePath}\Output\stock_packs.json"));
        }
    }
}
