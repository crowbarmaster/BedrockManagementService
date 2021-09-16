using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class PlayerManagerForm : Form
    {
        private readonly ServerInfo Server;
        private readonly string[] RegisteredPlayerColumnArray = new string[] { "XUID:", "Username:", "Permission:", "Whitelisted:", "Ignores max players:", "First connected on:", "Last connected on:", "Time spent in game:" };
        private readonly string[] KnownPlayerColumnArray = new string[] { "XUID:", "Username:", "Registered status:", "First connected on:", "Last connected on:", "Time spent in game:" };

        public PlayerManagerForm(ServerInfo server)
        {
            InitializeComponent();
            Server = server;
            tabPage1.Text = "Registered players";
            tabPage2.Text = "Known players";
            tabControl.SelectedTab = tabPage1;

            tabPage1.Enter += TabPage1_Enter;
            tabPage2.Enter += TabPage2_Enter;
        }

        private void TabPage1_Enter(object sender, EventArgs e)
        {
            gridView.Columns.Clear();
            gridView.Rows.Clear();
            foreach (string s in RegisteredPlayerColumnArray)
            {
                gridView.Columns.Add(s.Replace(" ", "").Replace(":", ""), s);
            }
            foreach (Player player in Server.KnownPlayers)
            {
                TimeSpan timeSpent = TimeSpan.FromTicks(long.Parse(player.LastDisconnectTime) - long.Parse(player.LastConnectedTime));
                string[] list = new string[] { player.XUID, player.Username, player.PermissionLevel, player.Whitelisted.ToString(), player.IgnorePlayerLimits.ToString(), player.FirstConnectedTime, player.LastConnectedTime, timeSpent.ToString("hhmmss") };
                if (player.FromConfig)
                    gridView.Rows.Add(list);
            }
            gridView.Refresh();
        }

        private void TabPage2_Enter(object sender, EventArgs e)
        {
            gridViewKnown.Columns.Clear();
            gridViewKnown.Rows.Clear();
            foreach (string s in KnownPlayerColumnArray)
            {
                gridViewKnown.Columns.Add(s.Replace(" ", "").Replace(":", ""), s);
            }
            foreach (Player player in Server.KnownPlayers)
            {
                TimeSpan timeSpent = TimeSpan.FromTicks(long.Parse(player.LastDisconnectTime) - long.Parse(player.LastConnectedTime));
                string[] list = new string[] { player.XUID, player.Username, player.FromConfig.ToString(), player.FirstConnectedTime, player.LastConnectedTime, timeSpent.ToString("hhmmss") };
                gridViewKnown.Rows.Add(list);
            }
            gridViewKnown.Refresh();
        }
    }
}
