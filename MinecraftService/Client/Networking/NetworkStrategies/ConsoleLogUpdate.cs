// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ConsoleLogUpdate(MmsLogger logger) : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            string data = Encoding.UTF8.GetString(message.Data, 5, message.Data.Length - 5);
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
                logger.AppendLine($"Error updating logs: {e.Message}");
                return false;
            }
            return true;
        });
    }
}
