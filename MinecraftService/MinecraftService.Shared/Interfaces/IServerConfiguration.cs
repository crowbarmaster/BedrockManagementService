using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;

namespace MinecraftService.Shared.Interfaces
{
    public interface IServerConfiguration : IBaseConfiguration {
        int GetRunningPid();
        void SetRunningPid(int runningPid);
        string GetServerName();
        string GetConfigFileName();
        void AddStartCommand(string command);
        bool DeleteStartCommand(string command);
        List<StartCmdEntry> GetStartCommands();
        void SetStartCommands(List<StartCmdEntry> newEntries);
        List<Player> GetPlayerList();
        void SetPlayerList(List<Player> playerList);
        Player GetOrCreatePlayer(string xuid, string username = null);
        IServerConfiguration GetServerInfo();
        MinecraftServerArch GetServerArch();
        void SetServerVersion(string newVersion);
        string GetServerVersion();
        void SetStatus(ServerStatusModel status);
        bool IsPrimaryServer();
        ServerStatusModel GetStatus();
        void SetBackupTotals(int totalBackups, int totalSize);
        void SetSettingsProp(ServerPropertyKeys key, string value);
        void SetSettingsProp(string key, string value);
        List<Property> GetSettingsList();
        void SetAllSettings(List<Property> settingsList);
        void UpdateServerProps(string version);
        void ProcessNewServerConfiguration();
        string GetDeployedVersion();
        void SetDeployedVersion(string version);
        IUpdater GetUpdater();
        PlayerManager GetPlayerManager();
        void ValidateDeployedServer();
    }
}
