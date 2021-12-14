using System;
using BedrockService.Shared.Classes;
using System.Collections.Generic;
using System.Text;

namespace BedrockService.Shared.MincraftJson
{
    internal class PermissionsJsonModel
    {
        public string permission { get; set; }
        public string xuid { get; set; }

        public PermissionsJsonModel(string permission, string xuid)
        {
            this.permission = permission;
            this.xuid = xuid;
        }
    }
}
