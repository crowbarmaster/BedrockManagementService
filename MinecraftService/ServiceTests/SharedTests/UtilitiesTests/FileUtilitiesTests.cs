using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MMS_Tests.SharedTests.UtilitiesTests {
    public class FileUtilitiesTests {
        private readonly string _testDir = @".\TestDir";
        private readonly string _testCopyDir = @".\CopyDir";
        private readonly string _testFile = @".\TestDir\File.file";
        private readonly string _testCopyFile = @".\CopyDir\File.file";
        private readonly string _testFileAsJson = @".\TestDir\File.json";

        [Fact]
        public void Can_Create_Inexistant_Directory() {
            if (Directory.Exists(@".\TestDir")) {
                Directory.Delete(_testDir, true);
            }
            FileUtilities.CreateInexistantDirectory(_testDir);
            Assert.True(Directory.Exists(_testDir));
        }

        [Fact]
        public void Can_Create_Inexistant_File() {
            if (File.Exists(_testFile)) {
                File.Delete(_testFile);
            }
            FileUtilities.CreateInexistantFile(_testFile);
            Assert.True(File.Exists(_testFile));
        }

        [Fact]
        public void CreateInexistantDirectory_Does_Not_Modify_Existing () {
            Can_Create_Inexistant_File();
            FileUtilities.CreateInexistantDirectory(_testDir);
            Assert.True(File.Exists(_testFile));
        }

        [Fact]
        public void Can_Copy_Folder_Tree() {
            FileUtilities.CreateInexistantDirectory(_testCopyDir);
            FileUtilities.CreateInexistantFile(_testCopyFile);
            FileUtilities.CopyFolderTree(new(_testCopyDir), new(@".\CopyTestDir"));
            Assert.True(File.Exists(@$".\CopyTestDir{_testCopyFile.Substring(1)}"));
        }
    }
}
