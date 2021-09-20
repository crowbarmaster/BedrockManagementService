using Newtonsoft.Json;

namespace BedrockService.Service.Server.HostInfoClasses
{
    public class Player
    {
        public string Username { get; set; }
        public string XUID { get; set; }
        public string PermissionLevel;
        public string[] PermissionLevels = new string[3]
        {
                    "visitor",
                    "member" ,
                    "operator"
        };
        public string FirstConnectedTime { get; set; }
        public string LastConnectedTime { get; set; }
        public string LastDisconnectTime { get; set; }
        public bool Whitelisted { get; set; }
        public bool IgnorePlayerLimits { get; set; }
        public bool FromConfig { get; set; }

        public Player(string xuid, string username, string firstConn, string lastConn, string lastDiscon, string serverDefaultPermission)
        {
            Username = username;
            XUID = xuid;
            FirstConnectedTime = firstConn;
            LastConnectedTime = lastConn;
            LastDisconnectTime = lastDiscon;
            PermissionLevel = serverDefaultPermission;
        }

        [JsonConstructor]
        public Player(string xuid, string username, string firstConn, string lastConn, string lastDiscon, bool whtlist, string perm, bool ignoreLimit, bool fromCfg)
        {
            Username = username;
            XUID = xuid;
            FirstConnectedTime = firstConn;
            LastConnectedTime = lastConn;
            LastDisconnectTime = lastDiscon;
            Whitelisted = whtlist;
            PermissionLevel = perm;
            IgnorePlayerLimits = ignoreLimit;
            FromConfig = fromCfg;
        }

        public Player(string xuid, string username, string serverDefaultPermission)
        {
            Username = username;
            XUID = xuid;
            PermissionLevel = serverDefaultPermission;
        }

        public string CommandStringTranslator (string input)
        {
            if (input == "name" || input == "username" || input == "un")
                return Username;
            if (input == "xuid" || input == "id")
                return XUID;
            if (input == "perm" || input == "permission" || input == "pl")
                return PermissionLevel;
            if (input == "whitelist" || input == "white" || input == "wl")
                return Whitelisted.ToString();
            if (input == "ignoreslimit" || input == "il")
                return IgnorePlayerLimits.ToString();
            return null;
        }
    }

}

