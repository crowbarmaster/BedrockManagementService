using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockService.Shared.MincraftJson
{
    internal class KnownPacksJsonModel
    {
        public int file_version { get; set; }
        public string file_system { get; set; }
        public bool? from_disk { get; set; }
        public List<string> hashes { get; set; }
        public string path { get; set; }
        public string uuid { get; set; }
        public string version { get; set; }

        public override string ToString()
        {
            return path;
        }
    }
}
