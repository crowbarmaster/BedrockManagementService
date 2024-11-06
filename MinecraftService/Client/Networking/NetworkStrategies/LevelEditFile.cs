// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class LevelEditFile : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            string pathToLevelDat = $@"{SharedStringBase.GetNewTempDirectory("LevelEdit")}\level.dat";
            File.WriteAllBytes(pathToLevelDat, message.Data);
            FormManager.MainWindow.LevelDatRecieved(pathToLevelDat);
            return true;
        });
    }
}
