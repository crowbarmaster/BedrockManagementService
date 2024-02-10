// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Client.Properties;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ClientReject : INetworkMessage {

        public Task<bool> ProcessMessage(byte[] _) => Task.Run(() => {
            FormManager.MainWindow.Invoke((MethodInvoker)delegate { FormManager.MainWindow.ServerInfoBox.Text = "Connection attempt rejected by Service!"; });
            FormManager.MainWindow.Invoke((MethodInvoker)delegate { FormManager.MainWindow.HostInfoLabel.Text = "Connection attempt rejected by Service!"; });
            return true;
        });
    }
}
