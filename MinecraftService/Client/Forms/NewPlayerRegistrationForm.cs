using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes;
using MinecraftService.Shared.Interfaces;
using System;
using System.Windows.Forms;

namespace MinecraftService.Client.Forms {
    public partial class NewPlayerRegistrationForm : Form {
        public IPlayer PlayerToAdd;
        private readonly IServerConfiguration _serverConfiguration;

        public NewPlayerRegistrationForm() {
            InitializeComponent();
            _serverConfiguration = FormManager.MainWindow.selectedServer;
        }

        private void saveClick(object sender, EventArgs e) {
            long curTime = DateTime.Now.Ticks;
            if (usernameTextBox.TextLength > 0 && xuidTextBox.TextLength == 16 || xuidTextBox.TextLength == 36 || xuidTextBox.TextLength == 32) {
                PlayerToAdd = 
                    new Player(xuidTextBox.Text, usernameTextBox.Text, curTime, curTime, curTime, whitelistedChkBox.Checked,
                    _serverConfiguration.GetServerArch() == SharedStringBase.MinecraftServerArch.Java ?
                    GetJavaPermLevel(permissionComboBox.SelectedItem.ToString()) :
                    permissionComboBox.SelectedItem.ToString(),
                    ignoreLimitChkBox.Checked);
                DialogResult = DialogResult.OK;
            }
        }

        private string GetJavaPermLevel(string permLevel) {
            if(permLevel == "visitor") {
                return "2";
            }
            if(permLevel == "member") {
                return "3";
            }
            if(permLevel == "op") {
                return "4";
            }
            return "1";
        }
    }
}
