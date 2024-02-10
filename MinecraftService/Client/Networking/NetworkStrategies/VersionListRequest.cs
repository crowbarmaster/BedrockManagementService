// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class VersionListRequest : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
            Dictionary<SharedStringBase.MinecraftServerArch, SimpleVersionModel[]> versionLists = JsonConvert.DeserializeObject<Dictionary<SharedStringBase.MinecraftServerArch, SimpleVersionModel[]>>(data, SharedStringBase.GlobalJsonSerialierSettings);
            FormManager.MainWindow.VersionListArrived(versionLists);
            return true;
        });
    }
}
