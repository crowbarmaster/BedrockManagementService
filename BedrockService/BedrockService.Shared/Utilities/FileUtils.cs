using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BedrockService.Shared.Utilities {
    public class FileUtils {
        readonly string _servicePath;
        public FileUtils(string servicePath) {
            _servicePath = servicePath;
        }
        public void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        }

        public void DeleteFilesRecursively(DirectoryInfo source, bool removeSourceFolder) {
            foreach (DirectoryInfo dir in source.GetDirectories())
                DeleteFilesRecursively(dir, removeSourceFolder);
            foreach (FileInfo file in source.GetFiles())
                file.Delete();
            foreach (DirectoryInfo emptyDir in source.GetDirectories())
                emptyDir.Delete(true);
            if (removeSourceFolder)
                source.Delete(true);
        }

        public void ClearTempDir() {
            DirectoryInfo tempDirectory = new DirectoryInfo($@"{_servicePath}\Temp");
            if (!tempDirectory.Exists)
                tempDirectory.Create();
            foreach (FileInfo file in tempDirectory.GetFiles("*", SearchOption.AllDirectories))
                file.Delete();
            foreach (DirectoryInfo directory in tempDirectory.GetDirectories())
                directory.Delete(true);
        }

        public void DeleteFilelist(string[] fileList, string serverPath) {
            foreach (string file in fileList)
                try {
                    File.Delete($@"{serverPath}\{file}");
                }
                catch { }
            List<string> exesInPath = Directory.EnumerateFiles(serverPath, "*.exe", SearchOption.AllDirectories).ToList();
            foreach (string exe in exesInPath)
                File.Delete(exe);
            foreach (string dir in Directory.GetDirectories(serverPath))
                if (Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Count() == 0)
                    Directory.Delete(dir, true);
        }

        public void WriteStringToFile(string path, string content) => File.WriteAllText(path, content);

        public void WriteStringArrayToFile(string path, string[] content) => File.WriteAllLines(path, content);

 
        public Task<bool> BackupWorldFilesFromQuery(Dictionary<string, int> fileNameSizePairs, string worldPath, string destinationPath) {
            return Task.Run<bool>(() => {
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
                }
                catch (System.Exception ex) {
                    Console.WriteLine($"Error! {ex.Message}");
                }
                return false;
            });
        }
    }
}
