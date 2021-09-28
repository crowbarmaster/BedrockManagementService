using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Server.Management
{
    public class MinecraftPackContainer
    {
        public MinecraftPackManifestJsonClass JsonManifest;
        public DirectoryInfo PackContentLocation = new DirectoryInfo($@"{Program.ServiceDirectory}\Temp");
        public string ManifestType;
    }

    public class MinecraftPackArchiveParser
    {
        public DirectoryInfo PackExtractDirectory = new DirectoryInfo($@"{Program.ServiceDirectory}\Temp");
        List<MinecraftPackContainer> FoundPacks = new List<MinecraftPackContainer>();

        public MinecraftPackArchiveParser (byte[] fileContents)
        {
            if (!PackExtractDirectory.Exists)
                PackExtractDirectory.Create();
            else
                foreach (FileInfo file in PackExtractDirectory.GetFiles("*", SearchOption.AllDirectories))
                    file.Delete();
            foreach (DirectoryInfo directory in PackExtractDirectory.GetDirectories())
                directory.Delete(true);
            using (MemoryStream fileStream = new MemoryStream(fileContents, 5, fileContents.Length - 5))
            {
                using (ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    zipArchive.ExtractToDirectory(PackExtractDirectory.FullName);
                }
            }
            InstanceProvider.ServiceLogger.AppendLine("Zip file extracted, now parsing files...");
            foreach(FileInfo file in PackExtractDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                if (file.Name == "level.dat")
                {
                    FoundPacks.Add(new MinecraftPackContainer
                    {
                        JsonManifest = null,
                        PackContentLocation = file.Directory,
                        ManifestType = "WorldPack"
                    });
                    InstanceProvider.ServiceLogger.AppendLine("Pack was detected as MCWorld");
                    return;
                }
                if (file.Name == "manifest.json")
                {
                    MinecraftPackContainer container = new MinecraftPackContainer
                    {
                        JsonManifest = new MinecraftPackManifestJsonClass(),
                        PackContentLocation = file.Directory,
                        ManifestType = ""
                    };
                    container.JsonManifest.ManifestFromFile = JsonConvert.DeserializeObject<MinecraftPackManifestJsonClass.Manifest>(File.ReadAllText(file.FullName));
                    container.ManifestType = container.JsonManifest.ManifestFromFile.modules[0].type;
                    InstanceProvider.ServiceLogger.AppendLine($"{container.ManifestType} pack found, name: {container.JsonManifest.ManifestFromFile.header.name}, path: {container.PackContentLocation.Name}");
                    FoundPacks.Add(container);
                }   
            }
        }
    }
}
