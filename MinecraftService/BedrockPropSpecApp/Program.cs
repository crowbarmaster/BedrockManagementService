using static MinecraftService.Shared.Classes.SharedStringBase;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using System.IO.Compression;
using MinecraftService.Shared.Utilities;
using System.Diagnostics;
using Newtonsoft.Json;
using MinecraftService.Shared.Classes.Updaters;
using MinecraftService.Shared.Interfaces;

string tempPath = $"{Path.GetTempPath()}MMSTemp\\PropBuilder";
Directory.CreateDirectory(tempPath);
Dictionary<string, List<PropInfoEntry>> versionPropLayout = new();
IProcessInfo processInfo = new ServiceProcessInfo("PropBuilder", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), Process.GetCurrentProcess().Id, false, false);
SetWorkingDirectory(processInfo);
IServerLogger logger = new MinecraftServerLogger(processInfo);

string bedrockManifestContent = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsVersionJson]).Result;
if (bedrockManifestContent == null) {
    logger.AppendLine("Error fetching manifest. Please run again!");
    return;
}
List<LegacyBedrockVersionModel> versionList = JsonConvert.DeserializeObject<List<LegacyBedrockVersionModel>>(bedrockManifestContent);
if(versionList == null) {
    logger.AppendLine("Error parisng verion list. Please check file and try again!");
    return;
}

void ConsoleWait() {
    Console.Clear();
    logger.AppendLine("Welcome to MMS's prop builder tool!");
    logger.AppendLine("Available inputs: q/Quit; p/build props; u/Update manifest file");
    string consoleInput = "";
    while ((consoleInput = Console.ReadLine()).ToLower() != "q") {
        if (consoleInput.ToLower() == "CreateBdsPropMft".ToLower()) {
            CreateBedrockPropManifestFile();
        }
        if (consoleInput.ToLower() == "UpLegacyBdsMft".ToLower()) {
            UpgradeBedrockLegacyManifest();
        }
        if (consoleInput.ToLower() == "CreateJavaPropVersionMft".ToLower()) {
            CreateJavaVersionPropManifestFile();
        }
        Task.Delay(100).Wait();
    }
}

ConsoleWait();

void CreateBedrockPropManifestFile() {
    versionPropLayout.Clear();
    foreach (LegacyBedrockVersionModel version in versionList) {
        HTTPHandler.RetrieveFileFromUrl(version.WindowsBinUrl, $@"{tempPath}\{version.Version}-Bedrock.zip").Wait();
        Task.Delay(1000).Wait();
        FileStream zipStream = File.Open($@"{tempPath}\{version.Version}-Bedrock.zip", FileMode.Open);
        ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        logger.AppendLine($"Now scanning version {version.Version}...");

        foreach (ZipArchiveEntry entry in zipArchive.Entries) {
            if (entry.Name == "server.properties") {
                ReadPropStreamToArray(entry.Open(), version.Version).Wait();
            }
        }
        zipArchive.Dispose();
        zipStream.Close();
    }
    File.WriteAllText("output.json", JsonConvert.SerializeObject(versionPropLayout, new JsonSerializerSettings { Formatting = Formatting.Indented }));
}

Task ReadPropStreamToArray(Stream stream, string version) => Task.Run(() => {
    StreamReader entryStream = new StreamReader(stream);
    string line;
    if (!versionPropLayout.ContainsKey(version)) {
        versionPropLayout.Add(version, new List<PropInfoEntry>());
    }
    while ((line = entryStream.ReadLine()) != null) {
        if (line != string.Empty && !line.StartsWith('#')) {
            string[] kvp = line.Split('=');
            if (kvp.Length < 2) {
                string[] temp = new string[2];
                temp[0] = kvp[0];
                temp[1] = "";
                kvp = temp;
            }
            versionPropLayout[version].Add(new PropInfoEntry(kvp[0], kvp[1]));
        }
    }
});

