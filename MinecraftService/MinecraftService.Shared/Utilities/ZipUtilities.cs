using MinecraftService.Shared.Classes;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Utilities {
    public class ZipUtilities {

        private static int RoundOff(int i) {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        public static Task ExtractToDirectory(Stream fileStream, string directory, IProgress<ProgressModel> progress, bool isClient = false) => Task.Run(() => {
            using ZipArchive archive = new(fileStream, ZipArchiveMode.Read);
            Regex jdkName = new(@"jdk-17\.[0-9]+\.[0-9]+/?(.*)");
            int fileCount = archive.Entries.Count;
            for (int i = 0; i < fileCount + 0; i++) {
                string fixedEntry = archive.Entries[i].FullName;
                if (fixedEntry.StartsWith("LiteLoaderBDS/")) {
                    fixedEntry = fixedEntry[14..];
                }
                if (jdkName.IsMatch(fixedEntry)) {
                    Match match = jdkName.Match(fixedEntry);
                    fixedEntry = match.Groups[1].Value;
                }
                if (string.IsNullOrEmpty(fixedEntry)) {
                    continue;
                }
                string fixedPath = $@"{directory}\{fixedEntry.Replace('/', '\\')}";
                if (fixedPath.EndsWith('\\')) {
                    continue;
                }
                int progressCallCount = (RoundOff(fileCount) / (isClient ? (fileCount / 10 > 1 ? fileCount / 10 : 1) : 6)) > 1 ? (RoundOff(fileCount) / (isClient ? (fileCount / 10 > 1 ? fileCount / 10 : 1) : 6)) : 1;
                if (progress != null && i % progressCallCount == 0) {
                    progress.Report(new(fixedPath.Substring(fixedPath.LastIndexOf('\\') + 1), Math.Round(i / (double)fileCount, 2) * 100));
                }
                FileInfo fileInfo = new(fixedPath);
                fileInfo.Directory.Create();
                Task.Run(() => {
                    archive.Entries[i].ExtractToFile(fixedPath, true);
                }).Wait();
            }
            fileStream.Close();
            progress.Report(new("Zip file has been extracted.", 100));
        });

        public static Task ExtractToDirectory(string filePath, string directory, IProgress<ProgressModel> progress, bool isClient = false) => ExtractToDirectory(File.OpenRead(filePath), directory, progress, isClient);

        public static Task AppendFile(string targetFile, string entryName, ZipArchive zipArchive) {
            return Task.Run(() => {
                if (string.IsNullOrEmpty(targetFile)) {
                    return;
                }
                if (!File.Exists(targetFile)) {
                    return;
                }
                if (zipArchive == null) {
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

        public static Task CreateFromDirectory(string targetDirectory, string targetZipPath, IProgress<ProgressModel> progress, bool isClient = false) {
            return Task.Run(() => {
                if (string.IsNullOrEmpty(targetDirectory)) {
                    return;
                }
                using FileStream zipStream = File.Open(targetZipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                ZipArchive zipArchive = new(zipStream);
                DirectoryInfo dirInfo = new DirectoryInfo(targetDirectory);
                var filesInDir = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);
                int fileCount = filesInDir.Count();
                int progressCallCount = (RoundOff(fileCount) / (isClient ? (fileCount / 10 > 1 ? fileCount / 10 : 1) : 6)) > 1 ? (RoundOff(fileCount) / (isClient ? (fileCount / 10 > 1 ? fileCount / 10 : 1) : 6)) : 1;
                int currentFile = 0;

                foreach (FileInfo file in filesInDir) {
                    using FileStream fileStream = File.Open(targetZipPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    string fixedPath = file.FullName.Replace($"{targetDirectory}\\", string.Empty).Replace('\\', '/');
                    ZipArchiveEntry newZippedFile = zipArchive.CreateEntry(fixedPath, CompressionLevel.Optimal);
                    using Stream newZippedStream = newZippedFile.Open();
                    fileStream.CopyTo(newZippedStream);
                    fileStream.Close();
                    currentFile++;
                    if (progress != null && currentFile % progressCallCount == 0) {
                        progress.Report(new($"Compressed file: {fixedPath.Substring(fixedPath.LastIndexOf('/') + 1)}", Math.Round(currentFile / (double)fileCount, 2) * 100));
                    }
                }
                zipStream.Close();
                progress.Report(new("Zip file has been created.", 100));
            });
        }

    }
}
