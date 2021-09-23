using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BedrockService.Service.Server.Management
{
    public class PlayerManager
    {
        public void PlayerConnected(string username, string xuid, ServerInfo serverInstance)
        {
            Player playerFound = serverInstance.KnownPlayers.FirstOrDefault(ply => ply.XUID == xuid);
            if (playerFound != null)
            {
                playerFound.LastConnectedTime = DateTime.Now.Ticks.ToString();
                if (playerFound.FirstConnectedTime == null)
                {
                    playerFound.FirstConnectedTime = playerFound.LastConnectedTime;
                }
                InstanceProvider.ConfigManager.SaveKnownPlayerDatabase(serverInstance);
            }
            else
            {
                playerFound = new Player(xuid, username, serverInstance.ServerPropList.FirstOrDefault(k => k.KeyName == "default-player-permission-level").Value)
                {
                    FirstConnectedTime = DateTime.Now.Ticks.ToString()
                };
                playerFound.LastConnectedTime = playerFound.FirstConnectedTime;
                serverInstance.KnownPlayers.Add(playerFound);
                InstanceProvider.ConfigManager.SaveKnownPlayerDatabase(serverInstance);
            }

        }

        public void PlayerDisconnected(string xuid, ServerInfo serverInstance)
        {
            Player playerFound = serverInstance.KnownPlayers.FirstOrDefault(ply => ply.XUID == xuid);
            playerFound.LastDisconnectTime = DateTime.Now.Ticks.ToString();
            InstanceProvider.ConfigManager.SaveKnownPlayerDatabase(serverInstance);
        }

        public void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit, ServerInfo serverInstance)
        {
            Player playerFound = serverInstance.KnownPlayers.FirstOrDefault(ply => ply.XUID == xuid);
            if (playerFound != null)
            {
                playerFound.PermissionLevel = permission;
                playerFound.Whitelisted = bool.Parse(whitelisted);
                playerFound.IgnorePlayerLimits = bool.Parse(ignoreMaxPlayerLimit);
                playerFound.FromConfig = true;
            }
            else
            {
                playerFound = new Player(xuid, username, serverInstance.ServerPropList.FirstOrDefault(k => k.KeyName == "default-player-permission-level").Value)
                {
                    PermissionLevel = permission,
                    Whitelisted = bool.Parse(whitelisted),
                    IgnorePlayerLimits = bool.Parse(ignoreMaxPlayerLimit),
                    FromConfig = true
                };
                serverInstance.KnownPlayers.Add(playerFound);
            }
        }

        public List<Player> ListKnownConfiguredPlayers(ServerInfo serverInstance)
        {
            List<Player> tempList = new List<Player>();
            if (serverInstance.KnownPlayers.Count > 0)
            {
                foreach (Player player in serverInstance.KnownPlayers)
                {
                    if (player.FromConfig)
                        tempList.Add(player);
                }
            }
            return tempList;
        }

        public List<Player> SearchPlayers(string cmd, string arg, string stringToMatch, ServerInfo serverInstance)
        {
            List<Player> tempList = new List<Player>();
            foreach (Player player in serverInstance.KnownPlayers)
            {
                switch (cmd)
                {
                    case "ByName":
                        if (player.Username.Contains(stringToMatch))
                        {
                            tempList.Add(player);
                        }
                        break;
                    case "ByPermLevel":
                        if (player.PermissionLevel == arg)
                        {
                            tempList.Add(player);
                        }
                        break;
                }
            }
            return tempList;
        }

    }
}
