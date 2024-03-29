﻿using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes.Updaters {
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
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]))) {
                _logger.AppendLine("Version ini file missing, creating...");
                File.Create(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch])).Close();
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
                int retryCount = 1;
                string versionManifestPath = GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString());
                string content = string.Empty;
                while (content == string.Empty) {
                    try {
                        content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsVersionJson]).Result;
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
                try {
                    List<BedrockVersionHistoryJson> versionList = JsonSerializer.Deserialize<List<BedrockVersionHistoryJson>>(content);
                    BedrockVersionHistoryJson latest = versionList.Last();

                    string downloadPath = latest.WindowsBinUrl;
                    string fetchedVersion = latest.Version;

                    _logger.AppendLine($"Latest version found: \"{fetchedVersion}\"");
                    if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, fetchedVersion))) {
                        FetchBuild(fetchedVersion).Wait();
                        ProcessBdsPackage(fetchedVersion).Wait();
                    }
                    File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.VersionManifest_Name, _serverArch.ToString()), content);
                    File.WriteAllText(GetServiceFilePath(MmsFileNameKeys.LatestVerIni_Name, MinecraftArchStrings[_serverArch]), fetchedVersion);
                    _serviceConfiguration.SetLatestVersion(_serverArch, fetchedVersion);

                    _serviceConfiguration.SetServerDefaultPropList(_serverArch, MinecraftFileUtilities.GetDefaultPropListFromFile(GetServiceFilePath(MmsFileNameKeys.BedrockStockProps_Ver, fetchedVersion)));
                } catch (Exception ex) {
                    _logger.AppendLine($"Error checking lastest Bedrock version. Check URL status and your network, and try again!\r\nStacktrace: {ex.Message}");
                }
            });
        }

        private Task ProcessBdsPackage(string version) {
            return Task.Run(() => {
                string propFile = GetServiceFilePath(MmsFileNameKeys.BedrockStockProps_Ver, version);
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
                string fetchUrl = string.Format(MmsUrlStrings[MmsUrlKeys.BdsPackage_Ver], version);
                string zipPath = GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, version);
                new FileInfo(zipPath).Directory.Create();
                if (File.Exists(zipPath)) {
                    ProcessBdsPackage(version).Wait();
                    return true;
                }
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
                FileInfo originalExeInfo = new(GetServerFilePath(ServerFileNameKeys.VanillaBedrock, _serverConfiguration));
                FileInfo mmsExeInfo = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                try {
                    if (!mmsExeInfo.Directory.Exists) {
                        mmsExeInfo.Directory.Create();
                    }
                    MinecraftFileUtilities.CleanBedrockDirectory(_serverConfiguration);
                    if (!Directory.Exists(_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString()))
                        Directory.CreateDirectory(_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString());
                    string filePath = GetServiceFilePath(MmsFileNameKeys.BdsUpdatePackage_Ver, version);
                    if (!File.Exists(filePath)) {
                        if (!FetchBuild(version).Result) {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.zip\" and version was not found in BDS manifest!");
                        }
                    }
                    Progress<ProgressModel> progress = new(percent => {
                        _logger.AppendLine($"Extracting Bedrock files for server {_serverConfiguration.GetServerName()}, {percent.Progress}% completed...");
                    });
                    ZipUtilities.ExtractToDirectory(filePath, _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath).ToString(), progress).Wait();

                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, _serverConfiguration), version);
                    if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue()) {
                        _serverConfiguration.SetServerVersion(version);
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

        public List<SimpleVersionModel> GetVersionList() {
            List<SimpleVersionModel> result = new List<SimpleVersionModel>();
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsVersionJson]).Result;
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
