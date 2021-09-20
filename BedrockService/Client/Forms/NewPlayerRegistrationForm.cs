using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class NewPlayerRegistrationForm : Form
    {
        public Player AddPlayer;
        public NewPlayerRegistrationForm()
        {
            InitializeComponent();
        }

        private void saveClick(object sender, EventArgs e)
        {
            if(usernameTextBox.TextLength > 0 && xuidTextBox.TextLength == 16)
            {
                Player player = new Player(xuidTextBox.Text, usernameTextBox.Text, "");
                player.PermissionLevel = (string)permissionComboBox.SelectedItem;
                player.Whitelisted = whitelistedChkBox.Checked;
                player.IgnorePlayerLimits = ignoreLimitChkBox.Checked;
                player.FirstConnectedTime = DateTime.Now.Ticks.ToString();
                player.LastConnectedTime = player.FirstConnectedTime;
                player.LastDisconnectTime = player.FirstConnectedTime;
                AddPlayer = player;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
