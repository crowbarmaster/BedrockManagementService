using MinecraftService.Shared.Classes;
using MinecraftService.Shared.JsonModels.LiteLoaderJsonModels;
using MinecraftService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Shared.Interfaces {
    public interface IServerConfiguration : IBaseConfiguration {
        int GetRunningPid();
        void SetRunningPid(int runningPid);
        string GetServerName();
        string GetConfigFileName();
        void AddStartCommand(string command);
        bool DeleteStartCommand(string command);
        List<StartCmdEntry> GetStartCommands();
        void SetStartCommands(List<StartCmdEntry> newEntries);
        List<IPlayer> GetPlayerList();
        void SetPlayerList(List<IPlayer> playerList);
        IPlayer GetOrCreatePlayer(string xuid, string username = null);
        IServerConfiguration GetServerInfo();
        MinecraftServerArch GetServerArch();
        LiteLoaderConfigNodeModel GetLiteLoaderConfig();
        void SetLiteLoaderConfig(LiteLoaderConfigNodeModel config);
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
        bool ValidateServerPropFile(string version);
        void UpdateServerProps(string version);
        void ProcessNewServerConfiguration();
        string GetDeployedVersion();
        void SetDeployedVersion(string version);
        IUpdater GetUpdater();
        IPlayerManager GetPlayerManager();
        void ValidateDeployedServer();
    }
}
