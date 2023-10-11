using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.PackParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Utilities {
    public class FileUtilities {

        public static void CreateInexistantFile(string filePath) {
            FileInfo fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName)) {
                fileInfo.Directory.Create();
            }
            if (!fileInfo.Exists) {
                fileInfo.Create().Close();
            }
        }

        public static void CreateInexistantDirectory(string DirectoryPath) {
            try {
                if (!Directory.Exists(DirectoryPath)) {
                    Directory.CreateDirectory(DirectoryPath);
                }
            } catch {
                return;
            }
        }

        public static void CopyFolderTree(DirectoryInfo source, DirectoryInfo target) {
            source.EnumerateFiles("*", SearchOption.AllDirectories)
                .ToList().ForEach(x => {
                    FileInfo newFile = new(x.FullName.Replace(source.FullName, target.FullName));
                    CreateInexistantDirectory(newFile.DirectoryName);
                    x.CopyTo(newFile.FullName, true);
                });
        }

        public static void CopyFilesMatchingExtension(string source, string target, string extension) {
            if (!extension.StartsWith('.')) {
                extension = $".{extension}";
            }
            new DirectoryInfo(source).EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(x => x.Extension.Equals(extension))
                .ToList()
                .ForEach(x => x.CopyTo(Path.Combine(target, x.Name), true));
        }

        public static Task DeleteFilesFromDirectory(DirectoryInfo source, bool removeSourceFolder) {
            return Task.Run(() => {
                if(!source.Exists) {
                    return;
                }
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


        public static void AppendServerPacksToArchive(string serverPath, ZipArchive backupZip, DirectoryInfo levelDirInfo) {
            string levelName = levelDirInfo.Name;
            CreatePackBackupFiles(serverPath, levelName, backupZip);
            if (Directory.Exists(GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName))) {
                ClearTempDir().Wait();
                ZipFile.CreateFromDirectory(GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName), $@"{Path.GetTempPath()}\MMSTemp\PackTemp\resource_packs.zip");
                backupZip.CreateEntryFromFile($@"{Path.GetTempPath()}\MMSTemp\PackTemp\resource_packs.zip", "resource_packs.zip");
            }
            if (Directory.Exists(GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName))) {
                ClearTempDir().Wait();
                ZipFile.CreateFromDirectory(GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName), $@"{Path.GetTempPath()}\MMSTemp\PackTemp\behavior_packs.zip");
                backupZip.CreateEntryFromFile($@"{Path.GetTempPath()}\MMSTemp\PackTemp\behavior_packs.zip", "behavior_packs.zip");
            }
        }

        public static void CreatePackBackupFiles(string serverPath, string levelName, ZipArchive destinationArchive) {
            string resouceFolderPath = GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName);
            string behaviorFolderPath = GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName);
            string behaviorFilePath = GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, serverPath, levelName);
            string resoruceFilePath = GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, serverPath, levelName);
            MinecraftPackParser packParser = new();
            packParser.ParseDirectory(resouceFolderPath);
            packParser.ParseDirectory(behaviorFolderPath);
            WorldPackFileModel worldPacks = new(resoruceFilePath);
            worldPacks.Contents.AddRange(new WorldPackFileModel(behaviorFilePath).Contents);
            ClearTempDir().Wait();
            string packBackupFolderPath = $@"{Path.GetTempPath()}\MMSTemp\InstalledPacks";
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

        public static void DeleteFilesFromDirectory(string source, bool removeSourceFolder) => DeleteFilesFromDirectory(new DirectoryInfo(source), removeSourceFolder);

        public static Task ClearTempDir() {
            return Task.Run(() => {
                DirectoryInfo tempDirectory = new($"{Path.GetTempPath()}\\MMSTemp");
                if (!tempDirectory.Exists)
                    tempDirectory.Create();
                DeleteFilesFromDirectory(tempDirectory, false).Wait();
            });
        }

        public static void DeleteFilelist(string[] fileList, string serverPath) {
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

        public static void WriteStringToFile(string path, string content) => File.WriteAllText(path, content);

        public static void WriteStringArrayToFile(string path, string[] content) => File.WriteAllLines(path, content);

        private static int RoundOff(int i) {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        public static List<string> ReadLines(string path)
        {
            CreateInexistantFile(path);
            return File.ReadLines(path).ToList();
        }
      
        public static Task ExtractZipToDirectory(string zipPath, string directory, IProgress<double> progress) => Task.Run(() => {
            FileInfo fileInfo = new(zipPath);
            using ZipArchive archive = ZipFile.OpenRead(zipPath);
            Regex jdkName = new(@"jdk-17\.[0-9]+\.[0-9]+/?(.*)");
            int fileCount = archive.Entries.Count;
            for (int i = 0; i < fileCount; i++) {
                string fixedEntry = archive.Entries[i].FullName;
                if (fixedEntry.StartsWith("LiteLoaderBDS/")) {
                    fixedEntry = fixedEntry[14..];
                }
                if (jdkName.IsMatch(fixedEntry)) {
                    Match match = jdkName.Match(fixedEntry);
                    fixedEntry = match.Groups[1].Value;
                }
                if(string.IsNullOrEmpty(fixedEntry)) {
                    continue;
                }
                string fixedPath = $@"{directory}\{fixedEntry.Replace('/', '\\')}";
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

        public static Task AppendFileToArchive(string targetFile, string entryName, ZipArchive zipArchive) {
            return Task.Run(() => {
                if (string.IsNullOrEmpty(targetFile)) {
                    return;
                }
                if(!File.Exists(targetFile)) {
                    return;
                }
                if(zipArchive == null) {
                    return;
                }
                using (FileStream fs = File.Open(targetFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    ZipArchiveEntry newZippedFile = zipArchive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using Stream zipStream = newZippedFile.Open();
                    fs.CopyTo(zipStream);
                    zipStream.Close();
                }
            });
        }
    }
}
