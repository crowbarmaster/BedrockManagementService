// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MinecraftService.Shared.Classes;

namespace MinecraftService.Client.Forms {
    public partial class ClientProgressDialog : Form {
        private readonly IProgress<ProgressModel> _progress;
        private System.Timers.Timer _closeTimer = new System.Timers.Timer(1500);

        public ClientProgressDialog() {
            InitializeComponent();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            _closeTimer.AutoReset = false;
            _closeTimer.Elapsed += (s, e) => {
                Invoke(Close);
            };
            _progress = new Progress<ProgressModel>((currentProgress) => {
                progressBar.Value = (int)currentProgress.Progress;
                progressLabel.Text = $"{currentProgress.Message}";
                string formattedPercent = string.Format("{0:N2}", currentProgress.Progress);
                if(currentProgress.Progress != 0) {
                    percentLabel.Text = $"{formattedPercent}% completed...";
                }
                Refresh();
            });
        }

        public IProgress<ProgressModel> GetDialogProgress() {
            return _progress;
        }

        public void EndProgress() {
            _closeTimer.Start();
        }
    }
}
