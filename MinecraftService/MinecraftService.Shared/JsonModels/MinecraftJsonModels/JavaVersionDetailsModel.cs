using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftService.Shared.JsonModels.MinecraftJsonModels {
    public class JavaVersionDetailsModel {
        [JsonProperty("arguments")]
        public Arguments Arguments { get; set; }

        [JsonProperty("assetIndex")]
        public AssetIndex AssetIndex { get; set; }

        [JsonProperty("assets")]
        public string Assets { get; set; }

        [JsonProperty("complianceLevel")]
        public int ComplianceLevel { get; set; }

        [JsonProperty("downloads")]
        public Downloads Downloads { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("javaVersion")]
        public JavaVersion JavaVersion { get; set; }

        [JsonProperty("libraries")]
        public List<Library> Libraries { get; set; }

        [JsonProperty("logging")]
        public Logging Logging { get; set; }

        [JsonProperty("mainClass")]
        public string MainClass { get; set; }

        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion { get; set; }

        [JsonProperty("releaseTime")]
        public DateTime ReleaseTime { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Arguments {
        [JsonProperty("game")]
        public List<object> Game { get; set; }

        [JsonProperty("jvm")]
        public List<object> Jvm { get; set; }
    }

    public class Artifact {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class AssetIndex {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("totalSize")]
        public int TotalSize { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Client {
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("argument")]
        public string Argument { get; set; }

        [JsonProperty("file")]
        public MCFile File { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class ClientMappings {
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Downloads {
        [JsonProperty("client")]
        public Client Client { get; set; }

        [JsonProperty("client_mappings")]
        public ClientMappings ClientMappings { get; set; }

        [JsonProperty("server")]
        public Server Server { get; set; }

        [JsonProperty("server_mappings")]
        public ServerMappings ServerMappings { get; set; }

        [JsonProperty("artifact")]
        public Artifact Artifact { get; set; }
    }

    public class MCFile {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class JavaVersion {
        [JsonProperty("component")]
        public string Component { get; set; }

        [JsonProperty("majorVersion")]
        public int MajorVersion { get; set; }
    }

    public class Library {
        [JsonProperty("downloads")]
        public Downloads Downloads { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rules")]
        public List<Rule> Rules { get; set; }
    }

    public class Logging {
        [JsonProperty("client")]
        public Client Client { get; set; }
    }

    public class Os {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Rule {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("os")]
        public Os Os { get; set; }
    }

    public class Server {
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class ServerMappings {
        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }


}
