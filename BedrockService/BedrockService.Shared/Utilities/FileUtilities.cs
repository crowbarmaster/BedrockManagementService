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

        public void DeleteFilesRecursively(DirectoryInfo source, bool removeSourceFolder) {
            var files = source.EnumerateFiles("*", SearchOption.AllDirectories)
                .ToList();
            files.ForEach(x => x.Delete());
            var dirs = source.EnumerateDirectories("*", SearchOption.AllDirectories)
                .Reverse()
                .ToList();
            dirs.ForEach(x => x.Delete());
            if (removeSourceFolder)
                source.Delete(true);
        }

        public void ClearTempDir() {
            DirectoryInfo tempDirectory = new($@"{_servicePath}\Temp");
            if (!tempDirectory.Exists)
                tempDirectory.Create();
            DeleteFilesRecursively(tempDirectory, false);
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

        public void CreatePackBackupFiles(string serverPath, string levelName, string targetDirectory) {
            string resouceFolderPath = $@"{serverPath}\resource_packs";
            string behaviorFolderPath = $@"{serverPath}\behavior_packs";
            string behaviorFilePath = $@"{serverPath}\worlds\{levelName}\world_behavior_packs.json";
            string resoruceFilePath = $@"{serverPath}\worlds\{levelName}\world_resource_packs.json";
            MinecraftPackParser packParser = new(_processInfo);
            packParser.ParseDirectory(resouceFolderPath);
            packParser.ParseDirectory(behaviorFolderPath);
            WorldPackFileModel worldPacks = new(resoruceFilePath);
            worldPacks.Contents.AddRange(new WorldPackFileModel(behaviorFilePath).Contents);
            string packBackupFolderPath = $@"{targetDirectory}\InstalledPacks";
            Directory.CreateDirectory(packBackupFolderPath);
            if (worldPacks.Contents.Count > 0) {
                foreach (WorldPackEntryJsonModel model in worldPacks.Contents) {
                    MinecraftPackContainer container = packParser.FoundPacks.First(x => x.JsonManifest.header.uuid == model.pack_id);
                    string packLocation = container.PackContentLocation;
                    ZipFile.CreateFromDirectory(packLocation, $@"{packBackupFolderPath}\{packLocation[packLocation.LastIndexOf('\\')..]}.zip");
                }
            }
        }

        public Task<bool> BackupWorldFilesFromQuery(Dictionary<string, int> fileNameSizePairs, string worldPath, string destinationPath) {
            return Task.Run(() => {
                try {
                    foreach (KeyValuePair<string, int> file in fileNameSizePairs) {
                        string fileName = file.Key.Replace('/', '\\');
                        int fileSize = file.Value;
                        string filePath = $@"{worldPath}\{fileName}";
                        string destFilePath = $@"{destinationPath}\{fileName}";
                        byte[] fileData = null;
                        using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (MemoryStream ms = new MemoryStream()) {
                            fs.CopyTo(ms);
                            ms.Position = 0;
                            fileData = ms.ToArray();
                        }
                        byte[] destData = new byte[fileSize];
                        Buffer.BlockCopy(fileData, 0, destData, 0, fileSize);
                        Directory.CreateDirectory(new FileInfo(destFilePath).DirectoryName);
                        File.WriteAllBytes(destFilePath, fileData);
                    }
                    return true;
                } catch (Exception ex) {
                    Console.WriteLine($"Error! {ex.Message}");
                }
                return false;
            });
        }
    }
}
