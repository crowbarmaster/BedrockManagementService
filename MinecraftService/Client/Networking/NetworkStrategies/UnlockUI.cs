// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftService.Client.Management;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class UnlockUI : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] _) => Task.Run(() => {
            FormManager.MainWindow.ServerBusy = false;
            return true;
        });
    }
}
