using static MinecraftService.Shared.Classes.SharedStringBase;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.JsonModels.MinecraftJsonModels;
using System.Text.Json;
using System.IO.Compression;
using System.Text.Json.Serialization;


string tempPath = $"{Path.GetTempPath()}MMSTemp\\PropBuilder";
Directory.CreateDirectory(tempPath);
Dictionary<string, List<PropInfoEntry>> versionPropLayout = new();

string bedrockManifestContent = HTTPHandler.FetchHTTPContent(MmsUrlStrings[MmsUrlKeys.BdsVersionJson]).Result;
if (bedrockManifestContent == null) {
    Console.WriteLine("Error fetching manifest. Please run again!");
    return;
}
List<LegacyBedrockVersionModel> versionList = JsonSerializer.Deserialize<List<LegacyBedrockVersionModel>>(bedrockManifestContent);
if(versionList == null) {
    Console.WriteLine("Error parisng verion list. Please check file and try again!");
    return;
}

Console.WriteLine("Welcome to MMS's prop builder tool!");
Console.WriteLine("Available inputs: q/Quit; p/build props; u/Update manifest file");

string consoleInput = "";
while((consoleInput = Console.ReadLine()).ToLower() != "q") {
    if(consoleInput.ToLower() == "p") {
        CreatePropManifests();
    }
    if (consoleInput.ToLower() == "u") {
        UpdateManifestListWithProps();
    }
}

void CreatePropManifests() {
    versionPropLayout.Clear();
    foreach (LegacyBedrockVersionModel version in versionList) {
        HTTPHandler.RetrieveFileFromUrl(version.WindowsBinUrl, $@"{tempPath}\{version.Version}-Bedrock.zip").Wait();
        Task.Delay(1000).Wait();
        FileStream zipStream = File.Open($@"{tempPath}\{version.Version}-Bedrock.zip", FileMode.Open);
        ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        Console.WriteLine($"Now scanning version {version.Version}...");

        foreach (ZipArchiveEntry entry in zipArchive.Entries) {
            if (entry.Name == "server.properties") {
                StreamReader entryStream = new StreamReader(entry.Open());
                string line;
                while ((line = entryStream.ReadLine()) != null) {
                    if (line != string.Empty && !line.StartsWith('#')) {
                        string[] kvp = line.Split('=');
                        if (!versionPropLayout.ContainsKey(version.Version)) {
                            versionPropLayout.Add(version.Version, new List<PropInfoEntry>());
                        }
                        if (kvp.Length < 2) {
                            string[] temp = new string[2];
                            temp[0] = kvp[0];
                            temp[1] = "";
                            kvp = temp;
                        }
                        versionPropLayout[version.Version].Add(new PropInfoEntry(kvp[0], kvp[1]));

                    }
                }
            }
        }
        zipArchive.Dispose();
        zipStream.Close();
    }
    File.WriteAllText("output.json", JsonSerializer.Serialize(versionPropLayout, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
}

void UpdateManifestListWithProps() {
    if(!File.Exists("output.json") || versionPropLayout.Count == 0) {
        CreatePropManifests();
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
    File.WriteAllText("updatedManifest.json", JsonSerializer.Serialize(newList, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
    Console.WriteLine("Manifest file has been created!");
}