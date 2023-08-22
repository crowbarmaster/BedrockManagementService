using BedrockService.Shared.Interfaces;
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
using BedrockService.Shared.Utilities;
using System.Diagnostics;

namespace BedrockService.Shared.Classes.Updaters {
    public class BedrockUpdater : IUpdater {
        private IServerLogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Bedrock;

        public BedrockUpdater(IServerLogger logger, IServiceConfiguration serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public BedrockUpdater(IServerLogger logger, IServiceConfiguration serviceConfiguration, IServerConfiguration serverConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _serverConfiguration = serverConfiguration;
            _logger = logger;
        }

        public void Initialize() {
            if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]))) {
                _logger.AppendLine("Version ini file missing, creating...");
                File.Create(GetServiceFilePath(BmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch])).Close();
                CheckLatestVersion().Wait();
                return;
            }
            if (_serverConfiguration == null) {
                CheckLatestVersion().Wait();
            }
        }

        public virtual Task CheckLatestVersion() {
            return Task.Run(() => {
                _logger.AppendLine("Checking latest BDS Version...");
                string content = HTTPHandler.FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.BdsVersionJson]).Result;
                if (content == null)
                    return;
                List<BedrockVersionHistoryJson> versionList = JsonSerializer.Deserialize<List<BedrockVersionHistoryJson>>(content);
                BedrockVersionHistoryJson latest = versionList.Last();

                string downloadPath = latest.WindowsBinUrl;
                string fetchedVersion = latest.Version;

                _logger.AppendLine($"Latest version found: \"{fetchedVersion}\"");
                if (!File.Exists(GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, fetchedVersion))) {
                    FetchBuild(fetchedVersion).Wait();
                    ProcessBdsPackage(fetchedVersion).Wait();
                }
                File.WriteAllText(GetServiceFilePath(BmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]), fetchedVersion);
                _serviceConfiguration.SetLatestVersion(_serverArch, fetchedVersion);

                _serviceConfiguration.SetServerDefaultPropList(_serverArch, MinecraftFileUtilities.GetDefaultPropListFromFile(GetServiceFilePath(BmsFileNameKeys.BedrockStockProps_Ver, fetchedVersion)));
            });
        }

        private Task ProcessBdsPackage(string version) {
            return Task.Run(() => {
                string propFile = GetServiceFilePath(BmsFileNameKeys.BedrockStockProps_Ver, version);
                if (!File.Exists(propFile)) {
                    FileInfo file = new(propFile);
                    BedrockUpdatePackageProcessor packageProcessor = new(version, file.Directory.FullName);
                    if (!packageProcessor.ExtractCoreFiles()) {
                        return;
                    }
                }
            });
        }

        public virtual Task<bool> FetchBuild(string version) {
            return Task.Run(() => {
                _logger.AppendLine($"Now downloading version {version}, please wait...");
                string fetchUrl = string.Format(BmsUrlStrings[BmsUrlKeys.BdsPackage_Ver], version);
                string zipPath = GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, version);
                new FileInfo(zipPath).Directory.Create();
                if (HTTPHandler.RetrieveFileFromUrl(fetchUrl, zipPath).Result) {
                    ProcessBdsPackage(version).Wait();
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
                ProcessUtilities.KillProcessList(Process.GetProcessesByName(exeName.Substring(0, exeName.Length - 4)));
                string version = versionOverride == "" ? _serverConfiguration.GetServerVersion() : versionOverride;
                FileInfo originalExeInfo = new(GetServerFilePath(BdsFileNameKeys.VanillaBedrock, _serverConfiguration));
                FileInfo bmsExeInfo = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                try {
                    if (!bmsExeInfo.Directory.Exists) {
                        bmsExeInfo.Directory.Create();
                    }
                    MinecraftFileUtilities.CleanBedrockDirectory(_serverConfiguration);
                    if (!Directory.Exists(_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString()))
                        Directory.CreateDirectory(_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
                    string filePath = GetServiceFilePath(BmsFileNameKeys.BdsUpdatePackage_Ver, version);
                    if (!File.Exists(filePath)) {
                        if (!FetchBuild(version).Result) {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.zip\" and version was not found in BDS manifest!");
                        }
                    }
                    Progress<double> progress = new(percent => {
                        _logger.AppendLine($"Extracting Bedrock files for server {_serverConfiguration.GetServerName()}, {percent}% completed...");
                    });
                    FileUtilities.ExtractZipToDirectory(filePath, _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), progress).Wait();

                    File.WriteAllText(GetServerFilePath(BdsFileNameKeys.DeployedINI, _serverConfiguration), version);
                    if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        _serverConfiguration.SetServerVersion(version);
                    }
                    File.Copy(originalExeInfo.FullName, bmsExeInfo.FullName, true);

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
            string content = HTTPHandler.FetchHTTPContent(BmsUrlStrings[BmsUrlKeys.BdsVersionJson]).Result;
            if (content == null)
                return new List<SimpleVersionModel>();
            List<BedrockVersionHistoryJson> versionList = JsonSerializer.Deserialize<List<BedrockVersionHistoryJson>>(content);
            foreach (var version in versionList) {
                result.Add(new(version.Version, false));
            }
            return result;
        }

        public void SetNewLogger(IServerLogger logger) => _logger = logger;
    }
}
