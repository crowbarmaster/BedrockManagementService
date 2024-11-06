using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Server.Updaters;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Interfaces;
using System.IO;
using Xunit;

namespace MMS_Tests.SharedTests
{
    public class MinecraftUpdatePackageProcessorTests {
        const string _testFilePath = @"..\..\..\..\..\TestFiles";
        private readonly DirectoryInfo _directory = new(_testFilePath);
        [Fact]
        public void Can_Create_JsonFiles() {
            _directory.Create();
            DirectoryInfo outDir = new DirectoryInfo(_testFilePath + @"\Output");
            outDir.Create();
            ProcessInfo processInfo = new ProcessInfo("TestHost", _directory.FullName, 0, true, true);
            BedrockUpdatePackageProcessor processor = new(new MinecraftService.Shared.Classes.Service.Core.MmsLogger(processInfo, new ServiceConfigurator(processInfo, new())), "1.0", outDir.FullName);
            processor.ExtractCoreFiles();
            Assert.True(File.Exists(@$"{_testFilePath}\Output\stock_props.conf"));
        }
    }
}
