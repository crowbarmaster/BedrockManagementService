using BedrockService.Shared.Interfaces;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace BedrockService.Shared.Utilities {
    public class FileUtilities {
        readonly string _servicePath;
        private readonly IProcessInfo _processInfo;

        public FileUtilities(IProcessInfo processInfo) {
            _processInfo = processInfo;
            _servicePath = _processInfo.GetDirectory();
        }

        public void CreateInexistantFile(string filePath) {
            if (!File.Exists(filePath)) {
                File.Create(filePath).Close();
            }
        }

        public void CreateInexistantDirectory(string DirectoryPath) {
            if (!Directory.Exists(DirectoryPath)) { 
                Directory.CreateDirectory(DirectoryPath);
            }
        }

        public void CopyFolderTree(DirectoryInfo source, DirectoryInfo target) {
            source.EnumerateFiles("*", SearchOption.AllDirectories)
                .ToList().ForEach(x => {
                    FileInfo newFile = new(x.FullName.Replace(source.FullName, target.FullName));
                    CreateInexistantDirectory(newFile.DirectoryName);
                    x.CopyTo(newFile.FullName, true); 
                });
        }

        public void CopyFilesMatchingExtension(string source, string target, string extension) {
            if (!extension.StartsWith('.')) {
                extension = $".{extension}";
            }
            new DirectoryInfo(source).EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(x => x.Extension.Equals(extension))
                .ToList()
                .ForEach(x => x.CopyTo(Path.Combine(target, x.Name), true));
        }

        public Task DeleteFilesFromDirectory(DirectoryInfo source, bool removeSourceFolder) {
            return Task.Run(() => {
                var files = source.EnumerateFiles("*", SearchOption.AllDirectories)
                    .ToList();
                files.ForEach(x => x.Delete());
                var dirs = source.EnumerateDirectories("*", SearchOption.AllDirectories)
                    .Reverse()
                    .ToList();
                dirs.ForEach(x => x.Delete());
                if (removeSourceFolder)
                    source.Delete(true);
            });
        }

        public void DeleteFilesFromDirectory(string source, bool removeSourceFolder) => DeleteFilesFromDirectory(new DirectoryInfo(source), removeSourceFolder);

        public Task ClearTempDir() {
            return Task.Run(() => {
                DirectoryInfo tempDirectory = new($"{Path.GetTempPath()}\\BMSTemp");
                if (!tempDirectory.Exists)
                    tempDirectory.Create();
                DeleteFilesFromDirectory(tempDirectory, false).Wait();
            });
        }

        public void DeleteFilelist(string[] fileList, string serverPath) {
            foreach (string file in fileList)
                try {
                    File.Delete($@"{serverPath}\{file}");
                } catch { }
            List<string> exesInPath = Directory.EnumerateFiles(serverPath, "*.exe", SearchOption.AllDirectories).ToList();
            foreach (string exe in exesInPath)
                File.Delete(exe);
            foreach (string dir in Directory.GetDirectories(serverPath))
                if (Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Count() == 0)
                    Directory.Delete(dir, true);
        }

        public void WriteStringToFile(string path, string content) => File.WriteAllText(path, content);

        public void WriteStringArrayToFile(string path, string[] content) => File.WriteAllLines(path, content);
    }
}
