// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MinecraftService.Client.Networking.NetworkStrategies;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Client.Networking {
    internal class NetworkMessageLookup {
        public Dictionary<MessageTypes, INetworkMessage> MessageLookupContainer { get; set; }

        public NetworkMessageLookup(MmsLogger logger, TCPClient client) {
            MessageLookupContainer = new Dictionary<MessageTypes, INetworkMessage>() {
                { MessageTypes.Connect, new ClientConnect(logger, client) },
                { MessageTypes.EnumBackups, new EnumBackups() },
                { MessageTypes.CheckUpdates, new UnlockUI() },
                { MessageTypes.UICallback, new UnlockUI() },
                { MessageTypes.BackupCallback, new BackupCallback() },
                { MessageTypes.VersionListRequest, new VersionListRequest() },
                { MessageTypes.ConsoleLogUpdate, new ConsoleLogUpdate(logger) },
                { MessageTypes.Backup, new Backup(logger) },
                { MessageTypes.PackList, new PackList() },
                { MessageTypes.PlayersRequest, new PlayersRequest() },
                { MessageTypes.LevelEditFile, new LevelEditFile() },
                { MessageTypes.ServerStatusRequest, new ServerStatusRequest() },
                { MessageTypes.ClientReject, new ClientReject() },
                { MessageTypes.ExportFile, new ExportFile() }
            };
        }
    }
}
