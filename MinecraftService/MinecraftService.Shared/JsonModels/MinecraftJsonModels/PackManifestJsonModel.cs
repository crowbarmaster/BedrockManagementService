using System;
using System.Collections.Generic;

namespace MinecraftService.Shared.JsonModels.MinecraftJsonModels {
    public class PackManifestJsonModel {
        public int format_version { get; set; }
        public Header header { get; set; }
        public List<Module> modules { get; set; }
        public List<Dependency> dependencies { get; set; }

        public override bool Equals(object obj) {
            return obj is PackManifestJsonModel model &&
                   EqualityComparer<Header>.Default.Equals(header, model.header);
        }

        public override int GetHashCode() {
            return HashCode.Combine(header);
        }

        public class Header {
            public string description { get; set; }
            public string name { get; set; }
            public string uuid { get; set; }
            public List<int> version { get; set; }

            public override bool Equals(object obj) {
                return obj is Header header &&
                       uuid == header.uuid;
            }

            public override int GetHashCode() {
                return HashCode.Combine(uuid);
            }
        }

        public class Module {
            public string description { get; set; }
            public string type { get; set; }
            public string uuid { get; set; }
            public List<int> version { get; set; }
        }

        public class Dependency {
            public string uuid { get; set; }
            public List<int> version { get; set; }
        }
    }
}
