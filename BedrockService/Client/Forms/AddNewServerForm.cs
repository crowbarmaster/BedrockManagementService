using System.Collections.Generic;
using BedrockService.Service.Server.HostInfoClasses;
using System.Linq;
using System.Windows.Forms;
using BedrockService.Client.Utilities;
using BedrockService.Service.Networking;

namespace BedrockService.Client.Forms
{
    public partial class AddNewServerForm : Form
    {
        public List<Property> DefaultProps = new List<Property>();
        public AddNewServerForm()
        {
            InitializeComponent();
            ServerInfo server = new ServerInfo();
            server.InitDefaults("Default");
            DefaultProps = server.ServerPropList;
        }

        private void editPropsBtn_Click(object sender, System.EventArgs e)
        {
            EditSrv editSrvDialog = new EditSrv();
            if (srvNameBox.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-name").Value = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-port").Value = ipV4Box.Text;
            if (ipV6Box.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-portv6").Value = ipV6Box.Text;
            editSrvDialog.PopulateBoxes(DefaultProps);
            if (editSrvDialog.ShowDialog() == DialogResult.OK)
            {
                DefaultProps = editSrvDialog.workingProps;
                editSrvDialog.Close();
                editSrvDialog.Dispose();
            }
        }

        private void saveBtn_Click(object sender, System.EventArgs e)
        {
            if (srvNameBox.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-name").Value = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-port").Value = ipV4Box.Text;
            if (ipV6Box.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-portv6").Value = ipV6Box.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
