using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.IO;
using Xunit;

namespace ServiceTests {
    public class MinecraftUpdatePackageProcessorTests {
        const string _testFilePath = @"..\..\..\..\..\TestFiles";
        private readonly DirectoryInfo _directory = new(_testFilePath);
        [Fact]
        public void Can_Create_JsonFiles() {
            _directory.Create();
            DirectoryInfo outDir = new DirectoryInfo(_testFilePath + @"\Output");
            IProcessInfo processInfo = new ServiceProcessInfo("TestHost", _directory.FullName, 0, true, true);
            MinecraftUpdatePackageProcessor processor = new(new BedrockLogger(processInfo, new ServiceConfigurator(processInfo)), "1.0", @$"{_testFilePath}\Output");
            processor.ExtractCoreFiles();
            Assert.True(File.Exists(@$"{_testFilePath}\Output\stock_props.conf"));
        }
    }
}