void CreateJavaVersionPropManifestFile() {
    versionPropLayout.Clear();
    JavaUpdater.ValidateJavaInstallation(logger).Wait();
    List<JavaVersionManifestModel> manifestList = new List<JavaVersionManifestModel>();
    string content = string.Empty;
    while (content == string.Empty) {
        try {
            content = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.JdsVersionJson]).Result;
        } catch {
            return;
        }
    }

    JavaVersionHistoryModel versionList;
    JavaVersionDetailsModel releaseDetails = new JavaVersionDetailsModel();
    try {
        versionList = JsonConvert.DeserializeObject<JavaVersionHistoryModel>(content);
    } catch (Exception ex) {
        logger.AppendLine("Error: Could not parse manifests to object model!");
        return;
    }
    manifestList.Clear();
    versionList.Versions = versionList.Versions.Where(x => !x.Type.StartsWith("old_")).ToList();
    versionList.Versions.Reverse();
    foreach (JavaVersionManifest version in versionList.Versions) {
        releaseDetails = ExtractJavaPropInfo(version).Result;
        if (versionPropLayout.ContainsKey(version.Id)) {
            JavaVersionManifestModel model = new JavaVersionManifestModel {
                Version = version.Id,
                IsBeta = version.Type == "snapshot",
                DownloadUrl = releaseDetails.Downloads.Server.Url,
                PropList = versionPropLayout[version.Id].ToList()
            };
            manifestList.Add(model);
        }
    }
    File.WriteAllText("java_version_prop_manifest.json", JsonConvert.SerializeObject(manifestList, new JsonSerializerSettings { Formatting = Formatting.Indented }));

    Task<JavaVersionDetailsModel?> ExtractJavaPropInfo (JavaVersionManifest version) => Task.Run(() => {
        if (File.Exists($"{tempPath}\\server.properties")) {
            File.Delete($"{tempPath}\\server.properties");
        }
        string content = string.Empty;
        int failCount = 0;
        JavaVersionDetailsModel releaseDetails = new();
        while (content == string.Empty) {
            try {
                content = HTTPHandler.FetchHTTPContent(version.Url).Result;
                if(content != string.Empty) {
                    releaseDetails = JsonConvert.DeserializeObject<JavaVersionDetailsModel>(content);
                }
            } catch {
                logger.AppendLine($"Attempt to fetch java release manifest failed.");
            }
            failCount++;
            if(failCount > 3) {
                logger.AppendLine($"Attempt to fetch java release manifest failed.");
                return null;
            }
        }
        if (releaseDetails.Downloads != null && releaseDetails.Downloads.Server != null) {
            string fixedId = releaseDetails.Id.Replace(' ', '-');
            HTTPHandler.RetrieveFileFromUrl(releaseDetails.Downloads.Server.Url, $"{tempPath}\\Server-{fixedId}.jar").Wait();
            string suffix = releaseDetails.Type == "release" ? "release" : "beta";
            logger.AppendLine($"Extracting Java version model: Version {releaseDetails.Id}-{suffix}...");
            ProcessUtilities.QuickLaunchJar($"{tempPath}\\Server-{fixedId}.jar").Wait();
            Stream propFile = null;
            failCount = 0;
            if (!TryOpenFile($"{tempPath}\\server.properties", out propFile) || propFile == null || propFile.Length == 0) {
                 return null;
            }
            ReadPropStreamToArray(propFile, releaseDetails.Id).Wait();
            propFile.Close();
            propFile.Dispose();
            File.Delete($"{tempPath}\\server.properties");
            Task.Delay(300).Wait();
        }
        return releaseDetails;
    });
}

bool TryOpenFile(string path, out Stream stream) {
    try {
        FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream = fileStream;
        return true;
    } catch (IOException ex) {
        stream = null;
        return false;
    }
}

void UpgradeBedrockLegacyManifest() {
    if(!File.Exists("output.json") || versionPropLayout.Count == 0) {
        CreateBedrockPropManifestFile();
    }

    List<BedrockVersionHistoryModel> newList = new();
    foreach (LegacyBedrockVersionModel bedrockVersion in versionList) {
        BedrockVersionHistoryModel newEntry = new BedrockVersionHistoryModel() {
            Version = bedrockVersion.Version,
            WindowsBinUrl = bedrockVersion.WindowsBinUrl,
            LinuxBinUrl = bedrockVersion.LinuxBinUrl,
            PropList = versionPropLayout[bedrockVersion.Version]
        };
        newList.Add(newEntry);
    }
    File.WriteAllText("updatedManifest.json", JsonConvert.SerializeObject(newList, new JsonSerializerSettings { Formatting = Formatting.Indented }));
    logger.AppendLine("Manifest file has been created!");
}