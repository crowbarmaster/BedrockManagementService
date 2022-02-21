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
                dataGrid.Rows.Add(new string[2] { prop.KeyName, prop.StringValue });
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

        private void CancelBtn_Click(object sender, EventArgs e) {
            Close();
            Dispose();
        }

        private void SaveBtn_Click(object sender, EventArgs e) {
            startCmds = new List<StartCmdEntry>();
            foreach (DataGridViewRow row in dataGrid.Rows) {
                if (workingProps != null) {
                    foreach (Property prop in workingProps) {
                        if ((string)row.Cells[0].Value == prop.KeyName) {
                            prop.StringValue = (string)row.Cells[1].Value;
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

        internal void PopulateBoxes(object p) {
            throw new NotImplementedException();
        }
    }
}
