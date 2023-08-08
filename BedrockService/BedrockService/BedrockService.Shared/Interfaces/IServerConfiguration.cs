using BedrockService.Shared.Classes;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using BedrockService.Shared.SerializeModels;
using System;
using System.Collections.Generic;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Interfaces {
    public interface IServerConfiguration : IBedrockConfiguration {
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
        ServerStatusModel GetStatus();
        void SetBackupTotals(int totalBackups, int totalSize);
        void SetSettingsProp(SharedStringBase.ServerPropertyKeys key, string value);
        void SetSettingsProp(string key, string value);
        List<Property> GetSettingsList();
        void SetAllSettings(List<Property> settingsList);
        void UpdateServerProps(string version);
        string GetDeployedVersion();
        void SetDeployedVersion(string version);
        IUpdater GetUpdater();
        void ValidateDeployedServer();
    }
}
