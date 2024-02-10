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
using MinecraftService.Shared.PackParser;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class PackList : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            byte serverIndex = messageData[2];
            string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
            List<MinecraftPackContainer> temp = new();
            JArray jArray = JArray.Parse(data);
            foreach (JToken token in jArray)
                temp.Add(token.ToObject<MinecraftPackContainer>());
            Task.Run(() => FormManager.MainWindow.Invoke(() => FormManager.MainWindow.RecievePackData(serverIndex, temp)));
            return true;
        });
    }
}
