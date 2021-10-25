using BedrockService.Shared.Interfaces;
using BedrockService.Shared.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace BedrockService.Shared.PackParser
{
    public class Header
    {
        public string description { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public List<int> version { get; set; }
    }

    public class Module
    {
        public string description { get; set; }
        public string type { get; set; }
        public string uuid { get; set; }
        public List<int> version { get; set; }
    }

    public class Dependency
    {
        public string uuid { get; set; }
        public List<int> version { get; set; }
    }

    public class Manifest
    {
        public int format_version { get; set; }
        public Header header { get; set; }
        public List<Module> modules { get; set; }
        public List<Dependency> dependencies { get; set; }
    }

    public class MinecraftPackContainer
    {
        public Manifest JsonManifest;
        public DirectoryInfo PackContentLocation;
        public string ManifestType;
        public string FolderName;
        public byte[] IconBytes;

        public override string ToString()
        {
            return JsonManifest != null ? JsonManifest.header.name : "WorldPack";
        }
    }

    public class MinecraftPackParser
    {
        private IProcessInfo ProcessInfo;
        private ILogger Logger;
        public DirectoryInfo PackExtractDirectory;
        public List<MinecraftPackContainer> FoundPacks = new List<MinecraftPackContainer>();

        [JsonConstructor]
        public MinecraftPackParser(ILogger logger, IProcessInfo processInfo)
        {
            ProcessInfo = processInfo;
            Logger = logger;
        }

        public MinecraftPackParser(byte[] fileContents, ILogger logger, IProcessInfo processInfo)
        {
            Logger = logger;
            PackExtractDirectory = new DirectoryInfo($@"{processInfo.GetDirectory()}\Temp");
            ProcessInfo = processInfo;
            new FileUtils(processInfo.GetDirectory()).ClearTempDir();
            using (MemoryStream fileStream = new MemoryStream(fileContents, 5, fileContents.Length - 5))
            {
                using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    zipArchive.ExtractToDirectory(PackExtractDirectory.FullName);
                }
            }
            ParseDirectory(PackExtractDirectory);
        }

        public MinecraftPackParser(string[] files, DirectoryInfo extractDir, ILogger logger, IProcessInfo processInfo)
        {
            PackExtractDirectory = extractDir;
            Logger = logger;
            ProcessInfo = processInfo;
            new FileUtils(ProcessInfo.GetDirectory()).ClearTempDir();
            if (Directory.Exists($@"{PackExtractDirectory.FullName}\ZipTemp"))
            {
                Directory.CreateDirectory($@"{PackExtractDirectory.FullName}\ZipTemp");
            }
            foreach (string file in files)
            {
                FileInfo fInfo = new FileInfo(file);
                string zipFilePath = $@"{PackExtractDirectory.FullName}\{fInfo.Name.Replace(fInfo.Extension, "")}";
                ZipFile.ExtractToDirectory(file, zipFilePath);
                foreach (FileInfo extractedFile in new DirectoryInfo(zipFilePath).GetFiles())
                {
                    if (extractedFile.Extension == ".mcpack")
                    {
                        Directory.CreateDirectory($@"{zipFilePath}\{extractedFile.Name.Replace(extractedFile.Extension, "")}");
                        ZipFile.ExtractToDirectory(extractedFile.FullName, $@"{zipFilePath}\{fInfo.Name.Replace(fInfo.Extension, "")}_{extractedFile.Name.Replace(extractedFile.Extension, "")}");
                    }
                }
            }
            foreach (DirectoryInfo directory in PackExtractDirectory.GetDirectories())
                ParseDirectory(directory);
        }

        public void ParseDirectory(DirectoryInfo directoryToParse)
        {
            Logger.AppendLine($"Parsing directory {directoryToParse.Name}");
            if (directoryToParse.Exists)
            {
                foreach (FileInfo file in directoryToParse.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (file.Name == "levelname.txt")
                    {
                        byte[] iconBytes;
                        try
                        {
                            if (File.Exists($@"{file.Directory.FullName}\world_icon.jpeg"))
                                iconBytes = File.ReadAllBytes($@"{file.Directory.FullName}\world_icon.jpeg");
                            else
                                iconBytes = File.ReadAllBytes($@"{file.Directory.FullName}\world_icon.png");
                        }
                        catch
                        {
                            iconBytes = null;
                        }

                        FoundPacks.Add(new MinecraftPackContainer
                        {
                            JsonManifest = null,
                            PackContentLocation = file.Directory,
                            ManifestType = "WorldPack",
                            FolderName = file.Directory.Name,
                            IconBytes = iconBytes
                        });
                        Logger.AppendLine("Pack was detected as MCWorld");
                        return;
                    }
                    if (file.Name == "manifest.json")
                    {
                        byte[] iconBytes;
                        if (File.Exists($@"{file.Directory.FullName}\pack_icon.jpeg"))
                            iconBytes = File.ReadAllBytes($@"{file.Directory.FullName}\pack_icon.jpeg");
                        else
                            iconBytes = File.ReadAllBytes($@"{file.Directory.FullName}\pack_icon.png");

                        MinecraftPackContainer container = new MinecraftPackContainer
                        {
                            JsonManifest = new Manifest(),
                            PackContentLocation = file.Directory,
                            ManifestType = "",
                            FolderName = file.Directory.Name,
                            IconBytes = iconBytes
                        };
                        JsonSerializerSettings settings = new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };
                        container.JsonManifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(file.FullName), settings);
                        container.ManifestType = container.JsonManifest.modules[0].type;
                        Logger.AppendLine($"{container.ManifestType} pack found, name: {container.JsonManifest.header.name}, path: {container.PackContentLocation.Name}");
                        FoundPacks.Add(container);
                    }
                }
            }
        }
    }
}
