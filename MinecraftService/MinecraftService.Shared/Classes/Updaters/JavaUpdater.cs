using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;
using MinecraftService.Shared.Utilities;
using JavaVersionManifest = MinecraftService.Shared.JsonModels.MinecraftJsonModels.Version;
using Newtonsoft.Json;

namespace MinecraftService.Shared.Classes.Updaters {
    public class JavaUpdater : IUpdater {
        private IServerLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Java;

        public JavaUpdater(IServerLogger logger, IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public JavaUpdater(IServerLogger logger, IServiceConfiguration serviceConfiguration, IServerConfiguration serverConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _serverConfiguration = serverConfiguration;
            _logger = logger;
        }

        public void Initialize() {
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.Jdk17JavaVanillaExe))) {
                if (HTTPHandler.RetrieveFileFromUrl(MmsUrlStrings[MmsUrlKeys.Jdk17DownloadLink], "Jdk.zip").Result) {
                    Progress<ProgressModel> progress = new(percent => {
                        _logger.AppendLine($"Extracting JDK 17 for Java support, {percent.Progress}% completed...");
                    });
                    ZipUtilities.ExtractToDirectory("Jdk.zip", GetServiceDirectory(ServiceDirectoryKeys.Jdk17Path), progress).Wait();
                    File.Copy(GetServiceFilePath(MmsFileNameKeys.Jdk17JavaVanillaExe), GetServiceFilePath(MmsFileNameKeys.Jdk17JavaMmsExe));
                    File.Delete("Jdk.zip");
                }
            }
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]))) {
                _logger.AppendLine("Version ini file missing, creating...");
                File.Create(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch])).Close();
                CheckLatestVersion().Wait();
                return;
            }
            if (_serverConfiguration == null) {
                CheckLatestVersion().Wait();
            }
            _logger.Initialize();
        }

        public virtual Task CheckLatestVersion() {
            return Task.Run(() => {
                _logger.AppendLine("Checking latest Java Release version...");
                int retryCount = 1;
                string content = string.Empty;
                string versionManifestPath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString());
                string javaReleaseManifestPath = GetServiceFilePath(MmsFileNameKeys.JavaLatestReleaseManifest, _serverArch.ToString());
                string javaBetaManifestPath = GetServiceFilePath(MmsFileNameKeys.JavaLatestBetaManifest, _serverArch.ToString());
                while (content == string.Empty) {
                    try {
                        content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
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
                JavaVersionHistoryModel versionList = JsonConvert.DeserializeObject<JavaVersionHistoryModel>(content);
                JavaVersionDetailsModel releaseDetails = new JavaVersionDetailsModel();
                JavaVersionDetailsModel betaDetails = new JavaVersionDetailsModel();
                JavaVersionManifest latestRelease = versionList.Versions.FirstOrDefault(x => x.Id == versionList.Latest.Release);
                JavaVersionManifest latestBeta = versionList.Versions.FirstOrDefault(x => x.Id == versionList.Latest.Snapshot);
                content = string.Empty;
                retryCount = 1;
                if (latestRelease != null) {
                    while (content == string.Empty) {
                        try {
                            content = HTTPHandler.FetchHTTPContent(latestRelease.Url).Result;
                            File.WriteAllText(javaReleaseManifestPath, content);
                        } catch {
                            if (retryCount > 2) {
                                if (File.Exists(javaReleaseManifestPath)) {
                                    _logger.AppendLine($"Attempt to fetch {_serverArch} java release manifest failed. Using previously stored copy!");
                                    content = File.ReadAllText(javaReleaseManifestPath);
                                    break;
                                } else {
                                    _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                                    return;
                                }
                            }
                            _logger.AppendLine($"Attempt to fetch {_serverArch} java release manifest failed... retry {retryCount} of 2");
                            retryCount++;
                        }
                    }
                    releaseDetails = JsonConvert.DeserializeObject<JavaVersionDetailsModel>(content);
                }
                content = string.Empty;
                retryCount = 1;
                if (latestBeta != null) {
                    while (content == string.Empty) {
                        try {
                            content = HTTPHandler.FetchHTTPContent(latestBeta.Url).Result;
                            File.WriteAllText(javaBetaManifestPath, content);
                        } catch {
                            if (retryCount > 2) {
                                if (File.Exists(javaBetaManifestPath)) {
                                    _logger.AppendLine($"Attempt to fetch {_serverArch} java beta manifest failed. Using previously stored copy!");
                                    content = File.ReadAllText(javaBetaManifestPath);
                                    break;
                                } else {
                                    _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                                    return;
                                }
                            }
                            _logger.AppendLine($"Attempt to fetch {_serverArch} java beta manifest failed... retry {retryCount} of 2");
                            retryCount++;
                        }
                    }
                    betaDetails = JsonConvert.DeserializeObject<JavaVersionDetailsModel>(content); 
                }
                if (releaseDetails == null) {
                    return;
                }
                if (betaDetails != null) {
                    _logger.AppendLine($"Latest beta version found: \"{betaDetails.Id}\"");
                }
                string downloadPath = releaseDetails.Downloads.Server.Url;
                string fetchedVersion = releaseDetails.Id;

                _logger.AppendLine($"Latest release version found: \"{fetchedVersion}\"");
                if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, fetchedVersion))) {
                    FetchBuild(releaseDetails).Wait();
                }
                ProcessJdsPackage(releaseDetails).Wait();
                File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]), fetchedVersion);
                _serviceConfiguration.SetLatestVersion(_serverArch, fetchedVersion);
                _serviceConfiguration.SetServerDefaultPropList(_serverArch, MinecraftFileUtilities.GetDefaultPropListFromFile(GetServiceFilePath(MmsFileNameKeys.JavaStockProps_Ver, fetchedVersion)));
            });
        }

        public static Task ProcessJdsPackage(JavaVersionDetailsModel version) {
            return Task.Run(() => {
                string propFile = GetServiceFilePath(MmsFileNameKeys.JavaStockProps_Ver, version.Id);
                FileInfo file = new(propFile);
                if (file.Exists && file.Length != 0) {
                    return;
                }
                FileInfo jarFile = new(GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version.Id));
                if (!jarFile.Exists) {
                    return;
                }
                ProcessUtilities.QuickLaunchJar($"{file.Directory.FullName}\\{jarFile.Name}").Wait();
            });
        }

        public static JavaVersionDetailsModel GetJavaVersionModel(string version) {
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
            if (content == null)
                return null;
            JavaVersionHistoryModel versionList = JsonConvert.DeserializeObject<JavaVersionHistoryModel>(content);
            JavaVersionDetailsModel releaseDetails = new JavaVersionDetailsModel();
            JavaVersionManifest selectedRelease = versionList.Versions.FirstOrDefault(x => x.Id == version);
            if (selectedRelease != null) {
                content = HTTPHandler.FetchHTTPContent(selectedRelease.Url).Result;
                releaseDetails = JsonConvert.DeserializeObject<JavaVersionDetailsModel>(content);
            }
            return releaseDetails;
        }

        public virtual Task<bool> FetchBuild(string version) {
            return Task.Run(() => {
                JavaVersionDetailsModel selectedVersion = GetJavaVersionModel(version);
                if (selectedVersion != null) {
                    FetchBuild(selectedVersion);
                    return true;
                }
                return false;
            });
        }

        public virtual Task<bool> FetchBuild(JavaVersionDetailsModel version) {
            return Task.Run(() => {
                _logger.AppendLine($"Now downloading Java version {version.Id}, please wait...");
                string zipPath = GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version.Id);
                if (HTTPHandler.RetrieveFileFromUrl(version.Downloads.Server.Url, zipPath).Result) {
                    return true;
                }
                return false;
            });
        }

        public virtual async Task ReplaceServerBuild(string versionOverride = "") {
            await Task.Run(() => {
                if (_serverConfiguration == null) {
                    return;
                }
                string exeName = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue;
                ProcessUtilities.KillJarProcess(exeName);
                string version = versionOverride == "" ? _serverConfiguration.GetServerVersion() : versionOverride;
                FileInfo originalExeInfo = new(GetServerFilePath(ServerFileNameKeys.VanillaJava, _serverConfiguration));
                FileInfo mmsExeInfo = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                try {
                    if (!mmsExeInfo.Directory.Exists) {
                        mmsExeInfo.Directory.Create();
                    }
                    MinecraftFileUtilities.CleanJavaServerDirectory(_serverConfiguration);
                    string filePath = GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version);
                    if (!File.Exists(filePath)) {
                        if (!FetchBuild(version).Result) {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.jar\" and version was not found in JDS manifest!");
                        }
                    }
                    File.Copy(filePath, mmsExeInfo.FullName, true);
                    MinecraftFileUtilities.WriteJavaEulaFile(_serverConfiguration);

                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, _serverConfiguration), version);
                    if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        _serverConfiguration.SetServerVersion(version);
                    }

                } catch (Exception e) {
                    if(e.GetType() == typeof(InvalidDataException)) {
                        throw new FileNotFoundException($"Build file \"Update_{version}.zip\" found corrupt. Service cannot proceed!!");
                    } else {
                        _logger.AppendLine($"ReplaceServerBuild resulted in error: {e.Message}\n{e.StackTrace}");
                    }
                }
            });
        }

        public string GetBaseVersion(string version) {
            return version;
        }

        public List<SimpleVersionModel> GetVersionList() {
            List<SimpleVersionModel> result = new List<SimpleVersionModel>();
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
            if (content == null)
                return new List<SimpleVersionModel>();
            JavaVersionHistoryModel versionList = JsonConvert.DeserializeObject<JavaVersionHistoryModel>(content);
            versionList.Versions.Reverse();
            foreach (var version in versionList.Versions) {
                result.Add(new(version.Id, version.Type != "release"));
            }
            return result;
        }

        public void SetNewLogger(IServerLogger logger) => _logger = logger;
    }
}
