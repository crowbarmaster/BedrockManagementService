﻿using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes.Updaters {
    public class LiteUpdater : BedrockUpdater, IUpdater {
        private IServerLogger _logger;
        private bool _isInitialized = false;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerConfiguration serverConfiguration;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.LiteLoader;
        private string _version;
        private Dictionary<string, LiteLoaderVersionManifest> _versionLookupTable = new();

        public LiteUpdater(IServerLogger logger, IServiceConfiguration serviceConfiguration) : base(logger, serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _version = "None";
        }

        public LiteUpdater(IServerLogger logger, IServiceConfiguration serviceConfiguration, IServerConfiguration serverConfiguration) : base(logger, serviceConfiguration, serverConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            serverConfiguration = serverConfiguration;
            _logger = logger;
            _version = "None";
        }

        [JsonConstructor]
        public LiteUpdater() { }

        public new Task Initialize() => Task.Run(() => {
            base.Initialize().Wait();
            UpdateVersionTable();
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]))) {
                _logger.AppendLine("Version ini file missing, creating and fetching build...");
                File.Create(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch])).Close();
            }
            _version = File.ReadAllText(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]));
        });

        public void UpdateVersionTable() {
            _versionLookupTable.Clear();
            _logger.AppendLine($"Fetching LiteLoader version manifest...");
            int retryCount = 0;
            string content = string.Empty;
            string versionManifestPath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString());
            while (content == string.Empty) {
                try {
                    content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.LLReleasesJson]).Result;
                    File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString()), content);
                } catch {
                    if (retryCount > 2) {
                        if (File.Exists(versionManifestPath)) {
                            _logger.AppendLine($"Attempt to fetch {_serverArch} version manifest failed. Using previously stored copy!");
                            content = File.ReadAllText(versionManifestPath);
                            break;
                        }
                        _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                        return;
                    }
                    _logger.AppendLine($"Attempt to fetch {_serverArch} version manifest failed... retry {retryCount} of 2");
                    retryCount++;
                }
            }
            if (content != null) {
                List<LiteLoaderVersionManifest> manifestList = JsonConvert.DeserializeObject<List<LiteLoaderVersionManifest>>(content);
                foreach (LiteLoaderVersionManifest manifest in manifestList) {
                    _versionLookupTable.Add(manifest.Version, manifest);
                }
            }
            _serviceConfiguration.SetServerDefaultPropList(_serverArch, GetDefaultVersionPropList());
            File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]), _versionLookupTable.Values.Last().Version);
        }

        public new bool IsInitialized() => _isInitialized;

        public override Task CheckLatestVersion() => Task.Run(() => {
            if (_versionLookupTable.Count != 0) {
                List<LiteLoaderVersionManifest> manifestList = _versionLookupTable.Values.ToList();
                manifestList.Reverse();
                LiteLoaderVersionManifest latestLLVersion = manifestList.FirstOrDefault();
                if (latestLLVersion == null) {
                    _logger.AppendLine("Could not parse LiteLoader manifest! Check link; Contact support.");
                    return;
                }
                _logger.AppendLine($"Latest LiteLoader version found: \"{latestLLVersion.Version}\"");
                latestLLVersion = manifestList.First();
                if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, latestLLVersion.BDSVersion))) {
                    base.FetchBuild(latestLLVersion.BDSVersion).Wait();
                }
                if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.LLUpdatePackage_Ver, latestLLVersion.Version))) {
                    FetchBuild(latestLLVersion.Version).Wait();
                }

                (bool hasModule, MmsUrlKeys targetUrlPattern) verDetails = GetLiteLoaderVersionDetails(latestLLVersion.Version);
                if (verDetails.hasModule && !File.Exists(GetServiceFilePath(MmsFileNameKeys.LLModUpdatePackage_Ver, latestLLVersion.Version))) {
                    FetchBuild(latestLLVersion.Version).Wait();
                }
                _serviceConfiguration.SetServerDefaultPropList(_serverArch, MinecraftFileUtilities.GetDefaultPropListFromFile(GetServiceFilePath(MmsFileNameKeys.BedrockStockProps_Ver, latestLLVersion.BDSVersion)));
            }
        });

        public new List<Property> GetVersionPropList(string version) {
            return base.GetVersionPropList(GetBaseVersion(version));
        }

        public new List<Property> GetDefaultVersionPropList() {

            return base.GetDefaultVersionPropList();
        }

        public bool ValidateBuildExists(string version) => !File.Exists(GetServiceFilePath(MmsFileNameKeys.LLUpdatePackage_Ver, version)) && !File.Exists(GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, GetBaseVersion(version)));

        public new Task<bool> FetchBuild(string version) =>
            Task.Run(() => {
                if (!base.ValidateBuildExists(GetBaseVersion(version))) {
                    base.FetchBuild(GetBaseVersion(version)).Wait();
                }
                _logger.AppendLine($"Now downloading LiteLoader version {version}, please wait...");
                (bool hasModule, MmsUrlKeys targetUrlPattern) verDetails = GetLiteLoaderVersionDetails(version);
                string llFetchUrl = string.Format(MmsUrlStrings[verDetails.targetUrlPattern], version);
                string llZipPath = GetServiceFilePath(MmsFileNameKeys.LLUpdatePackage_Ver, version);
                string llModFetchUrl = string.Format(MmsUrlStrings[MmsUrlKeys.LLModPackage_Ver], version);
                string llModZipPath = GetServiceFilePath(MmsFileNameKeys.LLModUpdatePackage_Ver, version);
                new FileInfo(llZipPath).Directory.Create();
                new FileInfo(llModZipPath).Directory.Create();
                if (HTTPHandler.RetrieveFileFromUrl(llFetchUrl, llZipPath).Result) {
                    if (verDetails.hasModule) {
                        HTTPHandler.RetrieveFileFromUrl(llModFetchUrl, llModZipPath).Wait(); // If this doesn't exist, it's AIO.
                    }
                    return true;
                }
                return false;
            });

        private (bool hasModule, MmsUrlKeys targetUrlPattern) GetLiteLoaderVersionDetails(string inputVersion) {
            System.Version outputVer;
            string testVer;
            MmsUrlKeys targetUrlPattern = MmsUrlKeys.LLPackage_Ver;
            bool hasModule = false;
            if (inputVersion.Contains('-')) {
                testVer = inputVersion.Split('-')[0];
            } else {
                testVer = inputVersion;
            }
            if (inputVersion == "2.10.0-beta.1") {
                _logger.AppendLine("LL version 2.10.0-beta.x is not supported! MMS Will deploy 2.10.1 instead.");
                inputVersion = "2.10.1";
            }
            if (System.Version.TryParse(testVer, out outputVer)) {
                if (outputVer < System.Version.Parse("2.10.0")) {
                    targetUrlPattern = MmsUrlKeys.LLPackageOld_Ver;
                    hasModule = false;
                } else if (outputVer < System.Version.Parse("2.12.2")) {
                    targetUrlPattern = MmsUrlKeys.LLPackage_Ver;
                    hasModule = true;
                } else {
                    targetUrlPattern = MmsUrlKeys.LLPackage_Ver;
                    hasModule = false;
                }
            }
            return (hasModule, targetUrlPattern);
        }

        public static Task<LiteLoaderVersionManifest> GetLiteLoaderVersionManifest(string version) {
            return Task.Run(() => {
                int retryCount = 1;
                string result = string.Empty;
                string versionManifestPath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, "LiteLoader");
                while (result == string.Empty) {
                    try {
                        result = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.LLReleasesJson]).Result;
                    } catch {
                        if (retryCount > 2) {
                            if (File.Exists(versionManifestPath)) {
                                result = File.ReadAllText(versionManifestPath);
                                break;
                            }
                            return null;
                        }
                        retryCount++;
                    }
                }
                if (result != null) {
                    List<LiteLoaderVersionManifest> manifestList = JsonConvert.DeserializeObject<List<LiteLoaderVersionManifest>>(result);
                    return manifestList.Where(x => x.Version == version).FirstOrDefault();
                }
                return null;
            });
        }

        public new string GetBaseVersion(string modVersion) {
            return GetLiteLoaderVersionManifest(modVersion).Result.BDSVersion;
        }

        public async override Task ReplaceBuild(IServerConfiguration serverConfiguration, string versionOverride = "") {
            await Task.Run(() => {
                string version = versionOverride == "" ? serverConfiguration.GetServerVersion() : versionOverride;
                FileInfo originalExeInfo = new(GetServerFilePath(ServerFileNameKeys.VanillaBedrock, serverConfiguration));
                FileInfo mmsExeInfo = new($@"{serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                if (!mmsExeInfo.Directory.Exists) {
                    mmsExeInfo.Directory.Create();
                }
                LiteLoaderVersionManifest selectedVersion = GetLiteLoaderVersionManifest(version).Result;
                if (selectedVersion == null) {
                    selectedVersion = GetLiteLoaderVersionManifest(_serviceConfiguration.GetLatestVersion(MinecraftServerArch.LiteLoader)).Result;
                }
                string liteVersion = selectedVersion?.Version;
                base.ReplaceBuild(serverConfiguration, selectedVersion.BDSVersion).Wait();

                try {
                    if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.LLUpdatePackage_Ver, liteVersion))) {
                        if (!FetchBuild(liteVersion).Result) {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.zip\" and version was not found in LiteLoader manifest!");
                        }
                    }
                    Progress<ProgressModel> progress = new(percent => {
                        _logger.AppendLine($"Extracting LiteLoader files for server {serverConfiguration.GetServerName()}, {(int)percent.Progress}% completed...");
                    });
                    ZipUtilities.ExtractToDirectory(GetServiceFilePath(MmsFileNameKeys.LLUpdatePackage_Ver, liteVersion), GetServerDirectory(ServerDirectoryKeys.ServerRoot, serverConfiguration), progress).Wait();
                    if (File.Exists(GetServiceFilePath(MmsFileNameKeys.LLModUpdatePackage_Ver, liteVersion))) {
                        progress = new(percent => {
                            _logger.AppendLine($"Extracting LiteLoader Module files for server {serverConfiguration.GetServerName()}, {(int)percent.Progress}% completed...");
                        });
                        ZipUtilities.ExtractToDirectory(GetServiceFilePath(MmsFileNameKeys.LLModUpdatePackage_Ver, liteVersion), GetServerDirectory(ServerDirectoryKeys.ServerRoot, serverConfiguration) + "\\plugins", progress).Wait();
                    }
                    LiteLoaderPECore.BuildLLExe(serverConfiguration);
                    MinecraftFileUtilities.CreateDefaultLoaderConfigFile(serverConfiguration);
                    if (serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        serverConfiguration.SetServerVersion(liteVersion);
                    }
                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, serverConfiguration), liteVersion);
                } catch (IOException e) {
                    if (e.Message.Contains("because it is being used by another process.")) {
                        ProcessUtilities.KillProcessList(Process.GetProcessesByName(mmsExeInfo.Name[..^mmsExeInfo.Extension.Length]));
                        File.Copy(GetServerFilePath(ServerFileNameKeys.VanillaBedrock, serverConfiguration), GetServerFilePath(ServerFileNameKeys.MmsServer_Name, serverConfiguration, serverConfiguration.GetServerName()), true);
                    }
                }
                _logger.AppendLine($"Extraction of files for {serverConfiguration.GetServerName()} completed.");
            });
        }
        public new List<SimpleVersionModel> GetSimpleVersionList() {
            List<SimpleVersionModel> result = new List<SimpleVersionModel>();
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.LLReleasesJson]).Result;
            List<LiteLoaderVersionManifest> versionList = JsonConvert.DeserializeObject<List<LiteLoaderVersionManifest>>(content);
            versionList.Reverse();
            if (content == null)
                return new List<SimpleVersionModel>();
            versionList.Reverse();
            foreach (var version in versionList) {
                result.Add(new(version.Version, version.IsBeta.ToLower() == "true"));
            }
            return result;
        }

        public new void SetNewLogger(IServerLogger logger) => _logger = logger;
    }
}
