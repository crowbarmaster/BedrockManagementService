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
                if (prop.KeyName == "server-name")
                    dataGrid.Rows[index].ReadOnly = true;
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

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
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
    }
}
