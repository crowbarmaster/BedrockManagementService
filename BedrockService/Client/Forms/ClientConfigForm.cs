using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class ClientConfigForm : Form
    {
        private readonly List<IClientSideServiceConfiguration> _clientConfigs;
        private readonly ConfigManager _configManager;
        public ClientConfigForm(ConfigManager configManager)
        {
            InitializeComponent();
            _configManager = configManager;
            _clientConfigs = _configManager.HostConnectList;
            if (!string.IsNullOrEmpty(_configManager.NBTStudioPath))
            {
                nbtPathLabel.Text = $"NBT Studio path: {_configManager.NBTStudioPath}";
            }
            foreach (IClientSideServiceConfiguration config in _clientConfigs)
            {
                serverGridView.Rows.Add(new string[3] { config.GetHostName(), config.GetAddress(), config.GetPort() });
            }
        }

        public void SimulateTests()
        {
            nbtButton.PerformClick();
        }

        private void nbtButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "EXE Files|*.exe";
                fileDialog.FileName = "NbtStudio.exe";
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    _configManager.NBTStudioPath = fileDialog.FileName;
                    nbtPathLabel.Text = $"NBT Studio path: {_configManager.NBTStudioPath}";
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            List<IClientSideServiceConfiguration> newConfigs = new List<IClientSideServiceConfiguration>();
            foreach (DataGridViewRow row in serverGridView.Rows)
            {
                if (!string.IsNullOrEmpty((string)row.Cells[0].Value))
                {
                    newConfigs.Add(new ClientSideServiceConfiguration((string)row.Cells[0].Value, (string)row.Cells[1].Value, (string)row.Cells[2].Value));
                }
            }
            _configManager.HostConnectList = newConfigs;

            _configManager.SaveConfigFile();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
