﻿using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.JsonModels.MinecraftJsonModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes {
    public class Updater : IUpdater {
        private readonly IBedrockLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private string _version;
        private string _liteLoaderVersion;

        public Updater(IBedrockLogger logger, IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _version = "None";
        }

        public void Initialize() {
            if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.BedrockVersionIni))) {
                _logger.AppendLine("Version ini file missing, creating and fetching build...");
                File.Create(GetServiceFilePath(BmsFileNameKeys.BedrockVersionIni)).Close();
                return;
            }
            _version = File.ReadAllText(GetServiceFilePath(BmsFileNameKeys.BedrockVersionIni));
            _serviceConfiguration.SetLatestBDSVersion(_version);
            _liteLoaderVersion = _serviceConfiguration.GetProp(ServicePropertyKeys.LatestLiteLoaderVersion).ToString();
        }

        public Task CheckLatestVersion() {
            return Task.Run(() => {
                _logger.AppendLine("Checking latest LiteLoader Version...");
                CheckLiteLiteLoaderVersion();
                _logger.AppendLine("Checking latest BDS Version...");
                string content = HTTPHandler.FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.BdsVersionJson]).Result;
                if (content == null)
                    return false;
                List<MinecraftVersionHistoryJson> versionList = JsonSerializer.Deserialize<List<MinecraftVersionHistoryJson>>(content);
                MinecraftVersionHistoryJson latest = versionList.Last();

                string downloadPath = latest.WindowsBinUrl;
                string fetchedVersion = latest.Version;

                _logger.AppendLine($"Latest version found: \"{fetchedVersion}\"");
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, fetchedVersion))) {
                    FetchBuild(fetchedVersion).Wait();
                    MinecraftUpdatePackageProcessor packageProcessor = new(_logger, fetchedVersion, GetServiceDirectory(BmsDirectoryKeys.CoreFileBuild_Ver, fetchedVersion));
                    if (!packageProcessor.ExtractCoreFiles()) {
                        _logger.AppendLine("Error extracting downloaded zip package. Check file/website!");
                    }
                }
                File.WriteAllText(GetServiceFilePath(BmsFileNameKeys.BedrockVersionIni), fetchedVersion);
                _serviceConfiguration.SetLatestBDSVersion(fetchedVersion);
                if (!File.Exists(string.Format(GetServiceFilePath(BmsFileNameKeys.LLUpdatePackage_Ver), _liteLoaderVersion))) {
                    FetchLiteLoaderBuild(_liteLoaderVersion).Wait();
                }
                return true;
            });
        }

        public Task CheckLiteLiteLoaderVersion() => Task.Run(() => {
            string result = HTTPHandler.FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.LLReleasesJson]).Result;
            if (result != null) {
                int distanceFromEnd = 1;
                List<LiteLoaderVersionManifest> manifestList = JsonSerializer.Deserialize<List<LiteLoaderVersionManifest>>(result);
                string llUseBetaFlag = _serviceConfiguration.GetProp(ServicePropertyKeys.UseBetaLiteLoaderVersions).StringValue;
                LiteLoaderVersionManifest latestLLVersion = manifestList[^distanceFromEnd];
                while (latestLLVersion.IsBeta == "true" && llUseBetaFlag == "false") {
                    distanceFromEnd++;
                    latestLLVersion = manifestList[^distanceFromEnd];
                }
                _logger.AppendLine($"Latest LiteLoader version found: \"{latestLLVersion.Version}\"");
                _serviceConfiguration.SetProp(ServicePropertyKeys.LatestLiteLoaderVersion, latestLLVersion.Version);
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.LLUpdatePackage_Ver, latestLLVersion.Version)) || !File.Exists(GetServiceFilePath(BmsFileNameKeys.LLModUpdatePackage_Ver, latestLLVersion.Version))) {
                    FetchLiteLoaderBuild(latestLLVersion.Version).Wait();
                }
                _serviceConfiguration.SetProp(ServicePropertyKeys.LatestLiteLoaderVersion, latestLLVersion.Version);
                string verResult = HTTPHandler.FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.BdsVersionJson]).Result;
                if (verResult == null) {
                    verResult = "[]";
                }
                List<MinecraftVersionHistoryJson> versions = JsonSerializer.Deserialize<List<MinecraftVersionHistoryJson>>(verResult);
                foreach (MinecraftVersionHistoryJson entry in versions) {
                    if (entry.Version.Contains(latestLLVersion.BDSVersion)) { // This is a hack to match a partial version string to its full representation.
                        latestLLVersion.BDSVersion = entry.Version;
                    }
                }
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, latestLLVersion.BDSVersion))) {
                    FetchBuild(latestLLVersion.BDSVersion).Wait();
                }
                _serviceConfiguration.SetLatestLLVersion(latestLLVersion.Version);
            }
        });

        public Task<LiteLoaderVersionManifest> GetLiteLoaderVersionManifest(string version) {
            return Task.Run(() => {
                string result = HTTPHandler.FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.LLReleasesJson]).Result;
                if (result != null) {
                    List<LiteLoaderVersionManifest> manifestList = JsonSerializer.Deserialize<List<LiteLoaderVersionManifest>>(result);
                    return manifestList.Where(x => x.Version == version).First();
                }
                return null;
            });
        }

            public static Task<bool> FetchBuild(string version) {
            return Task.Run(() => {
                string fetchUrl = string.Format(BmsUrlStrings[BmsUrlKeys.BdsPackage_Ver], version);
                string zipPath = GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, version);
                new FileInfo(zipPath).Directory.Create();
                if (RetrieveFileFromUrl(fetchUrl, zipPath).Result) {
                    if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.StockProps, version))) {
                        MinecraftUpdatePackageProcessor packageProcessor = new(version, GetServiceDirectory(BmsDirectoryKeys.CoreFileBuild_Ver, version));
                        if (!packageProcessor.ExtractCoreFiles()) {
                            return false;
                        }
                        return true;
                    }
                }
                return false;
            });
        }

        private static async Task<bool> RetrieveFileFromUrl(string url, string outputPath) {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(), stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true);
            try {
                if (contentStream.Length > 2000) {
                    await contentStream.CopyToAsync(stream);
                } else {
                    if (stream != null) {
                        stream.Close();
                        stream.Dispose();
                        File.Delete(outputPath);
                    }
                    return false;

                }
            } catch (Exception) {
                return false;
            }
            httpClient.Dispose();
            request.Dispose();
            contentStream.Dispose();
            return true;
        }

        public static Task<bool> FetchLiteLoaderBuild(string version) {
            return Task.Run(() => {
                string llFetchUrl = string.Format(BmsUrlStrings[BmsUrlKeys.LLPackage_Ver], version);
                string llModFetchUrl = string.Format(BmsUrlStrings[BmsUrlKeys.LLModPackage_Ver], version);
                string llZipPath = GetServiceFilePath(BmsFileNameKeys.LLUpdatePackage_Ver, version);
                string llModZipPath = GetServiceFilePath(BmsFileNameKeys.LLModUpdatePackage_Ver, version);
                new FileInfo(llZipPath).Directory.Create();
                new FileInfo(llModZipPath).Directory.Create();
                if (RetrieveFileFromUrl(llFetchUrl, llZipPath).Result) {
                    RetrieveFileFromUrl(llModFetchUrl, llModZipPath).Wait(); // If this doesn't exist, it's AIO.
                    return true;
                }
                return false;
            });
        }
    }
}
