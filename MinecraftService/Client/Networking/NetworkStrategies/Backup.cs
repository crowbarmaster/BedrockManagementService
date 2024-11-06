// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class Backup(MmsLogger logger) : INetworkMessage {

        public Task<bool> ProcessMessage(Message message) => Task.Run(() => {
            MessageFlags msgStatus = (MessageFlags)message.Data[4];
            logger.AppendLine(msgStatus.ToString());
            return true;
        });
    }
}
