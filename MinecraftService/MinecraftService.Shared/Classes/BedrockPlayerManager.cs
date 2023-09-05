using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static MinecraftService.Shared.Classes.SharedStringBase;
#nullable enable
namespace MinecraftService.Shared.Classes {
    public class BedrockPlayerManager : IPlayerManager {
        public List<IPlayer> PlayerList = new();

        readonly IServerConfiguration? _serverConfiguration;
        readonly string? _knownDatabasePath;
        readonly string? _registeredDatabasePath;

        public BedrockPlayerManager(IServerConfiguration? serverConfiguration) {
            if(serverConfiguration == null) {
                throw new ArgumentNullException(nameof(serverConfiguration));
            }
            _serverConfiguration = serverConfiguration;
            _knownDatabasePath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, _serverConfiguration.GetServerName());
            _registeredDatabasePath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, _serverConfiguration.GetServerName());
        }

        [JsonConstructor]
        public BedrockPlayerManager() { }

        public IPlayer PlayerConnected(string username, string xuid) {
            IPlayer playerFound = GetOrCreatePlayer(xuid, username);
            playerFound.UpdateTimes(DateTime.Now.Ticks, playerFound.GetTimes().Disconn);
            return playerFound;
        }

        public IPlayer PlayerDisconnected(string username, string xuid) {
            IPlayer playerFound = GetOrCreatePlayer(xuid);
            playerFound.UpdateTimes(playerFound.GetTimes().Conn, DateTime.Now.Ticks);
            return playerFound;
        }

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = PlayerList.FirstOrDefault(p => p.GetPlayerID() == xuid);
            if (foundPlayer == null) {
                Player player = new(_serverConfiguration.GetProp(MmsDependServerPropKeys.PermLevel).ToString());
                player.Initialize(xuid, username);
                PlayerList.Add(player);
                return player;
            }
            return foundPlayer;
        }

        public void AddUpdatePlayer(IPlayer player) {
            IPlayer foundPlayer = PlayerList.FirstOrDefault(p => p.GetPlayerID() == player.GetPlayerID());
            if (foundPlayer == null) {
                PlayerList.Add(player);
                return;
            }
            PlayerList[PlayerList.IndexOf(foundPlayer)] = player;
        }

        public List<IPlayer> GetPlayerList() => PlayerList ?? new List<IPlayer>();

        public void SetPlayerList(List<IPlayer> playerList) => PlayerList = playerList;

        private List<string[]> FilterLinesFromFile(string filePath) {
            FileUtilities.CreateInexistantFile(filePath);
            return File.ReadAllLines(filePath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
        }

        public void LoadPlayerDatabase() {
            string serverName = _serverConfiguration.GetServerName();
            string dbPath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, serverName);
            string regPath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, serverName);
            List<string[]> playerDbEntries = FilterLinesFromFile(dbPath);
            List<string[]> playerRegEntries = FilterLinesFromFile(regPath);
            playerDbEntries.ForEach(x => {
                GetOrCreatePlayer(x[0]).UpdatePlayerFromDbStrings(x);
            });
            playerRegEntries.ForEach(x => {
                GetOrCreatePlayer(x[0]).UpdatePlayerFromRegStrings(x);
            });
        }

        public void SavePlayerDatabase() {
            TextWriter knownDbWriter = new StreamWriter(_knownDatabasePath);
            TextWriter regDbWriter = new StreamWriter(_registeredDatabasePath);
            regDbWriter.WriteLine("# Registered player list");
            regDbWriter.WriteLine("# Register player entries: PlayerEntry=xuid,username,permission,isWhitelisted,ignoreMaxPlayers");
            regDbWriter.WriteLine("# Example: 1234111222333444,TestUser,visitor,false,false");
            regDbWriter.WriteLine("");

            foreach (IPlayer player in PlayerList) {
                if (player.IsDefaultRegistration()) {
                    knownDbWriter.WriteLine(player.ToString("Known"));
                } else {
                    regDbWriter.WriteLine(player.ToString("Registered"));
                }
            }

            knownDbWriter.Flush();
            knownDbWriter.Close();
            regDbWriter.Flush();
            regDbWriter.Close();
        }

    }
}
