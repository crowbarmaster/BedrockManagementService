using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;

namespace BedrockService.Shared.Classes
{
    public class Player : IPlayer
    {
        private string Username { get; set; }
        private string XUID { get; set; }
        private string PermissionLevel;
        private string FirstConnectedTime { get; set; }
        private string LastConnectedTime { get; set; }
        private string LastDisconnectTime { get; set; }
        private string ServerDefaultPerm { get; set; }
        private bool Whitelisted { get; set; }
        private bool IgnorePlayerLimits { get; set; }
        private bool FromConfig { get; set; }

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

        public string[] GetTimes()
        {
            return new string[] { FirstConnectedTime, LastConnectedTime, LastDisconnectTime };
        }

        public string[] GetRegistration()
        {
            return new string[] { Whitelisted.ToString(), PermissionLevel, IgnorePlayerLimits.ToString() };
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
                return $"{XUID},{Username},{Whitelisted},{PermissionLevel},{IgnorePlayerLimits}";
            }
            return null;
        }
    }

}