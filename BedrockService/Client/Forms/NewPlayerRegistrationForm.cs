using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using System;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class NewPlayerRegistrationForm : Form {
        public IPlayer PlayerToAdd;
        public NewPlayerRegistrationForm() {
            InitializeComponent();
        }

        private void saveClick(object sender, EventArgs e) {
            long curTime = DateTime.Now.Ticks;
            if (usernameTextBox.TextLength > 0 && xuidTextBox.TextLength == 16) {
                PlayerToAdd = new BedrockPlayer(xuidTextBox.Text, usernameTextBox.Text, curTime, curTime, curTime, whitelistedChkBox.Checked, permissionComboBox.SelectedItem.ToString(), ignoreLimitChkBox.Checked);
                    new Player(xuidTextBox.Text, usernameTextBox.Text, curTime, curTime, curTime, whitelistedChkBox.Checked,
                DialogResult = DialogResult.OK;
            }
        }
    }
}
