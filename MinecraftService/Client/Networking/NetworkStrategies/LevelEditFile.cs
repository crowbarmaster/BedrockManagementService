// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class LevelEditFile : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
            byte[] stripHeaderFromBuffer = new byte[messageData.Length - 5];
            Buffer.BlockCopy(messageData, 5, stripHeaderFromBuffer, 0, stripHeaderFromBuffer.Length);
            string pathToLevelDat = $@"{SharedStringBase.GetNewTempDirectory()}\level.dat";
            File.WriteAllBytes(pathToLevelDat, stripHeaderFromBuffer);
            FormManager.MainWindow.LevelDatRecieved(pathToLevelDat);
            return true;
        });
    }
}
