// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ClientConnect : INetworkMessage {
        public IServerLogger _logger;
        public TCPClient _client;

        public ClientConnect(IServerLogger logger, TCPClient client) {
            _logger = logger;
            _client = client;
        }

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            try {
                string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
                if (!string.IsNullOrEmpty(data)) {
                    _logger.AppendLine("Connection to Host successful!");
                    FormManager.MainWindow.connectedHost = JsonConvert.DeserializeObject<IServiceConfiguration>(data, SharedStringBase.GlobalJsonSerialierSettings);
                    _client.Connected = true;
                    FormManager.MainWindow.RefreshServerContents();
                    FormManager.MainWindow.ServerBusy = false;
                    return true;
                }
            } catch (Exception e) {
                _logger.AppendLine($"Error: ConnectMan reported error: {e.Message}\n{e.StackTrace}");
                _client.CloseConnection();
                _client.Cancel();
                return false;
            }
            return false;
        });
    }
}
