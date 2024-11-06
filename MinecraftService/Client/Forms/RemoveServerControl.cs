// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows.Forms;
using MinecraftService.Shared.Classes.Networking;

namespace MinecraftService.Client.Forms {
    public partial class RemoveServerControl : Form {
        public MessageFlags SelectedFlag;
        public RemoveServerControl() {
            InitializeComponent();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            if (checkedListBox1.CheckedItems.Count == 0) {
                return;
            }
            foreach (var item in checkedListBox1.CheckedItems) {
                if (item.ToString() == "Remove server files") {
                    SelectedFlag = MessageFlags.RemoveSrv;
                }
                if (item.ToString() == "Remove server's backups") {
                    if (SelectedFlag == MessageFlags.RemoveSrv) {
                        SelectedFlag = MessageFlags.RemoveBckSrv;
                    } else {
                        SelectedFlag = MessageFlags.RemoveBackups;
                    }
                }
                if (item.ToString() == "Remove player record files") {
                    if (SelectedFlag == MessageFlags.RemoveSrv) {
                        SelectedFlag = MessageFlags.RemovePlySrv;
                    }
                    if (SelectedFlag == MessageFlags.RemoveBckSrv) {
                        SelectedFlag = MessageFlags.RemoveAll;
                    }
                    if (SelectedFlag == MessageFlags.RemoveBackups) {
                        SelectedFlag = MessageFlags.RemoveBckPly;
                    }
                }
            }
            DialogResult = DialogResult.OK;
        }
    }
}
