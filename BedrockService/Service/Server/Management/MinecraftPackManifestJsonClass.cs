using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedrockService.Service.Server.Management
{
    public class MinecraftPackManifestJsonClass
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

        public Manifest ManifestFromFile;
    }
}
