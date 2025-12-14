// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MinecraftService.Shared.Classes.Service.Core;

namespace MinecraftService.Client.Forms {
    public partial class ProgressDialog : Form {
        private readonly IProgress<ProgressModel> _progress;
        private Task _onCompletion;
        private int _closeTimeout = 1500;
        private System.Timers.Timer _closeTimer;

        public ProgressDialog(Task callbackAction) {
            InitializeComponent();
            _closeTimer = new System.Timers.Timer(_closeTimeout);
            _onCompletion = callbackAction;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            _closeTimer.AutoReset = false;
            _progress = new Progress<ProgressModel>((currentProgress) => {
                progressBar.Value = (int)currentProgress.Progress;
                progressLabel.Text = $"{currentProgress.Message}";
                string formattedPercent = string.Format("{0:N2}", currentProgress.Progress);
                if (currentProgress.Progress != 0) {
                    percentLabel.Text = $"{formattedPercent}% completed...";
                }
                Refresh();
            });
        }

        public IProgress<ProgressModel> GetDialogProgress() {
            return _progress;
        }

        public void EndProgress(Task closingActions, int closeTimeout = 1500) {
            _closeTimeout = closeTimeout;
            _closeTimer.Elapsed += (s, e) => {
                if (closingActions != null && closingActions.Status == TaskStatus.Created) {
                    closingActions.Start();
                }
                if (_onCompletion != null && _onCompletion.Status == TaskStatus.Created) {
                    _onCompletion.Start();
                }
            };
            _closeTimer.Start();
        }

        public void SetCallback(Task action) {
            _onCompletion = action;
        }
    }
}
