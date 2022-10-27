using BedrockService.Shared.Interfaces;
using BedrockService.Shared.MinecraftFileModels.FileAccessModels;
using BedrockService.Shared.MinecraftFileModels.JsonModels;
using BedrockService.Shared.PackParser;
using BedrockService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace BedrockService.Shared.Classes {
    public class MinecraftUpdatePackageProcessor {
        private readonly string _serviceDirectory;
        private readonly string _packageVersion;
        private readonly string _fileTargetDirectory;
        private readonly string _workingDirectory;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly bool _loggingEnabled = true;


        public MinecraftUpdatePackageProcessor(IBedrockLogger logger, IProcessInfo processInfo, string packageVersion, string fileTargetDirectory) {
            _processInfo = processInfo;
            _packageVersion = packageVersion;
            _fileTargetDirectory = fileTargetDirectory;
            _serviceDirectory = processInfo.GetDirectory();
            _workingDirectory = $@"{Path.GetTempPath()}\BMSTemp\ServerFileTemp";
            Directory.CreateDirectory(_workingDirectory);
            _logger = logger;
        }
        
        public MinecraftUpdatePackageProcessor(IProcessInfo processInfo, string packageVersion, string fileTargetDirectory) {
            _processInfo = processInfo;
            _packageVersion = packageVersion;
            _fileTargetDirectory = fileTargetDirectory;
            _serviceDirectory = processInfo.GetDirectory();
            _workingDirectory = $@"{Path.GetTempPath()}\BMSTemp\ServerFileTemp";
            _loggingEnabled = false;
        }

        public bool ExtractCoreFiles() {
            try {
                FileUtilities fileUtils = new FileUtilities(_processInfo);
                fileUtils.ClearTempDir().Wait();
                Directory.CreateDirectory(_workingDirectory);
                string zipPath = $@"{_serviceDirectory}\BmsConfig\BDSBuilds\BuildArchives\Update_{_packageVersion}.zip";
                if (!File.Exists(zipPath)) {
                    if(_loggingEnabled) _logger.AppendLine("Requested build package was not found. BMS will attempt to fetch it now...");
                    if (!Updater.FetchBuild(_serviceDirectory, _packageVersion).Result) {
                        if (_loggingEnabled) _logger.AppendLine($"Version {_packageVersion} was not found on mojang servers. Place package manaually, or check configured version for error!");
                        return false;
                    }
                }
                using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                    int fileCount = archive.Entries.Count;
                    for (int i = 0; i < fileCount; i++) {
                        int percentResult = 1;
                        try {
                            percentResult = i % (((int)Math.Round(fileCount / 10.0)) * 10 / 6);
                        } catch (DivideByZeroException) { }
                        if (percentResult == 0) {
                            if (_loggingEnabled) _logger.AppendLine($"Extracting server files to build core files, {Math.Round(i / (double)fileCount, 2) * 100}% completed...");
                        }
                        string fixedPath = $@"{_workingDirectory}\{archive.Entries[i].FullName.Replace('/', '\\')}";
                        if (!archive.Entries[i].FullName.EndsWith("/")) {
                            if (File.Exists(fixedPath)) {
                                File.Delete(fixedPath);
                            }
                            FileInfo fileInfo = new FileInfo(fixedPath);
                            if(fileInfo.Extension == ".properties") {
                                archive.Entries[i].ExtractToFile(fixedPath);
                            }
                        } else {
                            if (!Directory.Exists(fixedPath)) {
                                Directory.CreateDirectory(fixedPath);
                            }
                        }
                    }
                    if (_loggingEnabled) _logger.AppendLine($"Extraction completed.");
                    CreateFiles();
                    return true;
                }
            } catch (Exception ex) {
                if (_loggingEnabled) _logger.AppendLine($"Error extracting core files. Verify build archive \"Update_{_packageVersion}.zip\" exists in BDSBuilds folder!");
                return false;
            }
        }

        private void CreateFiles() {
            if (_loggingEnabled) _logger.AppendLine($"Now building necessary files");
            Directory.CreateDirectory(_fileTargetDirectory);
            string propFile = $@"{_workingDirectory}\server.properties";

            List<string> propFileContents = new(File.ReadAllLines(propFile));
            propFileContents = propFileContents
                .Where(x => !x.StartsWith('#'))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            File.WriteAllLines($@"{_fileTargetDirectory}\stock_props.conf", propFileContents);
        }
    }
}
