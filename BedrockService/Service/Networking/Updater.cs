using BedrockService.Service.Networking.Interfaces;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BedrockService.Service.Networking {
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
            if (!Directory.Exists($@"{_processInfo.GetDirectory()}\Server")) { Directory.CreateDirectory($@"{_processInfo.GetDirectory()}\Server"); }
            if (!File.Exists($@"{_processInfo.GetDirectory()}\Server\bedrock_ver.ini")) {
                _logger.AppendLine("Version ini file missing, creating and fetching build...");
                File.Create($@"{_processInfo.GetDirectory()}\Server\bedrock_ver.ini").Close();
                return;
            }
            _version = File.ReadAllText($@"{_processInfo.GetDirectory()}\Server\bedrock_ver.ini");
        }

        public async Task CheckUpdates() {
            await Task.Run(async () => {
                if (_serviceConfiguration.GetServerVersion() != null) {
                    _version = _serviceConfiguration.GetServerVersion();
                }
                _logger.AppendLine("Checking MCS Version and fetching update if needed...");
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
                    _logger.AppendLine("Checking for updates failed. Check website functionality!");
                    return false;
                }
                string downloadPath = m.Groups[0].Value;
                string fetchedVersion = m.Groups[2].Value;
                client.Dispose();
                if (_version == fetchedVersion) {
                    _logger.AppendLine($"Current version \"{fetchedVersion}\" is up to date!");
                    return true;
                }
                _logger.AppendLine($"New version detected! Now fetching from {downloadPath}...");
                if (!FetchBuild(downloadPath, fetchedVersion).Wait(90000)) {
                    _logger.AppendLine("Fetching build timed out. If this is a new service instance, please restart service!");
                    return false;
                }
                _versionChanged = true;
                File.WriteAllText($@"{_processInfo.GetDirectory()}\Server\bedrock_ver.ini", fetchedVersion);
                _serviceConfiguration.SetServerVersion(fetchedVersion);
                return true;
            });
        }

        public bool CheckVersionChanged() => _versionChanged;

        public void MarkUpToDate() => _versionChanged = false;

        private async Task FetchBuild(string path, string version) {
            string ZipDir = $@"{_processInfo.GetDirectory()}\Server\MCSFiles\Update_{version}.zip";
            if (!Directory.Exists($@"{_processInfo.GetDirectory()}\Server\MCSFiles")) {
                Directory.CreateDirectory($@"{_processInfo.GetDirectory()}\Server\MCSFiles");
            }
            if (File.Exists(ZipDir)) {
                return;
            }
            _logger.AppendLine("Now downloading latest build of Minecraft Bedrock Server. Please wait...");
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(HttpMethod.Get, path)) {
                    using (Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(), stream = new FileStream(ZipDir, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true)) {
                        try {
                            await contentStream.CopyToAsync(stream);
                        } catch (Exception e) {
                            _logger.AppendLine($"Download zip resulted in error: {e.StackTrace}");
                        }
                        httpClient.Dispose();
                        request.Dispose();
                        contentStream.Dispose();
                        return;
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
