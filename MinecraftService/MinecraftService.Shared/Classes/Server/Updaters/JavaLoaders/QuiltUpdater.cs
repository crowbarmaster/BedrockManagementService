using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server.Updaters;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;
namespace MinecraftService.Shared.Classes.Server.Updaters.JavaLoaders
{
    public class QuiltUpdater : JavaUpdater, IUpdater
    {
        private MmsLogger _logger;
        private readonly ServiceConfigurator _serviceConfiguration;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly MinecraftServerArch _serverArch = MinecraftServerArch.Java;

        public QuiltUpdater(MmsLogger logger, ServiceConfigurator serviceConfiguration) : base(logger, serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public new Task Initialize() => Task.Run(() =>
        {
            base.Initialize();
        });

        public new virtual Task CheckLatestVersion()
        {
            return Task.Run(() =>
            {
                base.CheckLatestVersion();
            });
        }

        public virtual Task<bool> FetchBuild(string version)
        {
            return Task.Run(() =>
            {
                JavaVersionHistoryModel selectedVersion = GetJavaVersionModel(version);
                if (selectedVersion != null)
                {
                    FetchBuild(selectedVersion);
                    return true;
                }
                return false;
            });
        }

        public virtual Task<bool> FetchBuild(JavaVersionHistoryModel version)
        {
            return Task.Run(() =>
            {
                _logger.AppendLine($"Now downloading Java version {version.Version}, please wait...");
                string zipPath = GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version.Version);
                if (HTTPHandler.RetrieveFileFromUrl(version.DownloadUrl, zipPath).Result)
                {
                    return true;
                }
                return false;
            });
        }

        public virtual async Task ReplaceBuild(string versionOverride = "")
        {
            await Task.Run(() =>
            {
                if (_serverConfiguration == null)
                {
                    return;
                }
                ValidateJavaInstallation().Wait();
                string exeName = _serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName).StringValue;
                ProcessUtilities.KillJarProcess(exeName);
                string version = versionOverride == "" ? _serverConfiguration.GetServerVersion() : versionOverride;
                FileInfo originalExeInfo = new(GetServerFilePath(ServerFileNameKeys.VanillaJava, _serverConfiguration));
                FileInfo mmsExeInfo = new($@"{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerPath)}\{_serverConfiguration.GetSettingsProp(ServerPropertyKeys.ServerExeName)}");
                try
                {
                    if (!mmsExeInfo.Directory.Exists)
                    {
                        mmsExeInfo.Directory.Create();
                    }
                    MinecraftFileUtilities.CleanJavaServerDirectory(_serverConfiguration);
                    string filePath = GetServiceFilePath(MmsFileNameKeys.JdsUpdatePackage_Ver, version);
                    if (!File.Exists(filePath))
                    {
                        if (!FetchBuild(version).Result)
                        {
                            throw new FileNotFoundException($"Service could not locate file \"Update_{version}.jar\" and version was not found in JDS manifest!");
                        }
                    }
                    File.Copy(filePath, mmsExeInfo.FullName, true);
                    MinecraftFileUtilities.WriteJavaEulaFile(_serverConfiguration);

                    File.WriteAllText(GetServerFilePath(ServerFileNameKeys.DeployedINI, _serverConfiguration), version);
                    if (_serverConfiguration.GetSettingsProp(ServerPropertyKeys.AutoDeployUpdates).GetBoolValue())
                    {
                        _serverConfiguration.SetServerVersion(version);
                    }

                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(InvalidDataException))
                    {
                        throw new FileNotFoundException($"Build file \"Update_{version}.zip\" found corrupt. Service cannot proceed!!");
                    }
                    else
                    {
                        _logger.AppendLine($"ReplaceServerBuild resulted in error: {e.Message}\n{e.StackTrace}");
                    }
                }
            });
        }

        public string GetBaseVersion(string version)
        {
            return version;
        }

        public List<SimpleVersionModel> GetSimpleVersionList()
        {
            List<SimpleVersionModel> result = new List<SimpleVersionModel>();
            string content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
            if (content == null)
                return new List<SimpleVersionModel>();
            List<JavaVersionHistoryModel> versionList = JsonConvert.DeserializeObject<List<JavaVersionHistoryModel>>(content);

            foreach (var version in versionList)
            {
                result.Add(new(version.Version, version.IsBeta));
            }
            return result;
        }

        public void SetNewLogger(MmsLogger logger) => _logger = logger;

        private Task ValidateJavaInstallation() => Task.Run(() =>
        {
            if (!File.Exists(GetServiceFilePath(MmsFileNameKeys.Jdk17JavaVanillaExe)))
            {
                if (HTTPHandler.RetrieveFileFromUrl(MmsUrlStrings[MmsUrlKeys.Jdk17DownloadLink], "Jdk.zip").Result)
                {
                    Progress<ProgressModel> progress = new(percent =>
                    {
                        _logger.AppendLine($"First time java install; Extracting JDK 17. {percent.Progress}% completed...");
                    });
                    ZipUtilities.ExtractToDirectory("Jdk.zip", GetServiceDirectory(ServiceDirectoryKeys.Jdk17Path), progress).Wait();
                    File.Copy(GetServiceFilePath(MmsFileNameKeys.Jdk17JavaVanillaExe), GetServiceFilePath(MmsFileNameKeys.Jdk17JavaMmsExe));
                    File.Delete("Jdk.zip");
                }
            }
        });
    }
}
