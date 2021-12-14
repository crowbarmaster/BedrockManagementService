using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;

namespace BedrockService.Shared.Classes
{
    public class Player : IPlayer
    {
        [JsonProperty]
        private string Username { get; set; }
        [JsonProperty]
        private string XUID { get; set; }
        [JsonProperty]
        private string PermissionLevel;
        [JsonProperty]
        private string FirstConnectedTime { get; set; }
        [JsonProperty]
        private string LastConnectedTime { get; set; }
        [JsonProperty]
        private string LastDisconnectTime { get; set; }
        [JsonProperty]
        private string ServerDefaultPerm { get; set; }
        [JsonProperty]
        private bool Whitelisted { get; set; }
        [JsonProperty]
        private bool IgnorePlayerLimits { get; set; }
        [JsonProperty]
        private bool FromConfig { get; set; }

        [JsonConstructor]
        public Player(string xuid, string username, string firstConn, string lastConn, string lastDiscon, bool whtlist, string perm, bool ignoreLimit)
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

        public Player(string serverDefaultPermission)
        {
            ServerDefaultPerm = serverDefaultPermission;
            PermissionLevel = serverDefaultPermission;
        }

        public void Initialize(string xuid, string username)
        {
            XUID = xuid;
            Username = username;
        }

        public string GetUsername() => Username;

        public string SearchForProperty(string input)
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

        public string GetXUID() => XUID;

        public void UpdateTimes(string lastConn, string lastDiscon)
        {
            if (FirstConnectedTime == "")
                FirstConnectedTime = DateTime.Now.Ticks.ToString();
            LastConnectedTime = lastConn;
            LastDisconnectTime = lastDiscon;
        }

        public void UpdateRegistration(string whtlist, string perm, string ignoreLimit)
        {
            Whitelisted = bool.Parse(whtlist);
            PermissionLevel = perm;
            IgnorePlayerLimits = bool.Parse(ignoreLimit);
        }

        public (string First, string Conn, string Disconn) GetTimes()
        {
            return ( FirstConnectedTime, LastConnectedTime, LastDisconnectTime );
        }

        public bool IsDefaultRegistration()
        {
            return Whitelisted == false && IgnorePlayerLimits == false && PermissionLevel == ServerDefaultPerm;
        }

        public string ToString(string format)
        {
            if (format == "Known")
            {
                return $"{XUID},{Username},{FirstConnectedTime},{LastConnectedTime},{LastDisconnectTime}";
            }
            if (format == "Registered")
            {
                return $"{XUID},{Username},{PermissionLevel},{Whitelisted},{IgnorePlayerLimits}";
            }
            return null;
        }

        public bool IsPlayerWhitelisted() => Whitelisted;

        public bool PlayerIgnoresLimit() => IgnorePlayerLimits;

        public string GetPermissionLevel() => PermissionLevel;
    }

}