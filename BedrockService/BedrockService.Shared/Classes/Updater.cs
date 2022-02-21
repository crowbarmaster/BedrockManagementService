using BedrockService.Shared.Interfaces;
using BedrockService.Shared.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public class Updater : IUpdater {
        private bool _versionChanged = false;
        private readonly IBedrockLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IProcessInfo _processInfo;
        private string _version;

        public Updater(IProcessInfo processInfo, IBedrockLogger logger, IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _processInfo = processInfo;
            _logger = logger;
            _version = "None";
        }

        public void Initialize() {
            if (!Directory.Exists($@"{_processInfo.GetDirectory()}\BmsConfig")) { 
                Directory.CreateDirectory($@"{_processInfo.GetDirectory()}\BmsConfig"); 
            }
            if (!File.Exists($@"{_processInfo.GetDirectory()}\BmsConfig\latest_bedrock_ver.ini")) {
                _logger.AppendLine("Version ini file missing, creating and fetching build...");
                File.Create($@"{_processInfo.GetDirectory()}\BmsConfig\latest_bedrock_ver.ini").Close();
                return;
            }
            _version = File.ReadAllText($@"{_processInfo.GetDirectory()}\BmsConfig\latest_bedrock_ver.ini");
            _serviceConfiguration.SetLatestBDSVersion(_version);
        }

        public async Task CheckLatestVersion() {
            await Task.Run(async () => {
                _logger.AppendLine("Checking latest BDS Version...");
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/apng,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko; Google Page Speed Insights) Chrome/27.0.1453 Safari/537.36");
                client.Timeout = new TimeSpan(0, 0, 3);
                string content = FetchHTTPContent(client).Result;
                if (content == null) // This really doesn't fail often. Give it one more try or fail.
                {
                    Thread.Sleep(500);
                    content = await FetchHTTPContent(client);
                }
                if (content == null)
                    return false;
                Regex regex = new Regex(@"(https://minecraft.azureedge.net/bin-win/bedrock-server-)(.*)(\.zip)", RegexOptions.IgnoreCase);
                Match m = regex.Match(content);
                if (!m.Success) {
                    _logger.AppendLine("Checking version failed. Check website functionality!");
                    return false;
                }
                string downloadPath = m.Groups[0].Value;
                string fetchedVersion = m.Groups[2].Value;
                client.Dispose();

                _logger.AppendLine($"Latest version found: \"{fetchedVersion}\"");
                if (!File.Exists($@"{_processInfo.GetDirectory()}\BmsConfig\BDSBuilds\BuildArchives\Update_{fetchedVersion}.zip")) {
                    FetchBuild(_processInfo.GetDirectory(), fetchedVersion).Wait();
                }
                File.WriteAllText($@"{_processInfo.GetDirectory()}\BmsConfig\latest_bedrock_ver.ini", fetchedVersion);
                _serviceConfiguration.SetLatestBDSVersion(fetchedVersion);
                return true;
            });
        }

        public bool CheckVersionChanged() => _versionChanged;

        public void MarkUpToDate() => _versionChanged = false;

        public static async Task<bool> FetchBuild(string servicePath, string version) {
            string fetchUrl = $"https://minecraft.azureedge.net/bin-win/bedrock-server-{version}.zip";
            string zipPath = $@"{servicePath}\BmsConfig\BDSBuilds\BuildArchives\Update_{version}.zip";
            if (!Directory.Exists($@"{servicePath}\BmsConfig\BDSBuilds")) {
                Directory.CreateDirectory($@"{servicePath}\BmsConfig\BDSBuilds");
            }
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(HttpMethod.Get, fetchUrl)) {
                    DirectoryInfo zipDirInfo = new FileInfo(zipPath).Directory;
                    zipDirInfo.Create();
                    using (Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(), stream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true)) {
                        try {
                            if (contentStream.Length > 2000) {
                                await contentStream.CopyToAsync(stream);
                            } else {
                                if(stream != null) {
                                    stream.Close();
                                    stream.Dispose();
                                    File.Delete(zipPath);
                                }
                                return false;

                            }
                        } catch (Exception e) {
                            return false;
                        }
                        httpClient.Dispose();
                        request.Dispose();
                        contentStream.Dispose();
                        return true;
                    }
                }
            }
        }

        private async Task<string> FetchHTTPContent(HttpClient client) {
            try {
                return await client.GetStringAsync("https://www.minecraft.net/en-us/download/server/bedrock");
            } catch (HttpRequestException) {
                _logger.AppendLine($"Error! could not fetch current webpage content!");
            } catch (TaskCanceledException) {
                Thread.Sleep(200);
                return await FetchHTTPContent(client);
            } catch (Exception e) {
                _logger.AppendLine($"Updater resulted in error: {e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
            return null;
        }
    }
}
