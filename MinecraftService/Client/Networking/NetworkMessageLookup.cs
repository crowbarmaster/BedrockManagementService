// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MinecraftService.Client.Networking.NetworkStrategies;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Client.Networking {
    internal class NetworkMessageLookup {
        public Dictionary<NetworkMessageTypes, INetworkMessage> MessageLookupContainer { get; set; }

        public NetworkMessageLookup(IServerLogger logger, TCPClient client) {
            MessageLookupContainer = new Dictionary<NetworkMessageTypes, INetworkMessage>() {
                { NetworkMessageTypes.Connect, new ClientConnect(logger, client) },
                { NetworkMessageTypes.EnumBackups, new EnumBackups() },
                { NetworkMessageTypes.CheckUpdates, new UnlockUI() },
                { NetworkMessageTypes.UICallback, new UnlockUI() },
                { NetworkMessageTypes.BackupCallback, new BackupCallback() },
                { NetworkMessageTypes.VersionListRequest, new VersionListRequest() },
                { NetworkMessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(logger) },
                { NetworkMessageTypes.Backup, new Backup(logger) },
                { NetworkMessageTypes.PackList, new PackList() },
                { NetworkMessageTypes.PlayersRequest, new PlayersRequest() },
                { NetworkMessageTypes.LevelEditFile, new LevelEditFile() },
                { NetworkMessageTypes.ServerStatusRequest, new ServerStatusRequest() },
                { NetworkMessageTypes.ClientReject, new ClientReject() },
                { NetworkMessageTypes.ExportFile, new ExportFile() }
            };
        }
    }
}
