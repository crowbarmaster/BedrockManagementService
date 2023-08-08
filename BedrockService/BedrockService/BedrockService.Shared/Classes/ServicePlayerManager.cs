using BedrockService.Shared.Interfaces;
using BedrockService.Shared.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Classes {
    public class ServicePlayerManager : IPlayerManager {
        public List<IPlayer> PlayerList = new();

        readonly IServiceConfiguration _serviceConfiguration;
        readonly string _knownDatabasePath;
        readonly string _registeredDatabasePath;

        public ServicePlayerManager(IServiceConfiguration? serviceConfiguration) {
            _serviceConfiguration = serviceConfiguration;
            _knownDatabasePath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerTelem_Name, "Service");
            _registeredDatabasePath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerRegistry_Name, "Service");
        }

        [JsonConstructor]
        public ServicePlayerManager() { }

        public IPlayer PlayerConnected(string username, string xuid) {
            IPlayer playerFound = GetOrCreatePlayer(xuid, username);
            playerFound.UpdateTimes(DateTime.Now.Ticks, playerFound.GetTimes().Disconn);
            return playerFound;
        }

        public IPlayer PlayerDisconnected(string xuid) {
            IPlayer playerFound = GetOrCreatePlayer(xuid);
            playerFound.UpdateTimes(playerFound.GetTimes().Conn, DateTime.Now.Ticks);
            return playerFound;
        }

        public IPlayer GetOrCreatePlayer(string xuid, string username = null) {
            IPlayer foundPlayer = _serviceConfiguration.GetPlayerList().FirstOrDefault(p => p.GetXUID() == xuid);
            if (foundPlayer == null) {
                Player player = new(_serviceConfiguration.GetProp(SharedStringBase.ServicePropertyKeys.DefaultGlobalPermLevel).ToString());
                player.Initialize(xuid, username);
                _serviceConfiguration.GetPlayerList().Add(player);
                return player;
            }
            return foundPlayer;
        }

        public void AddUpdatePlayer(IPlayer player) {
            IPlayer foundPlayer = _serviceConfiguration.GetPlayerList().FirstOrDefault(p => p.GetXUID() == player.GetXUID());
            if (foundPlayer == null) {
                _serviceConfiguration.GetPlayerList().Add(player);
                return;
            }
            _serviceConfiguration.GetPlayerList()[_serviceConfiguration.GetPlayerList().IndexOf(foundPlayer)] = player;
        }

        public List<IPlayer> GetPlayerList() => _serviceConfiguration.GetPlayerList();

        public void SetPlayerList(List<IPlayer> playerList) => _serviceConfiguration.SetPlayerList(playerList);

        private List<string[]> FilterLinesFromFile(string filePath) {
            FileUtilities.CreateInexistantFile(filePath);
            return File.ReadAllLines(filePath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
        }
        
        public void LoadPlayerDatabase() {
            string dbPath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerTelem_Name, "Service");
            string regPath = GetServiceFilePath(BmsFileNameKeys.ServerPlayerRegistry_Name, "Service");
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
