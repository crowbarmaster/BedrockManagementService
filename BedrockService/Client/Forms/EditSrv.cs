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


        public EditSrv()
        {
            InitializeComponent();
            dataGrid = gridView;
        }

        public void PopulateBoxes(List<Property> propList)
        {
            workingProps = propList;
            foreach (Property prop in workingProps)
            {
                dataGrid.Rows.Add(new string[] { prop.KeyName, prop.Value });
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                foreach (Property prop in workingProps)
                {
                    if ((string)row.Cells[0].Value == prop.KeyName)
                    {
                        if (prop.KeyName != "server-name")
                        {
                            prop.Value = (string)row.Cells[1].Value;
                        }
                    }
                }
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
