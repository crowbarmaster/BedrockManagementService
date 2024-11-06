// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using System.Windows.Forms;
using MinecraftService.Client.Management;

namespace MinecraftService.Client.Networking.NetworkStrategies {
    public class ClientReject : INetworkMessage {

        public Task<bool> ProcessMessage(Shared.Classes.Networking.Message _) => Task.Run(() => {
            FormManager.MainWindow.Invoke((MethodInvoker)delegate { 
                FormManager.MainWindow.ServerInfoBox.Text = "Connection attempt rejected by Service!";
                FormManager.MainWindow.HostInfoLabel.Text = "Connection attempt rejected by Service!";
            });
            return true;
        });
    }
}
