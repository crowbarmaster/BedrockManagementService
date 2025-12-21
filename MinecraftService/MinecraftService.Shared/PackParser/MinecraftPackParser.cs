using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.Minecraft;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftService.Shared.PackParser
{
    public class MinecraftPackParser {
        public string PackExtractDirectory;
        public List<MinecraftPackContainer> FoundPacks = new();
        private readonly MmsLogger _serverLogger;
        private IProgress<ProgressModel> _progress;
        private bool _isClient = false;

        [JsonConstructor]
        public MinecraftPackParser() {
            _progress = new Progress<ProgressModel>();
        }

        public MinecraftPackParser(MmsLogger logger, IProgress<ProgressModel> progress, string extractDir = "") {
            _progress = progress;
            _serverLogger = logger;
            PackExtractDirectory = extractDir != string.Empty ? extractDir : SharedStringBase.GetNewTempDirectory("PackExtract");
        }

        public Task ProcessServerData(byte[] fileContents, Action onCompletion) => Task.Run(() => {
            using (MemoryStream fileStream = new(fileContents)) {
                ZipUtilities.ExtractToDirectory(fileStream, PackExtractDirectory, _progress).Wait();
            }
            ParseDirectory(PackExtractDirectory, 0);
            onCompletion();
        });

        public Task ProcessClientFiles(string[] files, Action onCompletion) => Task.Run(() => {
            _isClient = true;
            if (Directory.Exists($@"{PackExtractDirectory}\ZipTemp")) {
                Directory.CreateDirectory($@"{PackExtractDirectory}\ZipTemp");
            }
            double currentProgress = 5.0;
            foreach (string file in files) {
                FileInfo fInfo = new(file);
                string zipFilePath = $@"{PackExtractDirectory}\{fInfo.Name.Replace(fInfo.Extension, "")}";
                Progress<ProgressModel> zipProgress = new((p) => {
                    p.Progress /= 2.5;
                    currentProgress = p.Progress;
                    _progress.Report(p);
                });
                _progress.Report(new("Extracting archive...", currentProgress));
                ZipUtilities.ExtractToDirectory(file, zipFilePath, zipProgress, true).Wait();
                currentProgress = 40.0;
                var dirInfoFiles = new DirectoryInfo(zipFilePath).GetFiles();
                _progress.Report(new("Search for and extract any .mcpack files...", currentProgress));
                foreach (FileInfo extractedFile in dirInfoFiles) {
                    if (extractedFile.Extension == ".mcpack") {
                        _progress.Report(new($"Found mcpack {extractedFile.Name}, extracting...", currentProgress));
                        Directory.CreateDirectory($@"{zipFilePath}\{extractedFile.Name.Replace(extractedFile.Extension, "")}");
                        ZipUtilities.ExtractToDirectory(extractedFile.FullName, $@"{zipFilePath}\{fInfo.Name.Replace(fInfo.Extension, "")}_{extractedFile.Name.Replace(extractedFile.Extension, "")}", _progress, true).Wait();
                    }
                }
            }
            ParseDirectory(PackExtractDirectory, currentProgress);
            onCompletion();
        });


        public void ParseDirectory(string directoryToParse, double startingPercent, IProgress<ProgressModel> progress) {
            _progress = progress;
            ParseDirectory(directoryToParse, startingPercent);
        }

        public void ParseDirectory(string directoryToParse, double currentProgress) {
            if (_progress != null) {
                _progress.Report(new("Processing pack manifest files...", currentProgress));
            }
            DirectoryInfo directoryInfo = new(directoryToParse);
            if (directoryInfo.Exists) {
                var dirInfoFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                double partsPerFile = _isClient ? 100 - currentProgress : dirInfoFiles.Count() / 6;
                int currentFile = 1;
                foreach (FileInfo file in dirInfoFiles) {
                    currentFile++;
                    currentProgress += ((100 - currentProgress) / partsPerFile);
                    if (_progress != null && currentFile % partsPerFile == 0) {
                        _progress.Report(new("Processing pack manifest files...", currentProgress));
                    }
                    if (file.Name == "levelname.txt") {
                        if (_progress != null) {
                            _progress.Report(new($"Found WorldPack: {file.Directory.Name}", currentProgress));
                        }
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
                        EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> JsonError = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) {
                            if (_serverLogger != null) {
                                _serverLogger.AppendLine($"Error parsing packs: {args.ErrorContext.Error.Message}");
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
                            if (container.JsonManifest.modules == null) {
                                continue;
                            }
                            container.ManifestType = container.JsonManifest.modules[0].type;
                            if (_progress != null) {
                                _progress.Report(new($"Found addon pack: {file.Directory.Name} ({container.ManifestType})", currentProgress));
                            }
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
                if(FoundPacks.Any()) {
                    foreach (MinecraftPackContainer pack in FoundPacks) {
                        if (pack.JsonManifest.dependencies != null && pack.JsonManifest.dependencies.Any()) {
                            foreach (PackManifestJsonModel.Dependency dependency in pack.JsonManifest.dependencies) {
                                var foundDepend = FoundPacks.Where(x => x.JsonManifest.header.uuid == dependency.uuid).ToList();
                                if (!foundDepend.Any()) {
                                    _serverLogger.AppendLine($"Warning! Pack {pack.FolderName} has a missing dependancy!");
                                }
                                if (foundDepend.First().JsonManifest.header.version != dependency.version) {
                                    _serverLogger.AppendLine($"Warning! Pack {pack.FolderName} has a dependancy with mis-matched versions!");
                                }
                            }
                        }
                    }
                }
            }
            if (_progress != null) {
                _progress.Report(new("Packs have been parsed.", 100));
            }
        }
    }
}
