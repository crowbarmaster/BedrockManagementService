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
    public class Backup : INetworkMessage {
        private readonly IServerLogger _logger;

        public Backup(IServerLogger logger) {
            _logger = logger;
        }

        public Task<bool> ProcessMessage(byte[] messageData) => Task.Run(() => {
            NetworkMessageFlags msgStatus = (NetworkMessageFlags)messageData[4];
            _logger.AppendLine(msgStatus.ToString());
            return true;
        });
    }
}
