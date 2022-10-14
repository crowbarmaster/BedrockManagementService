using BedrockService.Shared.Classes;
using BedrockService.Shared.SerializeModels;
using System.Collections.Generic;

namespace BedrockService.Shared.Interfaces {
    public interface IServerConfiguration : IBedrockConfiguration {
        string GetServerName();
        string GetFileName();
        void AddStartCommand(string command);
        bool DeleteStartCommand(string command);
        List<StartCmdEntry> GetStartCommands();
        void SetStartCommands(List<StartCmdEntry> newEntries);
        void AddUpdatePlayer(IPlayer player);
        IPlayer GetOrCreatePlayer(string xuid, string username = null);
        List<IPlayer> GetPlayerList();
        void SetPlayerList(List<IPlayer> newList);
        IServerConfiguration GetServerInfo();
        void SetServerVersion(string newVersion);
        string GetServerVersion();
        string GetSelectedVersion();
        void SetStatus(ServerStatusModel status);
        ServerStatusModel GetStatus();
        void SetBackupTotals(int totalBackups, int totalSize);
        Property GetSettingsProp(string name);
        void SetSettingsProp(string name, string value);
        List<Property> GetSettingsList();
        void SetAllSettings(List<Property> settingsList);
        bool ValidateVersion(string version, bool skipNullCheck = false);
        void SetLiteLoaderStatus(bool statusToSet);
        bool GetLiteLoaderStatus();
    }
}
