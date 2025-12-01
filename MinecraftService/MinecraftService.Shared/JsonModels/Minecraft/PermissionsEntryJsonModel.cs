using System;

namespace MinecraftService.Shared.JsonModels.Minecraft {
    public class PermissionsEntryJsonModel {
        public string xuid { get; set; }
        public string permission { get; set; }

        public PermissionsEntryJsonModel(string permission, string xuid) {
            this.permission = permission;
            this.xuid = xuid;
        }

        public override bool Equals(object obj) {
            return obj is PermissionsEntryJsonModel model &&
                   xuid == model.xuid;
        }

        public override int GetHashCode() {
            return HashCode.Combine(xuid);
        }
    }
}
