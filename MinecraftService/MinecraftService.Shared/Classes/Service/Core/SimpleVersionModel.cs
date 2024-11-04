using System;
using System.Collections.Generic;

namespace MinecraftService.Shared.Classes.Service.Core
{
    public class SimpleVersionModel : IEquatable<SimpleVersionModel>
    {
        public string Version { get; set; }
        public bool IsBeta { get; set; }

        public SimpleVersionModel(string version, bool isBeta)
        {
            Version = version;
            IsBeta = isBeta;
        }

        public override string ToString()
        {
            return Version;
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleVersionModel model &&
                   Version == model.Version &&
                   IsBeta == model.IsBeta;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Version, IsBeta);
        }

        public bool Equals(SimpleVersionModel other)
        {
            return Equals(other);
        }

        public static bool operator ==(SimpleVersionModel left, SimpleVersionModel right)
        {
            return EqualityComparer<SimpleVersionModel>.Default.Equals(left, right);
        }

        public static bool operator !=(SimpleVersionModel left, SimpleVersionModel right)
        {
            return !(left == right);
        }
    }
}
