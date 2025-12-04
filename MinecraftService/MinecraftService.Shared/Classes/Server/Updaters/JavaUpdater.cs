using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.Service;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Server.Updaters {
    public class JavaUpdater : IUpdater {
        private MmsLogger _logger;
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Java;
        private bool _isInitialized = false;
        private readonly string _jsonFilePath = string.Empty;
        private readonly string _iniFilePath = string.Empty;
        private Dictionary<string, JavaVersionHistoryModel> _versionLookupTable = new();

        public JavaUpdater(MmsLogger logger, ServiceConfigurator serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _jsonFilePath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString());
            _iniFilePath = GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, _serverArch.ToString());
        }

        [JsonConstructor]
        public JavaUpdater() { }

        public Task Initialize() => Task.Run(() => {
            UpdateVersionTable();
            if (!File.Exists(_iniFilePath)) {
                _logger.AppendLine("Version ini file missing, creating...");
                File.Create(_iniFilePath).Close();
                CheckLatestVersion().Wait();
                _isInitialized = true;
                return;
            }
        });

        private void LoadLocalManifestContent() {
            if (File.Exists(_jsonFilePath)) {
                _versionLookupTable.Clear();
                try {
                    List<JavaVersionHistoryModel> versionList = JsonConvert.DeserializeObject<List<JavaVersionHistoryModel>>(File.ReadAllText(_jsonFilePath));
                    foreach (JavaVersionHistoryModel versionHistoryModel in versionList) {
                        _versionLookupTable.Add(versionHistoryModel.Version, versionHistoryModel);
                    }
                } catch {

                }
            }
        }

        private void UpdateVersionTable() {
            int retryCount = 1;
            string content = string.Empty;
            _logger.AppendLine($"Checking latest {_serverArch} version...");
            LoadLocalManifestContent();
            while (content == string.Empty) {
                try {
                    content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsLatestIni]).Result;
                } catch {
                    if (retryCount > 2) {
                        if (File.Exists(_iniFilePath)) {
                            _logger.AppendLine($"Attempt to fetch {_serverArch} version file failed. Using previously stored version!");
                            content = File.ReadAllText(_iniFilePath);
                            break;
                        }
                        _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                        return;
                    }
                    _logger.AppendLine($"Attempt to fetch {_serverArch} latest version failed... retry {retryCount} of 2");
                    retryCount++;
                }
            }
            if (content != File.ReadAllText(_iniFilePath)) {
                File.WriteAllText(_iniFilePath, content);
                content = string.Empty;
                retryCount = 1;
                _logger.AppendLine($"New {_serverArch} version was found! Fetching latest manifest file...");
                while (content == string.Empty) {
                    try {
                        content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
                        File.WriteAllText(_jsonFilePath, content);
                    } catch {
                        if (retryCount > 2) {
                            if (File.Exists(_jsonFilePath)) {
                                _logger.AppendLine($"Attempt to fetch {_serverArch} version manifest failed. Using previously stored copy!");
                                content = File.ReadAllText(_jsonFilePath);
                                break;
                            }
                            _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                            return;
                        }
                        _logger.AppendLine($"Attempt to fetch {_serverArch} version manifest failed... retry {retryCount} of 2");
                        retryCount++;
                    }
                }
                LoadLocalManifestContent();
            }
        }

        public bool IsInitialized() => _isInitialized;

        public virtual Task CheckLatestVersion() {
            return Task.Run(() => {
                _logger.AppendLine("Fetching latest Java Release version manifest...");
                UpdateVersionTable();
                List<JavaVersionHistoryModel> versionList = [.. _versionLookupTable.Values];
                versionList.Reverse();
                JavaVersionHistoryModel latestRelease = versionList.First(x => !x.IsBeta);
                JavaVersionHistoryModel latestBeta = versionList.First(x => x.IsBeta);

                _logger.AppendLine($"Latest Java release version found: \"{latestRelease.Version}\"");
                File.WriteAllText(_iniFilePath, latestRelease.Version);
                _serviceConfiguration.SetServerDefaultPropList(_serverArch, MinecraftFileUtilities.CreatePropListFromFile(GetServiceFilePath(MmsFileNameKeys.JavaStockProps_Ver, latestRelease.Version)));
            });
        }

        private JavaVersionHistoryModel GetJavaVersionModel(string version) {
            return _versionLookupTable[version];
        }

        public Task<bool> FetchBuild(string version) {
            return Task.Run(() => {
                JavaVersionHistoryModel selectedVersion = GetJavaVersionModel(version);
                if (selectedVersion != null) {
                    FetchBuild(selectedVersion).Wait();
                    return true;
                }
                return false;
            });
        }

        public List<Property> GetVersionPropList(string version) {
            List<Property> outList = new List<Property>();
            foreach (PropInfoEntry prop in _versionLookupTable[version].PropList) {
                outList.Add(new Property(prop.Key, prop.Value));
            }
            return outList;
        }

        public List<Property> GetDefaultVersionPropList() {
            List<Property> outList = new List<Property>();
            JavaVersionHistoryModel verModel = _versionLookupTable.Last(x => !x.Value.IsBeta).Value;
            foreach (PropInfoEntry prop in verModel.PropList) {
                outList.Add(new Property(prop.Key, prop.Value));
            }
            return outList;
        }

        public bool ValidateBuildExists(string version) => !File.Exists(GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version));

        public virtual Task<bool> FetchBuild(JavaVersionHistoryModel version) {
            return Task.Run(() => {
                _logger.AppendLine($"Now downloading Java version {version.Version}, please wait...");
                string zipPath = GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version.Version);
                if (HTTPHandler.RetrieveFileFromUrl(version.DownloadUrl, zipPath).Result) {
                    return true;
                }
                return false;
            });
        }

        public virtual async Task ReplaceBuild(IServerConfiguration serverConfiguration, string versionOverride = "") {
            await Task.Run(() => {
                ValidateJavaInstallation(_logger).Wait();
                if (serverConfiguration == null) {
                    return;
                }
                string exeName = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue;
                ProcessUtilities.KillJarProcess(exeName);
                string version = versionOverride == "" ? serverConfiguration.GetServerVersion() : versionOverride;
                FileInfo originalExeInfo = new(GetServerFilePath(ServerFileNameKeys.VanillaJava, serverConfiguration));
                FileInfo mmsExeInfo = new($@"{serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                try {
                    if (!mmsExeInfo.Directory.Exists) {
                        mmsExeInfo.Directory.Create();
                    }
                    MinecraftFileUtilities.CleanJavaServerDirectory(serverConfiguration);
                    string filePath = GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version);
                    if (!File.Exists(filePath)) {
                        if (!FetchBuild(version).Result) {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.jar\" and version was not found in JDS manifest!");
                        }
                    }
                    File.Copy(filePath, mmsExeInfo.FullName, true);
                    MinecraftFileUtilities.WriteJavaEulaFile(serverConfiguration);

                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, serverConfiguration), version);
                    if (serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        serverConfiguration.SetServerVersion(version);
                    }

                } catch (Exception e) {
                    if (e.GetType() == typeof(InvalidDataException)) {
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

        public List<SimpleVersionModel> GetSimpleVersionList() {
            List<SimpleVersionModel> outList = new List<SimpleVersionModel>();
            foreach (JavaVersionHistoryModel ver in _versionLookupTable.Values) {
                outList.Add(new SimpleVersionModel(ver.Version, ver.IsBeta, GetVersionPropList(ver.Version)));
            }
            return outList;
        }

        public void SetNewLogger(MmsLogger logger) => _logger = logger;

        public static Task ValidateJavaInstallation(MmsLogger logger) => Task.Run(() => {
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.JavaVanillaExe))) {
                if (HTTPHandler.RetrieveFileFromUrl(MmsUrlStrings[MmsUrlKeys.Jdk21DownloadLink], "Jdk.zip").Result) {
                    Progress<ProgressModel> progress = new(percent => {
                        logger.AppendLine($"First time java install; Extracting JDK 21. {percent.Progress}% completed...");
                    });
                    ZipUtilities.ExtractToDirectory("Jdk.zip", GetServiceDirectory(ServiceDirectoryKeys.Jdk21Path), progress).Wait();
                    File.Copy(GetServiceFilePath(MmsFileNameKeys.JavaVanillaExe), GetServiceFilePath(MmsFileNameKeys.JavaMmsExe));
                    File.Delete("Jdk.zip");
                }
            }
        });
    }
}