using System;
using System.Collections.Generic;

namespace MinecraftService.Shared.Classes {
    public class SimpleVersionModel : IEquatable<SimpleVersionModel> {
        public string Version { get; set; }
        public bool IsBeta { get; set; }

        public SimpleVersionModel(string version, bool isBeta) {
            Version = version;
            IsBeta = isBeta;
        }

        public override string ToString() {
            return Version;
        }

        public override bool Equals(object obj) {
            return Equals(obj as SimpleVersionModel);
        }

        public bool Equals(SimpleVersionModel other) {
            return other is not null &&
                   Version == other.Version &&
                   IsBeta == other.IsBeta;
        }

        public static bool operator ==(SimpleVersionModel left, SimpleVersionModel right) {
            return EqualityComparer<SimpleVersionModel>.Default.Equals(left, right);
        }

        public static bool operator !=(SimpleVersionModel left, SimpleVersionModel right) {
            return !(left == right);
        }
    }
}
