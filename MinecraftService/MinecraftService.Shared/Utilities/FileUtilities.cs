using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.FileModels.MinecraftFileModels;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.PackParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Utilities
{
    public class FileUtilities {

        public static string GetRandomPrefix() {
            Random random = new Random();
            int rndNumber = random.Next(111111, 9999999);
            return rndNumber.ToString();
        }

        public static void CreateInexistentFile(string filePath) {
            FileInfo fileInfo = new FileInfo(filePath);
            if (!Directory.Exists(fileInfo.Directory.FullName)) {
                fileInfo.Directory.Create();
            }
            if (!fileInfo.Exists) {
                fileInfo.Create().Close();
            }
        }

        public static void CreateInexistentDirectory(string DirectoryPath) {
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
                    CreateInexistentDirectory(newFile.DirectoryName);
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
            string resouceFolderPath = GetServerDirectory(ServerDirectoryKeys.ResourcePacksDir_LevelName, serverPath, levelName);
            string behaviorFolderPath = GetServerDirectory(ServerDirectoryKeys.BehaviorPacksDir_LevelName, serverPath, levelName);
            string behaviorManifestPath = GetServerFilePath(ServerFileNameKeys.WorldBehaviorPacks, serverPath, levelName);
            string rescManifestPath = GetServerFilePath(ServerFileNameKeys.WorldResourcePacks, serverPath, levelName);
            MinecraftPackParser packParser = new();
            packParser.ParseDirectory(resouceFolderPath, 0);
            packParser.ParseDirectory(behaviorFolderPath, 0);
            WorldPackFileModel worldPacks = new(rescManifestPath);
            worldPacks.Contents.AddRange(new WorldPackFileModel(behaviorManifestPath).Contents);
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

        public static List<string> ReadLines(string path) {
            CreateInexistentFile(path);
            return File.ReadLines(path).ToList();
        }
    }
}
