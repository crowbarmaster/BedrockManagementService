using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class BackupManagerForm : Form {
        IServiceConfiguration _serviceConfig;
        BackupInfoModel _defaultEntry;
        private bool _callbackRecieved;
        private byte[] _backupData;

        public BackupManagerForm() {
            InitializeComponent();
            _serviceConfig = FormManager.MainWindow.connectedHost;
        }

        public void UpdateBackupManagerData () {
            _defaultEntry = new BackupInfoModel(new System.IO.FileInfo("-----.zip"));
            _serviceConfig = FormManager.MainWindow.connectedHost;
            backupSelectBox.Items.Clear();
            if (FormManager.TCPClient.BackupList != null && FormManager.TCPClient.BackupList.Count > 0) {
                foreach (BackupInfoModel model in FormManager.TCPClient.BackupList) {
                    backupSelectBox.Items.Add(model);
                }
            } else {
                backupSelectBox.Items.Add(_defaultEntry);
            }
            backupSelectBox.SelectedIndex = 0;

        }

        private void deleteThisBackupToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 0) {
                return;
            }
            byte[] backupName = Encoding.UTF8.GetBytes(backupSelectBox.Text);
            FormManager.TCPClient.SendData(backupName, _serviceConfig.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.DelBackups);
            Task.Delay(500).Wait();
            backupSelectBox.Items.Remove(backupSelectBox.SelectedItem);
            FormManager.TCPClient.BackupList.Remove((BackupInfoModel)backupSelectBox.SelectedItem);
            UpdateBackupManagerData();
        }

        private void rollbackToThisBackupToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 0) {
                return;
            }
            byte[] backupName = Encoding.UTF8.GetBytes(backupSelectBox.Text);
            FormManager.TCPClient.SendData(backupName, _serviceConfig.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.BackupRollback);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e) {
            if (backupSelectBox.SelectedIndex < 0) {
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Zip File|*.zip";
            saveFileDialog.FileName = backupSelectBox.SelectedItem.ToString();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                byte[] dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(backupSelectBox.SelectedItem));
                _callbackRecieved = false;
                string[] newText = new string[4];
                newText[3] = "Downloading your selection now, please wait!";
                FormManager.TCPClient.SendData(dataBytes, _serviceConfig.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.ExportFile);
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
                FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes("-RemoveAll-"), _serviceConfig.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.DelBackups);
            }
        }

        private void editServerBackupSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            using PropEditorForm editor = new();
            List<Property> filteredList = FormManager.MainWindow.selectedServer.GetSettingsList().Where(x => x.KeyName.Contains("Backup")).ToList();
            editor.PopulateBoxes(filteredList);
            if (editor.ShowDialog() == DialogResult.OK) {
                byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(editor.workingProps));
                FormManager.TCPClient.SendData(serializedBytes, _serviceConfig.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.PropUpdate);
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
            if(backupSelectBox.SelectedIndex < 1) 
            {
                backupSelectBox.SelectedIndex = 0;
            }
            UpdateInfoBox();
        }

        private void UpdateInfoBox() {
            if (backupSelectBox.SelectedIndex < 1) {
                backupSelectBox.SelectedIndex = 0;
            }
            string[] displayTextStrings = new string[8];
            ((BackupInfoModel)backupSelectBox.SelectedItem).GetBackupInfo().CopyTo(displayTextStrings, 0);
            displayTextStrings[6] = $"Total backups contained in this server: {FormManager.MainWindow.selectedServer.GetStatus().TotalBackups}";
            displayTextStrings[7] = $"Total backups size for this server: {FormManager.MainWindow.selectedServer.GetStatus().TotalSizeOfBackups / 1000} MB";
            infoTextBox.Lines = displayTextStrings;
            infoTextBox.Refresh();
        }
    }
}
