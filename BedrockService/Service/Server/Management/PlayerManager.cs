using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace BedrockService.Service.Server.Management
{
    public class PlayerManager : IPlayerManager
    {
        readonly IServerConfiguration serverConfiguration;
        readonly ILogger logger;

        public PlayerManager(IServerConfiguration serverConfiguration, ILogger logger)
        {
            this.serverConfiguration = serverConfiguration;
            this.logger = logger;
        }

        public void ProcessConfiguration(string[] entries)
        {
            foreach (string entry in entries)
            {
                if (entry.StartsWith("#") || string.IsNullOrWhiteSpace(entry))
                    continue;
                string[] split = entry.Split(',');
                logger.AppendLine($"Server \"{serverConfiguration.GetServerName()}\" Loaded registered player: {split[1]}");
                IPlayer playerFound = serverConfiguration.GetPlayerByXuid(split[0]);
                if (playerFound == null)
                {
                    serverConfiguration.AddUpdatePlayer(new Player(split[0], split[1], DateTime.Now.Ticks.ToString(), "0", "0", split[3].ToLower() == "true", split[2], split[4].ToLower() == "true", true));
                    continue;
                }
                UpdatePlayerFromCfg(split[0], split[1], split[2], split[3], split[4]);
            }

        }

        public void PlayerConnected(string username, string xuid)
        {
            IPlayer playerFound = serverConfiguration.GetPlayerByXuid(xuid);
            if (playerFound != null)
            {
                serverConfiguration.AddUpdatePlayer(new Player(username, xuid, DateTime.Now.Ticks.ToString(), "0", "0", false, serverConfiguration.GetProp("default-player-permission-level").ToString(), false, false));
            }
            else
            {
                serverConfiguration.AddUpdatePlayer(playerFound);
            }
        }

        public void PlayerDisconnected(string xuid)
        {
            IPlayer playerFound = serverConfiguration.GetPlayerByXuid(xuid);
            string[] oldTimes = playerFound.GetTimes();
            playerFound.UpdateTimes(oldTimes[1], DateTime.Now.Ticks.ToString());
        }

        public void UpdatePlayerFromCfg(string xuid, string username, string permission, string whitelisted, string ignoreMaxPlayerLimit)
        {
            IPlayer playerFound = serverConfiguration.GetPlayerByXuid(xuid);
            if (playerFound == null)
            {
                playerFound = new Player(serverConfiguration.GetProp("default-player-permission-level").ToString());
                playerFound.Initialize(xuid, username);
            }
            playerFound.UpdateRegistration(permission, whitelisted, ignoreMaxPlayerLimit);
        }

        public IPlayer GetPlayerByXUID(string xuid) => serverConfiguration.GetPlayerByXuid(xuid);

        public void SetPlayer(IPlayer player)
        {
            try
            {
                serverConfiguration.GetPlayerList()[serverConfiguration.GetPlayerList().IndexOf(player)] = player;
            }
            catch
            {
                serverConfiguration.GetPlayerList().Add(player);
            }
        }

        public List<IPlayer> GetPlayers() => serverConfiguration.GetPlayerList();
    }
}
