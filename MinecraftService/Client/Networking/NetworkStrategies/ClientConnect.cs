// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ClientConnect(MmsLogger logger, TCPClient client) : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            try {
                string data = Encoding.UTF8.GetString(message.Data);
                if (!string.IsNullOrEmpty(data)) {
                    logger.AppendLine("Connection to Host successful!");
                    FormManager.MainWindow.connectedHost = JsonConvert.DeserializeObject<ServiceConfigurator>(data, SharedStringBase.GlobalJsonSerialierSettings);
                    client.Connected = true;
                    FormManager.MainWindow.RefreshServerBoxContents();
                    FormManager.MainWindow.ServerBusy = false;
                    return true;
                }
            } catch (Exception e) {
                logger.AppendLine($"Error: ConnectMan reported error: {e.Message}\n{e.StackTrace}");
                client.CloseConnection();
                client.Cancel();
                return false;
            }
            return false;
        });
    }
}
