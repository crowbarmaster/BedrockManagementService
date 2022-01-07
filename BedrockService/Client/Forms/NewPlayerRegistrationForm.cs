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
            if (usernameTextBox.TextLength > 0 && xuidTextBox.TextLength == 16) {
                PlayerToAdd = new Player(xuidTextBox.Text, usernameTextBox.Text, DateTime.Now.Ticks, 0, 0, whitelistedChkBox.Checked, permissionComboBox.SelectedItem.ToString(), ignoreLimitChkBox.Checked);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
