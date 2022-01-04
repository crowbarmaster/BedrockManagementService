using BedrockService.Shared.PackParser;
using BedrockService.Shared.MinecraftJsonModels.FileModels;
using BedrockService.Shared.MinecraftJsonModels.JsonModels;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BedrockService.Shared.Interfaces;

namespace BedrockManagementServiceASP.BedrockService.Management {
    public class MinecraftUpdatePackageProcessor {
        private readonly string _serviceDirectory;
        private readonly string _packageVersion;
        private readonly string _fileTargetDirectory;
        private readonly string _workingDirectory;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;


        public MinecraftUpdatePackageProcessor(IBedrockLogger logger, IProcessInfo processInfo, string serviceDirectory, string packageVersion, string fileTargetDirectory) {
            _processInfo = processInfo;
            _serviceDirectory = serviceDirectory;
            _packageVersion = packageVersion;
            _fileTargetDirectory = fileTargetDirectory;
            _workingDirectory = $@"{_serviceDirectory}\Temp\ServerFileTemp";
            Directory.CreateDirectory(_workingDirectory);
            _logger = logger;
        }

        public void ExtractToDirectory() {
            using (ZipArchive archive = ZipFile.OpenRead($@"{_serviceDirectory}\Server\MCSFiles\Update_{_packageVersion}.zip")) {
                int fileCount = archive.Entries.Count;
                for (int i = 0; i < fileCount; i++) {
                    int percentResult = 1;
                    try {
                        percentResult = i % ((int)Math.Round(fileCount / 10.0) * 10 / 6);
                    } catch (DivideByZeroException) { }
                    if (percentResult == 0) {
                        _logger.AppendLine($"Extracting server files to build core files, {Math.Round(i / (double)fileCount, 2) * 100}% completed...");
                    }
                    string fixedPath = $@"{_workingDirectory}\{archive.Entries[i].FullName.Replace('/', '\\')}";
                    if (!archive.Entries[i].FullName.EndsWith("/")) {
                        if (File.Exists(fixedPath)) {
                            File.Delete(fixedPath);
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
            MinecraftPackParser packParser = new MinecraftPackParser(_processInfo);
            packParser.ParseDirectory(resouceFolder);
            packParser.ParseDirectory(behaviorFolder);
            KnownPacksFileModel fileModel = new KnownPacksFileModel();
            fileModel.FilePath = $@"{_fileTargetDirectory}\stock_packs.json";
            foreach (MinecraftPackContainer pack in packParser.FoundPacks) {
                KnownPacksJsonModel jsonModel = new KnownPacksJsonModel();
                jsonModel.file_system = "RawPath";
                jsonModel.path = pack.PackContentLocation;
                jsonModel.uuid = pack.JsonManifest.header.uuid;
                jsonModel.version = string.Join(".", pack.JsonManifest.header.version);
                fileModel.Contents.Add(jsonModel);
            }
            fileModel.SaveToFile();
        }
    }
}
