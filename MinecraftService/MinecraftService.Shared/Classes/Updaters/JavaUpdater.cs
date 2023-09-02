using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;
using MinecraftService.Shared.Utilities;
using System.Diagnostics;
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
                    Progress<double> progress = new(percent => {
                        _logger.AppendLine($"Extracting JDK 17 for Java support, {percent}% completed...");
                    });
                    FileUtilities.ExtractZipToDirectory("Jdk.zip", GetServiceDirectory(ServiceDirectoryKeys.Jdk17Path), progress).Wait();
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
                string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
                if (content == null)
                    return;
                JavaVersionHistoryModel versionList = JsonConvert.DeserializeObject<JavaVersionHistoryModel>(content);
                JavaVersionDetailsModel releaseDetails = new JavaVersionDetailsModel();
                JavaVersionDetailsModel betaDetails = new JavaVersionDetailsModel();
                JavaVersionManifest latestRelease = versionList.Versions.FirstOrDefault(x => x.Id == versionList.Latest.Release);
                JavaVersionManifest latestBeta = versionList.Versions.FirstOrDefault(x => x.Id == versionList.Latest.Snapshot);
                if (latestRelease != null) {
                    content = HTTPHandler.FetchHTTPContent(latestRelease.Url).Result;
                    releaseDetails = JsonConvert.DeserializeObject<JavaVersionDetailsModel>(content);
                }
                if (latestBeta != null) {
                    content = HTTPHandler.FetchHTTPContent(latestBeta.Url).Result;
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

        private Task ProcessJdsPackage(JavaVersionDetailsModel version) {
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

        private JavaVersionDetailsModel GetJavaVersionModel(string version) {
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
            if (content == null)
                return null;
            JavaVersionHistoryModel versionList = JsonConvert.DeserializeObject<JavaVersionHistoryModel>(content);
            JavaVersionDetailsModel releaseDetails = new JavaVersionDetailsModel();
            JavaVersionManifest selectedRelease = versionList.Versions.FirstOrDefault(x => x.Id == version);
            if (selectedRelease != null) {
                content = HTTPHandler.FetchHTTPContent(selectedRelease.Url).Result;
                releaseDetails = JsonConvert.DeserializeObject<JavaVersionDetailsModel>(content);
            } else {
                _logger.AppendLine("Selected Java version does not exist! Please check server config.");
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
                    Progress<double> progress = new(percent => {
                        _logger.AppendLine($"Extracting Java files for server {_serverConfiguration.GetServerName()}, {percent}% completed...");
                    });
                    File.Copy(filePath, mmsExeInfo.FullName, true);
                    MinecraftFileUtilities.WriteJavaEulaFile(_serverConfiguration);

                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, _serverConfiguration), version);
                    if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        _serverConfiguration.SetServerVersion(version);
                    }

                } catch (InvalidDataException) {
                    throw new FileNotFoundException($"Build file \"Update_{version}.zip\" found corrupt. Service cannot proceed!!");
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
