// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.PackParser;
using Newtonsoft.Json.Linq;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class PackList : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            byte serverIndex = message.ServerIndex;
            string data = Encoding.UTF8.GetString(message.Data);
            List<MinecraftPackContainer> temp = new();
            JArray jArray = JArray.Parse(data);
            foreach (JToken token in jArray)
                temp.Add(token.ToObject<MinecraftPackContainer>());
            Task.Run(() => FormManager.MainWindow.Invoke(() => FormManager.MainWindow.ReceivePackData(serverIndex, temp)));
            return true;
        });
    }
}
