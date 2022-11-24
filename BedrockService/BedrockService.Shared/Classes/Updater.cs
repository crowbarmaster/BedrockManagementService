using BedrockService.Shared.Interfaces;
using BedrockService.Shared.JsonModels.GitHubApiJsonModels;
using BedrockService.Shared.JsonModels.MinecraftJsonModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
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
            string[] LLVersion = _serviceConfiguration.GetProp(ServicePropertyKeys.LatestLiteLoaderVersion).ToString().Split('|');
            _liteLoaderVersion = LLVersion[1];
        }

        public Task CheckLatestVersion() {
            return Task.Run(() => {
                _logger.AppendLine("Checking latest LiteLoader Version...");

                _logger.AppendLine("Checking latest BDS Version...");
                string content = FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.BdsDownloadPage]).Result;
                if (content == null)
                    return false;
                Regex regex = new(BmsUrlStrings[BmsUrlKeys.BdsVersionRegx], RegexOptions.IgnoreCase);
                Match m = regex.Match(content);
                if (!m.Success) {
                    _logger.AppendLine("Checking version failed. Check website functionality!");
                    return false;
                }
                string downloadPath = m.Groups[0].Value;
                string fetchedVersion = m.Groups[2].Value;

                _logger.AppendLine($"Latest version found: \"{fetchedVersion}\"");
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, fetchedVersion))) {
                    FetchBuild(fetchedVersion).Wait();
                    MinecraftUpdatePackageProcessor packageProcessor = new(_logger, fetchedVersion, GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, fetchedVersion));
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
            CheckLatestVersion().Wait();
            string result = FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.LLReleasesJson], new KeyValuePair<string, string>("Accept", "application/vnd.github+json")).Result;
            if (result != null) {
                GitHubReleaseListJsonModel releases = JsonSerializer.Deserialize<List<GitHubReleaseListJsonModel>>(result)[0];
                Regex regex = new(BmsUrlStrings[BmsUrlKeys.LLBdsVersionRegx], RegexOptions.IgnoreCase);
                Match match = regex.Match(releases.body);
                string bdsVersion = string.Empty;
                if (!match.Success) {
                    _logger.AppendLine("Regex did not match latest version. Using latest Minecraft version.");
                    bdsVersion = _serviceConfiguration.GetLatestBDSVersion();

                } else {
                    bdsVersion = match.Groups[2].Value;
                }
                string llVersion = releases.tag_name;
                if (!string.IsNullOrEmpty(llVersion) && !File.Exists(GetServiceFilePath(BmsFileNameKeys.LLUpdatePackage_Ver, llVersion))) {
                    FetchLiteLoaderBuild(llVersion).Wait();
                }

                List<MinecraftVersionHistoryJson> versions = JsonSerializer.Deserialize<List<MinecraftVersionHistoryJson>>(FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.BdsVersionJson]).Result);
                foreach (MinecraftVersionHistoryJson entry in versions) {
                    if (entry.Version.Contains(bdsVersion)) {
                        bdsVersion = entry.Version;
                    }
                }
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, bdsVersion))) {
                    FetchBuild(bdsVersion).Wait();
                }
            }
        });

        public static async Task<bool> FetchBuild(string version) {
            string fetchUrl = string.Format(BmsUrlStrings[BmsUrlKeys.BdsPackage_Ver], version);
            string zipPath = GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, version);
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(HttpMethod.Get, fetchUrl);
            DirectoryInfo zipDirInfo = new FileInfo(zipPath).Directory;
            zipDirInfo.Create();
            using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(), stream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true);
            try {
                if (contentStream.Length > 2000) {
                    await contentStream.CopyToAsync(stream);
                } else {
                    if (stream != null) {
                        stream.Close();
                        stream.Dispose();
                        File.Delete(zipPath);
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

        public static async Task<bool> FetchLiteLoaderBuild(string version) {
            string fetchUrl = string.Format(BmsUrlStrings[BmsUrlKeys.LLPackage_Ver], version);
            string zipPath = GetServiceFilePath(BmsFileNameKeys.LLUpdatePackage_Ver, version);
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(HttpMethod.Get, fetchUrl);
            DirectoryInfo zipDirInfo = new FileInfo(zipPath).Directory;
            zipDirInfo.Create();
            using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(), stream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true);
            try {
                if (contentStream.Length > 1000) {
                    await contentStream.CopyToAsync(stream);
                } else {
                    if (stream != null) {
                        stream.Close();
                        stream.Dispose();
                        File.Delete(zipPath);
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

        private async Task<string> FetchHTTPContent(string url, KeyValuePair<string, string> optionalHeader = new()) {
            HttpClient client = new();
            try {
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/apng,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko; Google Page Speed Insights) Chrome/27.0.1453 Safari/537.36");
                if (!string.IsNullOrEmpty(optionalHeader.Key)) {
                    if (client.DefaultRequestHeaders.Contains(optionalHeader.Key) && !string.IsNullOrEmpty(optionalHeader.Value))
                        client.DefaultRequestHeaders.Remove(optionalHeader.Key);
                    client.DefaultRequestHeaders.Add(optionalHeader.Key, optionalHeader.Value);
                }
                client.Timeout = new TimeSpan(0, 0, 3);
                return await client.GetStringAsync(url);
            } catch (HttpRequestException) {
                _logger.AppendLine($"Error! could not fetch current webpage content!");
            } catch (TaskCanceledException) {
                return null;
            } catch (Exception e) {
                _logger.AppendLine($"Updater resulted in error: {e.Message}\n{e.InnerException}\n{e.StackTrace}");
            } finally {
                client.Dispose();
            }
            return null;
        }
    }
}
