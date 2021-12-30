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
        private DirectoryInfo _directory = new(_testFilePath);
        [Fact]
        public void Can_Create_JsonFiles() {
            IProcessInfo processInfo = new ServiceProcessInfo("TestHost", _directory.FullName, 0, true, true);
            MinecraftUpdatePackageProcessor processor = new MinecraftUpdatePackageProcessor(new BedrockLogger(processInfo, new ServiceInfo(processInfo)), processInfo, "1.0", @$"{_testFilePath}\Output");
            processor.ExtractFilesToDirectory();
            Assert.True(File.Exists(@$"{_testFilePath}\Output\stock_props.conf"));
            Assert.True(File.Exists(@$"{_testFilePath}\Output\stock_packs.json"));
        }
    }
}
