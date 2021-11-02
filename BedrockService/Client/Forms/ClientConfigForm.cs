using BedrockService.Client.Management;
using BedrockService.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class ClientConfigForm : Form
    {
        private readonly ConfigManager _configManager;
        public ClientConfigForm(List<IClientSideServiceConfiguration> configs, ConfigManager configManager)
        {
            InitializeComponent();
            foreach(IClientSideServiceConfiguration config in configs)
            {
                serverGridView.Rows.Add(new string[3] { config.GetHostName(), config.GetAddress(), config.GetPort() });
            }
            _configManager = configManager;
        }

        public void SimulateTests()
        {
            nbtButton.PerformClick();
        }

        private void nbtButton_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "EXE Files|.exe";
                fileDialog.FileName = "nbtStudio.exe";
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if(fileDialog.ShowDialog() == DialogResult.OK)
                {
                    _configManager.NBTStudioPath = fileDialog.FileName;
                    nbtPathLabel.Text = $"NBT Studio path: {_configManager.NBTStudioPath}";
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {

        }
    }
}
