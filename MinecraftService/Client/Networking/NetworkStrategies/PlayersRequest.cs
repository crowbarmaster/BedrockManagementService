// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Interfaces;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class PlayersRequest : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
            List<Player> fetchedPlayers = JsonConvert.DeserializeObject<List<Player>>(data, SharedStringBase.GlobalJsonSerialierSettings);
            FormManager.MainWindow.RecievePlayerData(message.Data[2], fetchedPlayers);
            return true;
        });
    }
}
