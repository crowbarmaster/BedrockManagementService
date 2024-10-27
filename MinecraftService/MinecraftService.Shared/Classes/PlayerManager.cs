using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Classes {
    public class PlayerManager {
        public List<IPlayer> PlayerList = [];
        public readonly string Parent;
        public readonly string DefaultPerm;
        public readonly string KnownDatabasePath;
        public readonly string RegisteredDatabasePath;
        public PlayerManager(string parent, string defaultPerm) {
            Parent = parent;
            DefaultPerm = defaultPerm;
            KnownDatabasePath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, parent);
            RegisteredDatabasePath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, parent);
        }

        public IPlayer PlayerConnected(string username, string uuid) {
            IPlayer playerFound = GetOrCreatePlayer(uuid, username);
            playerFound.UpdateTimes(DateTime.Now.Ticks, playerFound.GetTimes().Disconn);
            return playerFound;
        }

        public IPlayer PlayerDisconnected(string username, string uuid) {
            IPlayer playerFound = GetOrCreatePlayer(uuid, username);
            playerFound.UpdateTimes(playerFound.GetTimes().Conn, DateTime.Now.Ticks);
            return playerFound;
        }

        public IPlayer? GetOrCreatePlayer(string uid, string username = "") {
            IPlayer? foundPlayer = PlayerList.FirstOrDefault(p => p.GetPlayerID() == uid);
            Player player = new();
            if (uid.Length > 16) {
                if (foundPlayer == null && username != "") {
                    foundPlayer = PlayerList.FirstOrDefault(ply => ply.GetUsername() == username);
                }
            } else {
                player = new(DefaultPerm);
            }
            if (foundPlayer == null) {
                if (uid != string.Empty) {
                    player.Initialize(uid, username);
                    PlayerList.Add(player);
                    return player;
                } else {
                    return null;
                }
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

        public List<string[]> FilterLinesFromFile(string filePath) {
            FileUtilities.CreateInexistantFile(filePath);
            return File.ReadAllLines(filePath)
                    .Where(x => !x.StartsWith("#"))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Split(','))
                    .ToList();
        }

        public void LoadPlayerDatabase() {
            List<IPlayer> playerList = [];
            string dbPath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerTelem_Name, Parent);
            string regPath = GetServiceFilePath(MmsFileNameKeys.ServerPlayerRegistry_Name, Parent);
            List<string[]> playerDbEntries = FilterLinesFromFile(dbPath);
            List<string[]> playerRegEntries = FilterLinesFromFile(regPath);
            playerDbEntries.ForEach(x => {
                new Player(DefaultPerm).UpdatePlayerFromDbStrings(x);
            });
            playerRegEntries.ForEach(x => {
                new Player(DefaultPerm).UpdatePlayerFromDbStrings(x);
            });
        }

        public void SavePlayerDatabase() {
            TextWriter knownDbWriter = new StreamWriter(KnownDatabasePath);
            TextWriter regDbWriter = new StreamWriter(RegisteredDatabasePath);
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


        public PlayerManager() { }
    }
}
