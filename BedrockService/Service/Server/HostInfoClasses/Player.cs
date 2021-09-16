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

        public Player(string xuid, string username, string firstConn, string lastConn, string lastDiscon)
        {
            Username = username;
            XUID = xuid;
            FirstConnectedTime = firstConn;
            LastConnectedTime = lastConn;
            LastDisconnectTime = lastDiscon;
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
        }

        public Player(string xuid, string username)
        {
            Username = username;
            XUID = xuid;
        }
    }

}

