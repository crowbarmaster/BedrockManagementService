using BedrockService.Shared.Interfaces;
using BedrockService.Shared.MinecraftFileModels.FileAccessModels;
using BedrockService.Shared.MinecraftFileModels.JsonModels;
using BedrockService.Shared.PackParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Utilities {
    public class FileUtilities {
        readonly string _servicePath;

        public FileUtilities() {
            _servicePath = GetServiceDirectory(BmsDirectoryKeys.WorkingDirectory);
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


        public void AppendServerPacksToArchive(string serverPath, ZipArchive backupZip, DirectoryInfo levelDirInfo) {
            string levelName = levelDirInfo.Name;
            CreatePackBackupFiles(serverPath, levelName, backupZip);
            if (Directory.Exists(GetServerDirectory(BdsDirectoryKeys.ResourcePacksDir, serverPath))) {
                ClearTempDir().Wait();
                ZipFile.CreateFromDirectory(string.Format(GetServerDirectory(BdsDirectoryKeys.ResourcePacksDir, serverPath), levelName), $@"{Path.GetTempPath()}\BMSTemp\resource_packs.zip");
                backupZip.CreateEntryFromFile($@"{Path.GetTempPath()}\BMSTemp\resource_packs.zip", "resource_packs.zip");
            }
            if (Directory.Exists(GetServerDirectory(BdsDirectoryKeys.BehaviorPacksDir, serverPath))) {
                ClearTempDir().Wait();
                ZipFile.CreateFromDirectory(string.Format(GetServerDirectory(BdsDirectoryKeys.BehaviorPacksDir, serverPath), levelName), $@"{Path.GetTempPath()}\BMSTemp\behavior_packs.zip");
                backupZip.CreateEntryFromFile($@"{Path.GetTempPath()}\BMSTemp\behavior_packs.zip", "behavior_packs.zip");
            }
        }

        public void CreatePackBackupFiles(string serverPath, string levelName, ZipArchive destinationArchive) {
            string resouceFolderPath = GetServerDirectory(BdsDirectoryKeys.ResourcePacksDir, serverPath);
            string behaviorFolderPath = GetServerDirectory(BdsDirectoryKeys.BehaviorPacksDir, serverPath);
            string behaviorFilePath = GetServerFilePath(BdsFileNameKeys.WorldBehaviorPacks, serverPath);
            string resoruceFilePath = GetServerFilePath(BdsFileNameKeys.WorldResourcePacks, serverPath);
            MinecraftPackParser packParser = new();
            packParser.ParseDirectory(resouceFolderPath);
            packParser.ParseDirectory(behaviorFolderPath);
            WorldPackFileModel worldPacks = new(resoruceFilePath);
            worldPacks.Contents.AddRange(new WorldPackFileModel(behaviorFilePath).Contents);
            ClearTempDir().Wait();
            string packBackupFolderPath = $@"{Path.GetTempPath()}\BMSTemp\InstalledPacks";
            Directory.CreateDirectory(packBackupFolderPath);
            if (worldPacks.Contents.Count > 0) {
                foreach (WorldPackEntryJsonModel model in worldPacks.Contents) {
                    MinecraftPackContainer container = packParser.FoundPacks.FirstOrDefault(x => x.JsonManifest.header.uuid == model.pack_id);
                    if (container == null) {
                        continue;
                    }
                    string packLocation = container.PackContentLocation;
                    ZipFile.CreateFromDirectory(packLocation, $@"{packBackupFolderPath}\{packLocation[packLocation.LastIndexOf('\\')..]}.zip");
                    destinationArchive.CreateEntryFromFile($@"{packBackupFolderPath}\{packLocation[packLocation.LastIndexOf('\\')..]}.zip", $@"InstalledPacks/{packLocation[packLocation.LastIndexOf('\\')..]}.zip");
                }
            }
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

        private int RoundOff(int i) {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        public Task ExtractZipToDirectory(string zipPath, string directory, IProgress<double> progress) => Task.Run(() => {
            FileInfo fileInfo = new FileInfo(zipPath);
            using ZipArchive archive = ZipFile.OpenRead(zipPath);
            int fileCount = archive.Entries.Count;
            for (int i = 0; i < fileCount; i++) {
                string fixedPath = $@"{directory}\{archive.Entries[i].FullName.Replace('/', '\\')}";
                if (i % (RoundOff(fileCount) / 6) == 0) {
                    progress.Report(Math.Round(i / (double)fileCount, 2) * 100);
                }
                if (fixedPath.EndsWith("\\")) {
                    if (!Directory.Exists(fixedPath)) {
                        Directory.CreateDirectory(fixedPath);
                    }
                } else {
                    Task.Run(() => {
                        archive.Entries[i].ExtractToFile(fixedPath, true);
                    }).Wait();
                }
            }
        });
    }
}
