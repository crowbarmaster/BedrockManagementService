// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class VersionListRequest : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            Dictionary<SharedStringBase.MinecraftServerArch, SimpleVersionModel[]> versionLists = JsonConvert.DeserializeObject<Dictionary<SharedStringBase.MinecraftServerArch, SimpleVersionModel[]>>(data, SharedStringBase.GlobalJsonSerialierSettings);
            FormManager.MainWindow.VersionListArrived(versionLists);
            return true;
        });
    }
}
