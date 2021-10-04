using BedrockService.Client.Management;
using BedrockService.Service.Networking;
using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class EditSrv : Form
    {
        DataGridView dataGrid;
        public List<Property> workingProps;
        public List<StartCmdEntry> startCmds;
        public string RollbackFolderName = "";

        public EditSrv()
        {
            InitializeComponent();
            dataGrid = gridView;
        }

        public void PopulateBoxes(List<Property> propList)
        {
            int index = 0;
            workingProps = propList;
            foreach (Property prop in workingProps)
            {
                dataGrid.Rows.Add(new string[2] { prop.KeyName, prop.Value });
                if (prop.KeyName == "server-name" || prop.KeyName == "server-port" || prop.KeyName == "server-portv6")
                {
                    dataGrid.Rows[index].ReadOnly = true;
                    dataGrid.Rows[index].DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
                }
                index++;
            }
            gridView.AllowUserToAddRows = false;
        }

        public void PopulateStartCmds(List<StartCmdEntry> list)
        {
            startCmds = list;
            gridView.Columns.RemoveAt(0);
            foreach (StartCmdEntry entry in startCmds)
            {
                dataGrid.Rows.Add(new string[1] { entry.Command });
            }
        }

        public void EnableBackupManager()
        {
            gridView.MultiSelect = true;
            DelBackupBtn.Enabled = true;
            DelBackupBtn.Visible = true;
            SaveBtn.Text = "Rollback this date";
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (DelBackupBtn.Enabled)
            {
                if (dataGrid.SelectedRows.Count < 2)
                    RollbackFolderName = (string)dataGrid.CurrentRow.Cells[0].Value;
                DialogResult = DialogResult.OK;
                Close();
            }
            startCmds = new List<StartCmdEntry>();
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (workingProps != null)
                {
                    foreach (Property prop in workingProps)
                    {
                        if ((string)row.Cells[0].Value == prop.KeyName)
                        {
                            prop.Value = (string)row.Cells[1].Value;
                        }
                    }
                }
                else
                {
                    if ((string)row.Cells[0].Value != null)
                        startCmds.Add(new StartCmdEntry((string)row.Cells[0].Value));
                }

            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void gridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            DataGridViewRow focusedRow = gridView.CurrentRow;
            focusedRow.Cells[0].Value = "Cmd:";
            gridView.Refresh();
        }

        private void DelBackupBtn_Click(object sender, EventArgs e)
        {
            List<string> removeBackups = new List<string>();
            if (dataGrid.SelectedRows.Count > 0)
                foreach (DataGridViewRow viewRow in dataGrid.SelectedRows)
                    removeBackups.Add((string)viewRow.Cells[0].Value);
            Utilities.JsonUtilities.SendJsonMsgToSrv<List<string>>(removeBackups, NetworkMessageDestination.Service, (byte)FormManager.GetMainWindow.connectedHost.Servers.IndexOf(FormManager.GetMainWindow.selectedServer), NetworkMessageTypes.DelBackups);

        }
    }
}
