using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.Service;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Classes.Server.Updaters {
    public class BedrockUpdater : IUpdater {
        private MmsLogger _logger;
        private bool _isInitialized = false;
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Bedrock;
        private Dictionary<string, BedrockVersionHistoryModel> _versionLookupTable = [];

        public BedrockUpdater(MmsLogger logger, ServiceConfigurator serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        [JsonConstructor]
        public BedrockUpdater() { }

        public Task Initialize() => Task.Run(() => {
            CheckLatestVersion().Wait();
            _isInitialized = true;
        });

        private void LoadLocalManifestContent() {
            string versionManifestPath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString());
            if (File.Exists(versionManifestPath)) {
                _versionLookupTable.Clear();
                try {
                    List<BedrockVersionHistoryModel> versionList = JsonConvert.DeserializeObject<List<BedrockVersionHistoryModel>>(File.ReadAllText(versionManifestPath));
                    foreach (BedrockVersionHistoryModel versionHistoryModel in versionList) {
                        _versionLookupTable.Add(versionHistoryModel.Version, versionHistoryModel);
                    }
                } catch {

                }
            }
        }

        private void UpdateManifestTable() {
            string versionManifestPath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString());
            string latestVerFile = GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, _serverArch.ToString());
            LoadLocalManifestContent();
            _logger.AppendLine("Checking latest BDS version...");
            int retryCount = 1;
            string content = string.Empty;
            while (content == string.Empty) {
                try {
                    content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsLatestIni]).Result;
                } catch {
                    if (retryCount > 2) {
                        if (File.Exists(latestVerFile)) {
                            _logger.AppendLine($"Attempt to fetch {_serverArch} version file failed. Using previously stored version!");
                            content = File.ReadAllText(latestVerFile);
                            break;
                        }
                        _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                        return;
                    }
                    _logger.AppendLine($"Attempt to fetch {_serverArch} latest version failed... retry {retryCount} of 2");
                    retryCount++;
                }
            }
            if (!File.Exists(latestVerFile) || content != File.ReadAllText(latestVerFile)) {
                File.WriteAllText(latestVerFile, content);
                content = string.Empty;
                retryCount = 1;
                _logger.AppendLine("New version of BDS found - Fetching latest BDS version manifest...");
                while (content == string.Empty) {
                    try {
                        content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsVersionJson]).Result;
                        File.WriteAllText(versionManifestPath, content);
                    } catch {
                        if (retryCount > 2) {
                            if (!File.Exists(versionManifestPath)) {
                                _logger.AppendLine("No local manifest file was found. MMS may not function correctly!");
                                return;
                            }
                            _logger.AppendLine("Error fetching content from URL. Check connection and try again!");
                            break;
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
                string latestVerFile = GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, _serverArch.ToString());
                UpdateManifestTable();
                _logger.AppendLine($"Latest version found: \"{File.ReadAllText(latestVerFile)}\"");
                _serviceConfiguration.SetServerDefaultPropList(_serverArch, GetDefaultVersionPropList());
            });
        }

        public List<Property> GetVersionPropList(string version) {
            List<Property> outList = new List<Property>();
            if (!_versionLookupTable.ContainsKey(version)) {
                return outList;
            }
            foreach (PropInfoEntry prop in _versionLookupTable[version].PropList) {
                outList.Add(new Property(prop.Key, prop.Value));
            }
            return outList;
        }

        public List<Property> GetDefaultVersionPropList() {
            List<Property> outList = new List<Property>();
            BedrockVersionHistoryModel verModel = _versionLookupTable.Last().Value;
            foreach (PropInfoEntry prop in verModel.PropList) {
                outList.Add(new Property(prop.Key, prop.Value));
            }
            return outList;
        }

        public bool ValidateBuildExists(string version) => !File.Exists(GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, version));

        public virtual Task<bool> FetchBuild(string version) {
            return Task.Run(() => {
                _logger.AppendLine($"Now downloading version {version}, please wait...");
                string fetchUrl = string.Format(MmsUrlStrings[MmsUrlKeys.BdsPackage_Ver], version);
                string zipPath = GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, version);
                new FileInfo(zipPath).Directory.Create();
                if (File.Exists(zipPath)) {
                    return true;
                }
                if (HTTPHandler.RetrieveFileFromUrl(fetchUrl, zipPath).Result) {
                    return true;
                }
                return false;
            });
        }

        public virtual async Task ReplaceBuild(IServerConfiguration serverConfiguration, string versionOverride = "") {
            await Task.Run(() => {
                if (serverConfiguration == null) {
                    return;
                }
                string exeName = serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue;
                ProcessUtilities.KillProcessList(Process.GetProcessesByName(exeName.Substring(0, exeName.Length - 4)));
                string version = versionOverride == "" ? serverConfiguration.GetServerVersion() : versionOverride;
                FileInfo originalExeInfo = new(GetServerFilePath(ServerFileNameKeys.VanillaBedrock, serverConfiguration));
                FileInfo mmsExeInfo = new($@"{serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                try {
                    if (!mmsExeInfo.Directory.Exists) {
                        mmsExeInfo.Directory.Create();
                    }
                    MinecraftFileUtilities.CleanBedrockDirectory(serverConfiguration);
                    if (!Directory.Exists(serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString()))
                        Directory.CreateDirectory(serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
                    string filePath = GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, version);
                    if (!File.Exists(filePath)) {
                        if (!FetchBuild(version).Result) {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.zip\" and version was not found in BDS manifest!");
                        }
                    }
                    Progress<ProgressModel> progress = new(percent => {
                        _logger.AppendLine($"Extracting Bedrock files for server {serverConfiguration.GetServerName()}, {percent.Progress}% completed...");
                    });
                    ZipUtilities.ExtractToDirectory(filePath, serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), progress).Wait();

                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, serverConfiguration), version);
                    if (serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        serverConfiguration.SetServerVersion(version);
                    }
                    File.Copy(originalExeInfo.FullName, mmsExeInfo.FullName, true);

                } catch (InvalidDataException) {
                    throw new FileNotFoundException($"Build file \"Update_{version}.zip\" found corrupt. Service cannot proceed!!");
                }
            });
        }

        public string GetBaseVersion(string version) {
            return version;
        }

        public List<SimpleVersionModel> GetSimpleVersionList() {
            List<SimpleVersionModel> result = new List<SimpleVersionModel>();
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsVersionJson]).Result;
            if (content == null)
                return new List<SimpleVersionModel>();
            List<LegacyBedrockVersionModel> versionList = JsonConvert.DeserializeObject<List<LegacyBedrockVersionModel>>(content);
            foreach (var version in versionList) {
                result.Add(new(version.Version, false, GetVersionPropList(version.Version)));
            }
            return result;
        }

        public void SetNewLogger(MmsLogger logger) => _logger = logger;
    }
}
