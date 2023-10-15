﻿using MinecraftService.Shared.Classes;
using MinecraftService.Shared.SerializeModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinecraftService.Shared.Interfaces {
    public interface IConfigurator {
        Task LoadGlobals();
        Task LoadServerConfigurations();
        void SaveGlobalFile();
        void SavePlayerDatabase(IServerConfiguration server);
        void SaveServerConfiguration(IServerConfiguration server);
        void WriteJSONFiles(IServerConfiguration server);
        Task<List<BackupInfoModel>> EnumerateBackupsForServer(byte serverIndex);
        Task RemoveServerConfigs(IServerConfiguration serverInfo, NetworkMessageFlags flag);
        void DeleteBackupForServer(byte serverIndex, string backupName);
    }
}