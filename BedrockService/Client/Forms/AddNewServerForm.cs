using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class AddNewServerForm : Form {
        public ServerCombinedPropModel ServerCombinedPropModel = new();
        private readonly List<IServerConfiguration> serverConfigurations;
        private readonly IClientSideServiceConfiguration serviceConfiguration;
        private readonly List<string> serviceConfigExcludeList = new List<string>() { "ServerName", "ServerExeName", "FileName", "ServerPath", "DeployedVersion" };

        public AddNewServerForm(IClientSideServiceConfiguration serviceConfiguration, List<IServerConfiguration> serverConfigurations) {
            this.serviceConfiguration = serviceConfiguration;
            this.serverConfigurations = serverConfigurations;
            InitializeComponent();
            IServerConfiguration server = new ServerConfigurator(FormManager.processInfo, FormManager.Logger, FormManager.MainWindow.connectedHost);
            server.InitializeDefaults();
            ServerCombinedPropModel.ServerPropList = FormManager.MainWindow.connectedHost.GetServerDefaultPropList();
            ServerCombinedPropModel.ServicePropList = server.GetSettingsList();
            versionTextBox.Text = FormManager.MainWindow.connectedHost.GetLatestBDSVersion();
        }

        private void editPropsBtn_Click(object sender, System.EventArgs e) {
            using PropEditorForm editSrvDialog = new PropEditorForm();
            if (srvNameBox.TextLength > 0)
                ServerCombinedPropModel.ServerPropList.First(prop => prop.KeyName == "server-name").StringValue = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                ServerCombinedPropModel.ServerPropList.First(prop => prop.KeyName == "server-port").StringValue = ipV4Box.Text;
            if (ipV6Box.TextLength > 0)
                ServerCombinedPropModel.ServerPropList.First(prop => prop.KeyName == "server-portv6").StringValue = ipV6Box.Text;
            editSrvDialog.PopulateBoxes(ServerCombinedPropModel.ServerPropList);
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                ServerCombinedPropModel.ServerPropList = editSrvDialog.workingProps;
            }
        }

        private void serverSettingsBtn_Click(object sender, System.EventArgs e) {
            using PropEditorForm editSrvDialog = new();
            List<Property> filteredProps = new List<Property>();
            editSrvDialog.PopulateBoxes(ServerCombinedPropModel.ServicePropList.Where(x => !serviceConfigExcludeList.Contains(x.KeyName)).ToList());
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                editSrvDialog.workingProps.ForEach(x => {
                    ServerCombinedPropModel.ServicePropList.First(y => y.KeyName == x.KeyName).SetValue(x.StringValue);
                });
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
                ServerCombinedPropModel.ServerPropList.First(prop => prop.KeyName == "server-name").StringValue = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                ServerCombinedPropModel.ServerPropList.First(prop => prop.KeyName == "server-port").StringValue = ipV4Box.Text;
            if (ipV6Box.TextLength > 0)
                ServerCombinedPropModel.ServerPropList.First(prop => prop.KeyName == "server-portv6").StringValue = ipV6Box.Text;

            ServerCombinedPropModel.ServicePropList.First(prop => prop.KeyName == "DeployedVersion").StringValue = versionTextBox.Text;
            DialogResult = DialogResult.OK;
        }

        private void versionTextBox_TextChanged(object sender, System.EventArgs e) {
            if (versionTextBox.Text != FormManager.MainWindow.connectedHost.GetLatestBDSVersion()) {
                editPropsBtn.Enabled = false;
            } else {
                editPropsBtn.Enabled = true;
            }
            ServerCombinedPropModel.ServicePropList.First(prop => prop.KeyName == "SelectedServerVersion").StringValue = versionTextBox.Text;
        }
    }
}
