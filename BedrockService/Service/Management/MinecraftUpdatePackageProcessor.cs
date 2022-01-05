using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using BedrockService.Shared.PackParser;
using System.IO.Compression;

namespace BedrockService.Service.Management {
    public class MinecraftUpdatePackageProcessor {
        private readonly string _serviceDirectory;
        private readonly string _packageVersion;
        private readonly string _fileTargetDirectory;
        private readonly string _workingDirectory;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;


        public MinecraftUpdatePackageProcessor(IBedrockLogger logger, IProcessInfo processInfo, string packageVersion, string fileTargetDirectory) {
            _processInfo = processInfo;
            _packageVersion = packageVersion;
            _fileTargetDirectory = fileTargetDirectory;
            _serviceDirectory = processInfo.GetDirectory();
            _workingDirectory = $@"{_serviceDirectory}\Temp\ServerFileTemp";
            Directory.CreateDirectory(_workingDirectory);
            _logger = logger;
        }

        public void ExtractFilesToDirectory() {
            using (ZipArchive archive = ZipFile.OpenRead($@"{_serviceDirectory}\Server\MCSFiles\Update_{_packageVersion}.zip")) {
                int fileCount = archive.Entries.Count;
                for (int i = 0; i < fileCount; i++) {
                    int percentResult = 1;
                    try {
                        percentResult = i % (((int)Math.Round(fileCount / 10.0)) * 10 / 6);
                    } catch (DivideByZeroException) { }
                    if (percentResult == 0) {
                        _logger.AppendLine($"Extracting server files to build core files, {Math.Round((double)i / (double)fileCount, 2) * 100}% completed...");
                    }
                    string fixedPath = $@"{_workingDirectory}\{archive.Entries[i].FullName.Replace('/', '\\')}";
                    if (!archive.Entries[i].FullName.EndsWith("/")) {
                        if (File.Exists(fixedPath)) {
                            File.Delete(fixedPath);
                        }
                        if (fixedPath.Contains("bedrock_server.pdb")) {
                            continue;
                        }
                        archive.Entries[i].ExtractToFile(fixedPath);
                    } else {
                        if (!Directory.Exists(fixedPath)) {
                            Directory.CreateDirectory(fixedPath);
                        }
                    }
                }
                _logger.AppendLine($"Extraction completed.");
                CreateFiles();
            }
        }

        private void CreateFiles() {
            _logger.AppendLine($"Now building necessary files");
            Directory.CreateDirectory(_fileTargetDirectory);
            string resouceFolder = $@"{_workingDirectory}\resource_packs";
            string behaviorFolder = $@"{_workingDirectory}\behavior_packs";
            string propFile = $@"{_workingDirectory}\server.properties";
            MinecraftPackParser packParser = new(_processInfo);
            packParser.ParseDirectory(resouceFolder);
            packParser.ParseDirectory(behaviorFolder);
            KnownPacksFileModel fileModel = new();
            fileModel.FilePath = $@"{_fileTargetDirectory}\stock_packs.json";
            foreach (MinecraftPackContainer pack in packParser.FoundPacks) {
                string fixedPath = pack.PackContentLocation
                        .Replace(_workingDirectory + '\\', "")
                        .Replace('\\', '/');
                KnownPacksJsonModel jsonModel = new() {
                    file_system = "RawPath",
                    path = fixedPath,
                    uuid = pack.JsonManifest.header.uuid,
                    version = string.Join(".", pack.JsonManifest.header.version)
                };
                fileModel.Contents.Add(jsonModel);
            }
            fileModel.SaveToFile();
            List<string> propFileContents = new(File.ReadAllLines(propFile));
            propFileContents = propFileContents
                .Where(x => !x.StartsWith('#'))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            File.WriteAllLines($@"{_fileTargetDirectory}\stock_props.conf", propFileContents);
        }
    }
}
