// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;

namespace MinecraftService.Client.Forms {
    public partial class NewPlayerRegistrationForm : Form {
        public Player PlayerToAdd;
        private readonly IServerConfiguration _serverConfiguration;

        public NewPlayerRegistrationForm() {
            InitializeComponent();
            _serverConfiguration = FormManager.MainWindow.SelectedServer;
        }

        private void saveClick(object sender, EventArgs e) {
            long curTime = DateTime.Now.Ticks;
            if (usernameTextBox.TextLength > 0 && xuidTextBox.TextLength == 16 || xuidTextBox.TextLength == 36 || xuidTextBox.TextLength == 32) {
                PlayerToAdd =
                    new Player(xuidTextBox.Text, usernameTextBox.Text, curTime, curTime, curTime, whitelistedChkBox.Checked,
                    _serverConfiguration.GetServerArch() == SharedStringBase.MinecraftServerArch.Java ?
                    GetJavaPermLevel(permissionComboBox.SelectedItem.ToString()) :
                    permissionComboBox.SelectedItem.ToString(),
                    ignoreLimitChkBox.Checked);
                DialogResult = DialogResult.OK;
            }
        }

        private string GetJavaPermLevel(string permLevel) {
            if (permLevel == "visitor") {
                return "2";
            }
            if (permLevel == "member") {
                return "3";
            }
            if (permLevel == "op") {
                return "4";
            }
            return "1";
        }
    }
}
