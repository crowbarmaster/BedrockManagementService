using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockService.Shared.MincraftJson
{
    internal class WhitelistJsonModel
    {
        public bool ignoresPlayerLimit { get; set; }
        public string permission { get; set; }
        public string xuid { get; set; }

        public WhitelistJsonModel (bool IgnoreLimits, string XUID, string Permission)
        {
            ignoresPlayerLimit = IgnoreLimits;
            xuid = XUID;
            permission = Permission;
        }
    }
}
