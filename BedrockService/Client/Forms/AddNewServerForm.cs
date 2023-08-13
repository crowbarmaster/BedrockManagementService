using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Classes.Configurations;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Client.Forms {
    public partial class AddNewServerForm : Form {
        public Dictionary<MinecraftServerArch, ServerCombinedPropModel> LoadedConfigs = new Dictionary<MinecraftServerArch, ServerCombinedPropModel>();
        public ServerCombinedPropModel SelectedPropModel = new();
        private readonly List<IServerConfiguration> _serverConfigurations;
        private readonly IClientSideServiceConfiguration _serviceConfiguration;
        private readonly List<string> serviceConfigExcludeList = new()
        {
            ServerPropertyStrings[ServerPropertyKeys.MinecraftType],
            ServerPropertyStrings[ServerPropertyKeys.ServerName],
            ServerPropertyStrings[ServerPropertyKeys.ServerExeName],
            ServerPropertyStrings[ServerPropertyKeys.FileName],
            ServerPropertyStrings[ServerPropertyKeys.ServerPath],
            ServerPropertyStrings[ServerPropertyKeys.ServerVersion]
        };

        public AddNewServerForm(IClientSideServiceConfiguration serviceConfiguration, List<IServerConfiguration> serverConfigurations) {
            this._serviceConfiguration = serviceConfiguration;
            this._serverConfigurations = serverConfigurations;
            ServerCombinedPropModel serverCombinedPropModel = new();
            InitializeComponent();
            IServerConfiguration server;
            EnumTypeLookup typeLookup = new EnumTypeLookup(FormManager.Logger, FormManager.MainWindow.connectedHost);
            foreach (KeyValuePair<MinecraftServerArch, string> kvp in MinecraftArchStrings) {
                server = typeLookup.PrepareNewServerByArchName(kvp.Value, FormManager.processInfo, FormManager.Logger, FormManager.MainWindow.connectedHost);
                server.InitializeDefaults();
                serverCombinedPropModel.ServerPropList = FormManager.MainWindow.connectedHost.GetServerDefaultPropList(kvp.Key);
                serverCombinedPropModel.ServicePropList = server.GetSettingsList();
                LoadedConfigs.Add(server.GetServerArch(), serverCombinedPropModel);
                serverCombinedPropModel.VersionList = server.GetUpdater().GetVersionList();
            }
            VersionSelectComboBox.Items.Clear();
            VersionSelectComboBox.Items.AddRange(LoadedConfigs[MinecraftServerArch.Bedrock].VersionList.ToArray());
            ServerTypeComboBox.Items.Clear();
            ServerTypeComboBox.Items.AddRange(MinecraftArchStrings.Values.ToArray());
            ServerTypeComboBox.SelectedIndex = 0;
            VersionSelectComboBox.SelectedIndex = 0;
        }

        private void editPropsBtn_Click(object sender, System.EventArgs e) {
            using PropEditorForm editSrvDialog = new();
            if (srvNameBox.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.ServerName]).StringValue = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.PortI4]).StringValue = ipV4Box.Text;
            if (ipV6Box.Enabled && ipV6Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.PortI6]).StringValue = ipV6Box.Text;
            editSrvDialog.PopulateBoxes(SelectedPropModel.ServerPropList);
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                SelectedPropModel.ServerPropList = editSrvDialog.workingProps;
            }
        }

        private void serverSettingsBtn_Click(object sender, System.EventArgs e) {
            using PropEditorForm editSrvDialog = new();
            List<Property> filteredProps = new();
            editSrvDialog.PopulateBoxes(SelectedPropModel.ServicePropList.Where(x => !serviceConfigExcludeList.Contains(x.KeyName)).ToList());
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                editSrvDialog.workingProps.ForEach(x => {
                    SelectedPropModel.ServicePropList.First(y => y.KeyName == x.KeyName).SetValue(x.StringValue);
                });
            }
        }

        private void saveBtn_Click(object sender, System.EventArgs e) {
            List<string> usedPorts = new() {
                _serviceConfiguration.GetPort()
            };
            foreach (IServerConfiguration serverConfiguration in _serverConfigurations) {
                usedPorts.Add(serverConfiguration.GetProp(BmsDependServerPropKeys.PortI4).ToString());
                usedPorts.Add(serverConfiguration.GetProp(BmsDependServerPropKeys.PortI6).ToString());
            }
            foreach (string port in usedPorts) {
                if (ipV4Box.Text == port || ipV6Box.Text == port) {
                    MessageBox.Show($"You have selected port {port}, but this port is already in use. Please select another port!");
                    return;
                }
            }
            if (srvNameBox.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.ServerName]).StringValue = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.PortI4]).StringValue = ipV4Box.Text;
            if (ipV6Box.Enabled && ipV6Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == BmsDependServerPropStrings[BmsDependServerPropKeys.PortI6]).StringValue = ipV6Box.Text;

            SelectedPropModel.ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerVersion]).StringValue = VersionSelectComboBox.SelectedText;
            DialogResult = DialogResult.OK;
        }

        private void ServerTypeComboBox_SelectedIndexChanged(object sender, System.EventArgs e) {
            SelectedPropModel = LoadedConfigs[GetArchFromString(ServerTypeComboBox.SelectedText)];
            VersionSelectComboBox.SelectedIndex = -1;
            VersionSelectComboBox.Items.Clear();
            VersionSelectComboBox.Items.AddRange(SelectedPropModel.VersionList.ToArray());
            if(GetArchFromString(ServerTypeComboBox.SelectedText) == MinecraftServerArch.Java) {
                ipV6Box.Enabled = false;
            }
        }

        private void VersionSelectComboBox_SelectedIndexChanged(object sender, System.EventArgs e) {

        }
    }
}
