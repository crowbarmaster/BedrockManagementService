using MinecraftService.Shared.Classes;
using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.PackParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Utilities {
    public class FileUtilities {

        public static string GetRandomPrefix() {
            Random random = new Random();
            int rndNumber = random.Next(111111, 9999999);
            return rndNumber.ToString();
        }

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
                Directory.CreateDirectory(DirectoryPath);
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

        public static Task DeleteFilesFromDirectory(DirectoryInfo source, bool removeSourceFolder, IProgress<ProgressModel> progress) {
            return Task.Run(() => {
                if (!source.Exists) {
                    return;
                }
                int curFileCount = 0;
                double prog = 0.0;
                var files = source.EnumerateFiles("*", SearchOption.AllDirectories)
                    .ToList();
                double reportAt = files.Count() / 6.0;
                files.ForEach(x => {
                    curFileCount++;
                    if (curFileCount % reportAt == 0) {
                        progress.Report(new("Deleting files...", Math.Round(prog += 100.0 / 6.0)));
                    }
                    x.Delete();
                });
                var dirs = source.EnumerateDirectories("*", SearchOption.AllDirectories)
                    .Reverse()
                    .ToList();
                dirs.ForEach(x => x.Delete());
                if (removeSourceFolder)
                    source.Delete(true);
            });
        }

        public static void AppendServerPacksToArchive(string serverPath, string levelName, ZipArchive destinationArchive, IProgress<ProgressModel> progress) {
            string resouceFolderPath = GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir, serverPath, levelName);
            string behaviorFolderPath = GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir, serverPath, levelName);
            string behaviorFilePath = GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, serverPath, levelName);
            string resoruceFilePath = GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, serverPath, levelName);
            MinecraftPackParser packParser = new();
            packParser.ParseDirectory(resouceFolderPath, 0);
            packParser.ParseDirectory(behaviorFolderPath, 0);
            WorldPackFileModel worldPacks = new(resoruceFilePath);
            worldPacks.Contents.AddRange(new WorldPackFileModel(behaviorFilePath).Contents);
            string packBackupFolderPath = GetNewTempDirectory("InstalledPacks");
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

        public static void DeleteFilesFromDirectory(string source, bool removeSourceFolder, IProgress<ProgressModel> progress) => DeleteFilesFromDirectory(new DirectoryInfo(source), removeSourceFolder, progress);

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

        public static List<string> ReadLines(string path) {
            CreateInexistantFile(path);
            return File.ReadLines(path).ToList();
        }
    }
}
