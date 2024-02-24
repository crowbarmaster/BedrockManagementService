﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MinecraftService.Client.Management;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class BackupCallback : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            bool rollbackPassed = messageData[5] == 1;
            FormManager.MainWindow.BackupRollbackCompleted(rollbackPassed);
            FormManager.MainWindow.ServerBusy = false;
            return true;
        });
    }
}
