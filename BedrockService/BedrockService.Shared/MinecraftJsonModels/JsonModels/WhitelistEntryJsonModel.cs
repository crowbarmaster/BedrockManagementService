using System;

namespace BedrockService.Shared.MinecraftJsonModels.JsonModels {
    public class WhitelistEntryJsonModel {
        public bool ignoresPlayerLimit { get; set; }
        public string name { get; set; }
        public string xuid { get; set; }

        public WhitelistEntryJsonModel(bool IgnoreLimits, string XUID, string username) {
            ignoresPlayerLimit = IgnoreLimits;
            xuid = XUID;
            name = username;
        }

        public override bool Equals(object obj) {
            return obj is WhitelistEntryJsonModel model &&
                   xuid == model.xuid;
        }

        public override int GetHashCode() {
            return HashCode.Combine(xuid);
        }
    }
}
