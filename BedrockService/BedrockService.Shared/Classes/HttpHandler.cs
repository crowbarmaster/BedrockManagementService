using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Shared.Classes {
    public class HTTPHandler {
        public static BedrockLogger Logger = new BedrockLogger();

        public static async Task<bool> RetrieveFileFromUrl(string url, string outputPath) {
            using HttpClient httpClient = new();
            using HttpRequestMessage request = new(HttpMethod.Get, url);
            using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
            using Stream stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true);
            try {
                if (contentStream.Length > 0) {
                    await contentStream.CopyToAsync(stream);
                } else {
                    if (stream != null) {
                        stream.Close();
                        stream.Dispose();
                        File.Delete(outputPath);
                    }
                    return false;
                }
            } catch (Exception e) {
                Logger.AppendErrorFromException(e);
                return false;
            }
            httpClient.Dispose();
            request.Dispose();
            contentStream.Dispose();
            return true;
        }

        public static async Task<string> FetchHTTPContent(string url, KeyValuePair<string, string> optionalHeader = new()) {
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
                Logger.AppendLine($"Error! could not fetch current webpage content!");
            } catch (TaskCanceledException) {
                return null;
            } catch (Exception e) {
                Logger.AppendLine($"FetchHTTPContent resulted in error: {e.Message}\n{e.InnerException}\n{e.StackTrace}");
            } finally {
                client.Dispose();
            }
            return null;
        }
    }
}
