using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MinecraftService.Shared.JsonModels.MinecraftJsonModels {
    public class JavaVersionHistoryModel {
        [JsonProperty("latest")]
        public Latest Latest { get; set; }

        [JsonProperty("versions")]
        public List<Version> Versions { get; set; }
    }

    public class Latest {
        [JsonProperty("release")]
        public string Release { get; set; }

        [JsonProperty("snapshot")]
        public string Snapshot { get; set; }
    }

    public class Version {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("releaseTime")]
        public DateTime ReleaseTime { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("complianceLevel")]
        public int ComplianceLevel { get; set; }
    }


}
