using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BedrockService.Service.Networking
{
    public static class Updater
    {
        public static bool VersionChanged = false;
        public static string[] FileList;

        public static async Task<bool> CheckUpdates()
        {
            InstanceProvider.GetServiceLogger().AppendLine("Checking MCS Version and fetching update if needed...");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Pragma", "no-cache");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko; Google Page Speed Insights) Chrome/27.0.1453 Safari/537.36");
            client.Timeout = new TimeSpan(0, 0, 2);

            string content = FetchHTTPContent(client).Result;
            if (content == null) // This really doesn't fail often. Give it one more try or fail.
            {
                Thread.Sleep(500);
                content = await FetchHTTPContent(client);
            }
            if (content == null)
                return false;
            string pattern = @"(https://minecraft.azureedge.net/bin-win/bedrock-server-)(.*)(\.zip)";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = regex.Match(content);
            if (!m.Success)
            {
                InstanceProvider.GetServiceLogger().AppendLine("Checking for updates failed. Check website functionality!");
                return false;
            }
            string downloadPath = m.Groups[0].Value;
            string version = m.Groups[2].Value;
            client.Dispose();

            if (File.Exists($@"{Program.ServiceDirectory}\Server\bedrock_ver.ini"))
            {
                string LocalVer = File.ReadAllText($@"{Program.ServiceDirectory}\Server\bedrock_ver.ini");
                if (LocalVer != version)
                {
                    InstanceProvider.GetServiceLogger().AppendLine($"New version detected! Now fetching from {downloadPath}...");
                    VersionChanged = true;
                    FetchBuild(downloadPath, version).Wait();
                    File.WriteAllText($@"{Program.ServiceDirectory}\Server\bedrock_ver.ini", version);
                }
                return true;
            }
            else
            {
                InstanceProvider.GetServiceLogger().AppendLine("Version ini file missing, fetching build to recreate...");
                FetchBuild(downloadPath, version).Wait();
                File.WriteAllText($@"{Program.ServiceDirectory}\Server\bedrock_ver.ini", version);
                return true;
            }
        }

        private static async Task<string> FetchHTTPContent(HttpClient client)
        {
            try
            {
                return await client.GetStringAsync("https://www.minecraft.net/en-us/download/server/bedrock");
            }
            catch (HttpRequestException e)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"Error! Updater timed out, could not fetch current build!");
            }
            catch (Exception e)
            {
                InstanceProvider.GetServiceLogger().AppendLine($"Updater resulted in error: {e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
            return null;
        }

        public static async Task FetchBuild(string path, string version)
        {
            string ZipDir = $@"{Program.ServiceDirectory}\Server\MCSFiles\Update_{version}.zip";
            if (!Directory.Exists($@"{Program.ServiceDirectory}\Server\MCSFiles"))
            {
                Directory.CreateDirectory($@"{Program.ServiceDirectory}\Server\MCSFiles");
            }
            if (File.Exists(ZipDir))
            {
                return;
            }
            if (InstanceProvider.GetHostInfo().GetGlobalValue("AcceptedMojangLic") == "false")
            {
                InstanceProvider.GetServiceLogger().AppendLine("------First time download detected------\n");
                InstanceProvider.GetServiceLogger().AppendLine("You will need to agree to the Minecraft End User License Agreement");
                InstanceProvider.GetServiceLogger().AppendLine("in order to continue. Visit https://account.mojang.com/terms");
                InstanceProvider.GetServiceLogger().AppendLine("to view terms. Type \"Yes\" and press enter to confirm that");
                InstanceProvider.GetServiceLogger().AppendLine("you agree to said terms.");
                Console.Write("Do you agree to the terms? ");
                Console.Out.Flush();
                if (Console.ReadLine() != "Yes")
                {
                    return;
                }
                InstanceProvider.GetHostInfo().SetGlobalProperty("AcceptedMojangLic", "true");
                InstanceProvider.GetConfigManager().SaveGlobalFile();
                InstanceProvider.GetServiceLogger().AppendLine("Now downloading latest build of Minecraft Bedrock Server. Please wait...");
            }
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, path))
                {
                    using (Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(), stream = new FileStream(ZipDir, FileMode.Create, FileAccess.Write, FileShare.None, 256000, true))
                    {
                        try
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                        catch (Exception e)
                        {
                            InstanceProvider.GetServiceLogger().AppendLine($"Download zip resulted in error: {e.StackTrace}");
                        }
                        httpClient.Dispose();
                        request.Dispose();
                        contentStream.Dispose();
                    }
                }
            }
        }
    }
}
