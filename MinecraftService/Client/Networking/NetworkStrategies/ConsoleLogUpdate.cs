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
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ConsoleLogUpdate : INetworkMessage {
        private readonly IServerLogger _logger;

        public ConsoleLogUpdate(IServerLogger logger) {
            _logger = logger;
        }

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(messageData, 5, messageData.Length - 5);
            try {
                string[] strings = data.Split("|?|");
                for (int i = 0; i < strings.Length; i++) {
                    string[] srvSplit = strings[i].Split("|;|");
                    string srvName = srvSplit[0];
                    string srvText = srvSplit[1];
                    int srvCurLen = int.Parse(srvSplit[2]);
                    if (srvName != "Service") {
                        IServerConfiguration bedrockServer = FormManager.MainWindow.connectedHost.GetServerInfoByName(srvName);
                        int curCount = bedrockServer.GetLog().Count;
                        if (curCount == srvCurLen) {
                            bedrockServer.GetLog().Add(new LogEntry(srvText));
                        }
                    } else {
                        int curCount = FormManager.MainWindow.connectedHost.GetLog().Count;
                        if (curCount == srvCurLen) {
                            FormManager.MainWindow.connectedHost.GetLog().Add(new LogEntry(srvText));
                        }
                    }
                }
            } catch (Exception e) {
                _logger.AppendLine($"Error updating logs: {e.Message}");
                return false;
            }
            return true;
        });
    }
}
