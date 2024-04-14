// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using static MinecraftService.Shared.Classes.SharedStringBase;

namespace MinecraftService.Client.Forms {
    public partial class AddNewServerForm : Form {
        public Dictionary<MinecraftServerArch, ServerCombinedPropModel> LoadedConfigs = new Dictionary<MinecraftServerArch, ServerCombinedPropModel>();
        public ServerCombinedPropModel SelectedPropModel = new();
        private Dictionary<MinecraftServerArch, SimpleVersionModel[]> VersionLists;
        private Dictionary<MinecraftServerArch, SimpleVersionModel> _latestVersionLookup = new();
        private bool _customVersionSelected = false;
        private bool _typeChanging = false;
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

        public AddNewServerForm(IClientSideServiceConfiguration serviceConfiguration, List<IServerConfiguration> serverConfigurations, Dictionary<SharedStringBase.MinecraftServerArch, SimpleVersionModel[]> verLists) {
            VersionLists = verLists;
            _serviceConfiguration = serviceConfiguration;
            _serverConfigurations = serverConfigurations;
            InitializeComponent();
            IServerConfiguration server;
            EnumTypeLookup typeLookup = new EnumTypeLookup(FormManager.Logger, FormManager.MainWindow.connectedHost);
            foreach (KeyValuePair<MinecraftServerArch, string> kvp in MinecraftArchStrings) {
                ServerCombinedPropModel serverCombinedPropModel = new();
                server = typeLookup.PrepareNewServerByArch(kvp.Value, FormManager.processInfo, FormManager.Logger, FormManager.MainWindow.connectedHost);
                server.InitializeDefaults();
                serverCombinedPropModel.ServerPropList = server.GetAllProps();
                serverCombinedPropModel.ServicePropList = server.GetSettingsList();
                serverCombinedPropModel.VersionList = new List<SimpleVersionModel>(VersionLists[kvp.Key]);
                _latestVersionLookup.Add(kvp.Key, serverCombinedPropModel.VersionList[0]);
                LoadedConfigs.Add(server.GetServerArch(), serverCombinedPropModel);
            }
            VersionSelectComboBox.Items.Clear();
            ServerTypeComboBox.Items.Clear();
            ServerTypeComboBox.Items.AddRange(MinecraftArchStrings.Values.ToArray());
            ServerTypeComboBox.SelectedIndex = 0;
            VersionSelectComboBox.SelectedIndex = 0;
        }

        private void editPropsBtn_Click(object sender, System.EventArgs e) {
            using PropEditorForm editSrvDialog = new();
            if (srvNameBox.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.ServerName]).StringValue = srvNameBox.Text;
            if (ipV4Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.PortI4]).StringValue = ipV4Box.Text;
            if (ipV6Box.Enabled && ipV6Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.PortI6]).StringValue = ipV6Box.Text;
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
                usedPorts.Add(serverConfiguration.GetProp(MmsDependServerPropKeys.PortI4).ToString());
                if (serverConfiguration.GetServerArch() != MinecraftServerArch.Java) {
                    usedPorts.Add(serverConfiguration.GetProp(MmsDependServerPropKeys.PortI6).ToString());
                }
            }
            foreach (string port in usedPorts) {
                if (ipV4Box.Text == port || ipV6Box.Text == port) {
                    MessageBox.Show($"You have selected port {port}, but this port is already in use. Please select another port!");
                    return;
                }
            }
            if (srvNameBox.TextLength > 0) {
                if (SelectedPropModel.ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[ServerPropertyKeys.MinecraftType]).StringValue == "Java") {
                    SelectedPropModel.ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerName]).StringValue = srvNameBox.Text;
                } else {
                    SelectedPropModel.ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.ServerName]).StringValue = srvNameBox.Text;
                }
            }
            if (ipV4Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.PortI4]).StringValue = ipV4Box.Text;
            if (ipV6Box.Enabled && ipV6Box.TextLength > 0)
                SelectedPropModel.ServerPropList.First(prop => prop.KeyName == MmsDependServerPropStrings[MmsDependServerPropKeys.PortI6]).StringValue = ipV6Box.Text;

            SelectedPropModel.ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[ServerPropertyKeys.ServerVersion]).StringValue = ((SimpleVersionModel)VersionSelectComboBox.SelectedItem).Version;
            if (_customVersionSelected) {
                SelectedPropModel.ServicePropList.First(prop => prop.KeyName == ServerPropertyStrings[ServerPropertyKeys.AutoDeployUpdates]).StringValue = "False";
            }
            DialogResult = DialogResult.OK;
        }

        private void ServerTypeComboBox_SelectedIndexChanged(object sender, System.EventArgs e) {
            _typeChanging = true;
            SelectedPropModel = LoadedConfigs[GetArchFromString((string)ServerTypeComboBox.SelectedItem)];
            VersionSelectComboBox.SelectedIndex = -1;
            VersionSelectComboBox.Items.Clear();
            VersionSelectComboBox.Items.AddRange(SelectedPropModel.VersionList.Where(x => x.IsBeta == BetaVersionCheckBox.Checked).ToArray());
            ipV6Box.Enabled = GetArchFromString((string)ServerTypeComboBox.SelectedItem) != MinecraftServerArch.Java;
            if (VersionSelectComboBox.Items.Count > 0) {
                VersionSelectComboBox.SelectedIndex = 0;
            }
            _typeChanging = false;
        }

        private void VersionSelectComboBox_SelectedIndexChanged(object sender, System.EventArgs e) {
            if (!_typeChanging && VersionSelectComboBox.Items.Count > 0 && _latestVersionLookup.GetValueOrDefault(GetArchFromString((string)ServerTypeComboBox.SelectedItem)).Version != ((SimpleVersionModel)((ComboBox)sender).SelectedItem).Version) {
                _customVersionSelected = true;
            }
        }

        private void BetaVersionCheckBox_CheckedChanged(object sender, System.EventArgs e) {
            VersionSelectComboBox.SelectedIndex = -1;
            VersionSelectComboBox.Items.Clear();
            VersionSelectComboBox.Items.AddRange(SelectedPropModel.VersionList.Where(x => x.IsBeta == BetaVersionCheckBox.Checked).ToArray());
            if (VersionSelectComboBox.Items.Count > 0) {
                VersionSelectComboBox.SelectedIndex = 0;
            }
        }
    }
}
