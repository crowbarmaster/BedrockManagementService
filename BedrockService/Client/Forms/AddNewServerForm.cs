using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class AddNewServerForm : Form {
        public List<Property> DefaultProps = new List<Property>();
        private readonly List<IServerConfiguration> serverConfigurations;
        private readonly IClientSideServiceConfiguration serviceConfiguration;
        public AddNewServerForm(IClientSideServiceConfiguration serviceConfiguration, List<IServerConfiguration> serverConfigurations) {
            this.serviceConfiguration = serviceConfiguration;
            this.serverConfigurations = serverConfigurations;
            InitializeComponent();
            IServerConfiguration server = new ServerConfigurator(FormManager.MainWindow.connectedHost.GetProp("ServersPath").ToString(), FormManager.MainWindow.connectedHost.GetServerDefaultPropList());
            server.InitializeDefaults();
            DefaultProps = FormManager.MainWindow.connectedHost.GetServerDefaultPropList();
        }

        private void editPropsBtn_Click(object sender, System.EventArgs e) {
            PropEditorForm editSrvDialog = new PropEditorForm();
            if (srvNameBox.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-name").Value = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-port").Value = ipV4Box.Text;
            if (ipV6Box.TextLength > 0)
                DefaultProps.First(prop => prop.KeyName == "server-portv6").Value = ipV6Box.Text;
            editSrvDialog.PopulateBoxes(DefaultProps);
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                DefaultProps = editSrvDialog.workingProps;
                editSrvDialog.Close();
                editSrvDialog.Dispose();
            }
        }

        private void saveBtn_Click(object sender, System.EventArgs e) {
            List<string> usedPorts = new List<string>();
            usedPorts.Add(serviceConfiguration.GetPort());
            foreach (IServerConfiguration serverConfiguration in serverConfigurations) {
                usedPorts.Add(serverConfiguration.GetProp("server-port").ToString());
                usedPorts.Add(serverConfiguration.GetProp("server-portv6").ToString());
            }
            foreach (string port in usedPorts) {
                if (ipV4Box.Text == port || ipV6Box.Text == port) {
                    MessageBox.Show($"You have selected port {port} to use, but this port is already used. Please select another port!");
                    return;
                }
            }
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
