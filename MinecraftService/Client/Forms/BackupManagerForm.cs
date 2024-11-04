// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using Newtonsoft.Json;

namespace MinecraftService.Client.Forms {
    public partial class BackupManagerForm : Form {
        ServiceConfigurator _serviceConfig;
        BackupInfoModel _defaultEntry;
        private bool _callbackRecieved;
        private byte[] _backupData;

        public BackupManagerForm() {
            InitializeComponent();
            _serviceConfig = FormManager.MainWindow.connectedHost;
        }

        public void UpdateBackupManagerData(List<BackupInfoModel> backupInfo) {
            _defaultEntry = new BackupInfoModel();
            _serviceConfig = FormManager.MainWindow.connectedHost;
            backupSelectBox.Items.Clear();
            if (backupInfo != null && backupInfo.Count > 0) {
                backupSelectBox.Items.AddRange(backupInfo.ToArray());
            } else {
                backupSelectBox.Items.Add(_defaultEntry);
            }
            backupSelectBox.SelectedIndex = 0;

        }

        public void MarkRollbackComplete(bool backupPassed) {
            Invoke(() => {
                string backupResult = backupPassed ? "completed successfully" : "failed";
                infoTextBox.Lines = new string[4] { string.Empty, string.Empty, string.Empty, $"Rollback has {backupResult}!" };
                closeBtn.Enabled = true;
            });
        }

        private void deleteThisBackupToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 0) {
                return;
            }
            byte[] backupName = Encoding.UTF8.GetBytes(backupSelectBox.Text);
            FormManager.TCPClient.SendData(backupName, _serviceConfig.GetServerIndex(FormManager.MainWindow.SelectedServer), NetworkMessageTypes.DelBackups);
            Task.Delay(500).Wait();
            backupSelectBox.Items.Remove(backupSelectBox.SelectedItem);
        }

        private void rollbackToThisBackupToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 0) {
                return;
            }
            closeBtn.Enabled = false;
            infoTextBox.Lines = new string[4] { string.Empty, string.Empty, string.Empty, "Now rolling back to selected backup... Please wait!" };
            byte[] backupName = Encoding.UTF8.GetBytes(backupSelectBox.Text);
            FormManager.TCPClient.SendData(backupName, _serviceConfig.GetServerIndex(FormManager.MainWindow.SelectedServer), NetworkMessageTypes.BackupRollback);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 0) {
                return;
            }
            BackupInfoModel selectedBackup = backupSelectBox.SelectedItem as BackupInfoModel;
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Zip File|*.zip";
            saveFileDialog.FileName = backupSelectBox.SelectedItem.ToString();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                ExportImportFileModel exportModel = new();
                exportModel.Filename = selectedBackup.Filename;
                exportModel.FileType = FileTypeFlags.Backup;
                byte[] dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(exportModel));
                _callbackRecieved = false;
                string[] newText = new string[4];
                newText[3] = "Downloading your selection now, please wait!";
                FormManager.TCPClient.SendData(dataBytes, _serviceConfig.GetServerIndex(FormManager.MainWindow.SelectedServer), NetworkMessageTypes.ExportFile);
                while (!_callbackRecieved) {
                    Task.Delay(500).Wait();
                }
                if (_backupData != null) {
                    File.WriteAllBytes(saveFileDialog.FileName, _backupData);
                }
                newText[3] = "File download completed!";
                infoTextBox.Lines = newText;
            }
        }

        private void purgeBackupsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (MessageBox.Show("This will remove all backups from the server. This is irrevesable!!", "Confirm removal", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) {
                FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes("-RemoveAll-"), _serviceConfig.GetServerIndex(FormManager.MainWindow.SelectedServer), NetworkMessageTypes.DelBackups);
            }
        }

        private void editServerBackupSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            using PropEditorForm editor = new();
            List<Property> filteredList = new List<Property>(FormManager.MainWindow.SelectedServer.GetSettingsList().Where(x => x.KeyName.Contains("Backup")).ToList());
            editor.PopulateBoxes(filteredList);
            if (editor.ShowDialog() == DialogResult.OK) {
                byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(editor.workingProps));
                FormManager.TCPClient.SendData(serializedBytes, _serviceConfig.GetServerIndex(FormManager.MainWindow.SelectedServer), NetworkMessageTypes.PropUpdate);
                MessageBox.Show("Backup settings sent to service! Restart server to apply.");
            }
        }

        public void RecieveExportData(byte[] data) {
            _backupData = data;
            _callbackRecieved = true;
        }

        private void closeBtn_Click(object sender, EventArgs e) {
            Close();
            Dispose();
        }

        private void backupSelectBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 1) {
                backupSelectBox.SelectedIndex = 0;
            }
            UpdateInfoBox();
        }

        private void UpdateInfoBox() {
            if (backupSelectBox.SelectedIndex < 1) {
                backupSelectBox.SelectedIndex = 0;
            }
            if (backupSelectBox.SelectedItem != null && closeBtn.Enabled) {
                string[] displayTextStrings = new string[8];
                ((BackupInfoModel)backupSelectBox.SelectedItem).GetBackupInfo().CopyTo(displayTextStrings, 0);
                displayTextStrings[6] = $"Total backups contained in this server: {FormManager.MainWindow.SelectedServer.GetStatus().TotalBackups}";
                displayTextStrings[7] = $"Total backups size for this server: {FormManager.MainWindow.SelectedServer.GetStatus().TotalSizeOfBackups / 1000} MB";
                infoTextBox.Lines = displayTextStrings;
                infoTextBox.Refresh();
            }
        }
    }
}
