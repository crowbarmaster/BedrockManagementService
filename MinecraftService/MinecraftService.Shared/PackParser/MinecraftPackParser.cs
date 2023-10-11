using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace MinecraftService.Shared.PackParser {
    public class MinecraftPackParser {
        public string PackExtractDirectory;
        public List<MinecraftPackContainer> FoundPacks = new();
        private readonly IServerLogger _serverLogger;

        [JsonConstructor]
        public MinecraftPackParser() {
        }

        public MinecraftPackParser(IServerLogger logger) {
            PackExtractDirectory = $"{Path.GetTempPath()}\\MMSTemp\\PackExtract";
            _serverLogger = logger;
        }



        public MinecraftPackParser(byte[] fileContents, IServerLogger logger) {
            _serverLogger = logger;
            PackExtractDirectory = $"{Path.GetTempPath()}\\MMSTemp\\PackExtract";
            FileUtilities.ClearTempDir().Wait();
            using (MemoryStream fileStream = new(fileContents, 5, fileContents.Length - 5)) {
                using ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Read);
                zipArchive.ExtractToDirectory(PackExtractDirectory);
            }
            ParseDirectory(PackExtractDirectory);
        }

        public MinecraftPackParser(string[] files, string extractDir, IServerLogger logger) {
            _serverLogger = logger;
            PackExtractDirectory = extractDir;
            FileUtilities.ClearTempDir().Wait();
            if (Directory.Exists($@"{PackExtractDirectory}\ZipTemp")) {
                Directory.CreateDirectory($@"{PackExtractDirectory}\ZipTemp");
            }
            foreach (string file in files) {
                FileInfo fInfo = new(file);
                string zipFilePath = $@"{PackExtractDirectory}\{fInfo.Name.Replace(fInfo.Extension, "")}";
                ZipFile.ExtractToDirectory(file, zipFilePath);
                foreach (FileInfo extractedFile in new DirectoryInfo(zipFilePath).GetFiles()) {
                    if (extractedFile.Extension == ".mcpack") {
                        Directory.CreateDirectory($@"{zipFilePath}\{extractedFile.Name.Replace(extractedFile.Extension, "")}");
                        ZipFile.ExtractToDirectory(extractedFile.FullName, $@"{zipFilePath}\{fInfo.Name.Replace(fInfo.Extension, "")}_{extractedFile.Name.Replace(extractedFile.Extension, "")}");
                    }
                }
            }
            ParseDirectory(PackExtractDirectory);
        }

        public void ParseDirectory(string directoryToParse) {
            DirectoryInfo directoryInfo = new(directoryToParse);
            if (directoryInfo.Exists) {
                foreach (FileInfo file in directoryInfo.GetFiles("*", SearchOption.AllDirectories)) {
                    if (file.Name == "levelname.txt") {
                        byte[] iconBytes;
                        try {
                            if (File.Exists($@"{file.Directory.FullName}\world_icon.jpeg"))
                                iconBytes = File.ReadAllBytes($@"{file.Directory.FullName}\world_icon.jpeg");
                            else
                                iconBytes = File.ReadAllBytes($@"{file.Directory.FullName}\world_icon.png");
                        } catch {
                            iconBytes = null;
                        }

                        FoundPacks.Add(new MinecraftPackContainer {
                            JsonManifest = null,
                            PackContentLocation = file.Directory.FullName,
                            ManifestType = "WorldPack",
                            FolderName = file.Directory.Name,
                            IconBytes = iconBytes
                        });
                        continue;
                    }
                    if (file.Name == "manifest.json") {
                        byte[] iconBytes = File.Exists($@"{file.Directory.FullName}\pack_icon.jpeg") ?
                            File.ReadAllBytes($@"{file.Directory.FullName}\pack_icon.jpeg") :
                            File.Exists($@"{file.Directory.FullName}\pack_icon.png") ?
                            File.ReadAllBytes($@"{file.Directory.FullName}\pack_icon.png") :
                            null;
                        System.EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> JsonError = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) {
                            if(_serverLogger != null) { 
                                _serverLogger.AppendLine($"Error parsing packs: {args.ErrorContext.Error.Message}.\r\nPack object: {args.CurrentObject.ToString()}");
                            }
                            args.ErrorContext.Handled = true;
                        };
                        MinecraftPackContainer container = new() {
                            JsonManifest = new(),
                            PackContentLocation = file.Directory.FullName,
                            ManifestType = "",
                            FolderName = file.Directory.Name,
                            IconBytes = iconBytes
                        };
                        try {
                            container.JsonManifest = JsonConvert.DeserializeObject<PackManifestJsonModel>(File.ReadAllText(file.FullName), new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Error = JsonError });
                            container.ManifestType = container.JsonManifest.modules[0].type ?? "unknown";
                            FoundPacks.Add(container);
                        } catch (Exception e) {
                            if (_serverLogger != null) { 
                                string plugin = container != null && container.FolderName != null ? container.FolderName : "null";
                                _serverLogger.AppendLine($"Error parsing pack {plugin}! Error: {e.Message}");
                            }
                            continue;
                        }
                    }
                }
            }
        }
    }
}
