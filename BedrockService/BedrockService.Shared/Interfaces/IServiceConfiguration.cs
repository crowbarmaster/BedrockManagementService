﻿using BedrockService.Shared.Classes;
using BedrockService.Shared.JsonModels.LiteLoaderJsonModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Shared.Interfaces {
    public interface IServiceConfiguration : IBedrockConfiguration {
        IServerConfiguration GetServerInfoByName(string serverName);
        IServerConfiguration GetServerInfoByIndex(int index);
        byte GetServerIndex(IServerConfiguration server);
        List<IServerConfiguration> GetServerList();
        List<IPlayer> GetPlayerList();
        Property GetProp(ServicePropertyKeys keyName);
        void SetPlayerList(List<IPlayer> playerList);
        IPlayer GetOrCreatePlayer(string xuid, string username = null);
        void SetAllServerInfos(List<IServerConfiguration> newInfos);
        void AddNewServerInfo(IServerConfiguration serverConfiguration);
        void RemoveServerInfoByIndex(int serverIndex);
        void RemoveServerInfo(IServerConfiguration serverConfiguration);
        List<Property> GetServerDefaultPropList();
        Task CalculateTotalBackupsAllServers();
        (int totalBackups, int totalSize) GetServiceBackupInfo();
        void SetLatestBDSVersion(string version);
        string GetLatestBDSVersion();
        void SetLatestLLVersion(string version);
        string GetLatestLLVersion();
        bool ValidateLatestVersion();
        LLServerPluginRegistry GetPluginRegistry();
        PluginVersionInfo GetServerPluginInfo(int serverIndex, string pluginFilename);
        void SetServerPluginInfo(int serverIndex, PluginVersionInfo info);
    }
}
