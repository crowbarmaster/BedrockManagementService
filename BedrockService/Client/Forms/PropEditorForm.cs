using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class PropEditorForm : Form {
        readonly DataGridView dataGrid;
        public List<Property> workingProps;
        public List<StartCmdEntry> startCmds;
        public string RollbackFolderName = "";

        public PropEditorForm() {
            InitializeComponent();
            dataGrid = gridView;
        }

        public void PopulateBoxes(List<Property> propList) {
            int index = 0;
            workingProps = propList;
            this.Text = "Properties Editor";
            foreach (Property prop in workingProps) {
                dataGrid.Rows.Add(new string[2] { prop.KeyName, prop.Value });
                if (prop.KeyName == "server-name" || prop.KeyName == "server-port" || prop.KeyName == "server-portv6") {
                    dataGrid.Rows[index].ReadOnly = true;
                    dataGrid.Rows[index].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
                }
                index++;
            }
            gridView.AllowUserToAddRows = false;
        }

        public void PopulateStartCmds(List<StartCmdEntry> list) {
            this.Text = "Start Command Editor";
            startCmds = list;
            gridView.Columns.RemoveAt(0);
            gridView.AllowUserToDeleteRows = true;
            foreach (StartCmdEntry entry in startCmds) {
                dataGrid.Rows.Add(new string[1] { entry.Command });
            }
        }

        public void EnableBackupManager() {
            this.Text = "Backup Manager";
            gridView.MultiSelect = true;
            DelBackupBtn.Enabled = true;
            DelBackupBtn.Visible = true;
            SaveBtn.Text = "Rollback this date";
        }

        private void CancelBtn_Click(object sender, EventArgs e) {
            Close();
            Dispose();
        }

        private void SaveBtn_Click(object sender, EventArgs e) {
            if (DelBackupBtn.Enabled) {
                if (dataGrid.SelectedRows.Count == 0) {
                    if (dataGrid.SelectedCells.Count == 1) {
                        dataGrid.SelectedCells[0].OwningRow.Selected = true;
                    }
                }
                if (dataGrid.SelectedRows.Count < 2)
                    RollbackFolderName = (string)dataGrid.CurrentRow.Cells[0].Value;
                DialogResult = DialogResult.OK;
                Close();
            }
            startCmds = new List<StartCmdEntry>();
            foreach (DataGridViewRow row in dataGrid.Rows) {
                if (workingProps != null) {
                    foreach (Property prop in workingProps) {
                        if ((string)row.Cells[0].Value == prop.KeyName) {
                            prop.Value = (string)row.Cells[1].Value;
                        }
                    }
                } else {
                    if ((string)row.Cells[0].Value != null)
                        startCmds.Add(new StartCmdEntry((string)row.Cells[0].Value));
                }

            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void gridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e) {
            DataGridViewRow focusedRow = gridView.CurrentRow;
            focusedRow.Cells[0].Value = "Cmd:";
            gridView.Refresh();
        }

        private void DelBackupBtn_Click(object sender, EventArgs e) {
            if (dataGrid.SelectedRows.Count == 0) {
                if (dataGrid.SelectedCells.Count == 1) {
                    dataGrid.SelectedCells[0].OwningRow.Selected = true;
                }
            }
            List<string> removeBackups = new List<string>();
            if (dataGrid.SelectedRows.Count > 0)
                foreach (DataGridViewRow viewRow in dataGrid.SelectedRows) {
                    removeBackups.Add((string)viewRow.Cells[0].Value);
                    dataGrid.Rows.Remove(viewRow);
                }
            DelBackupBtn.Enabled = false;
            BackupBtnEnableAfterDelay();
            JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(removeBackups, Formatting.Indented, settings));
            FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.DelBackups);
        }

        private Task BackupBtnEnableAfterDelay() {
            return Task.Factory.StartNew(() => {
                Task.Delay(1500).Wait();
                Invoke(() => { DelBackupBtn.Enabled = true; });
            });
        }
    }
}
