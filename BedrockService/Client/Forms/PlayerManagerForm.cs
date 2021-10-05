using BedrockService.Client.Management;
using BedrockService.Service.Server.HostInfoClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class PlayerManagerForm : Form
    {
        private readonly ServerInfo Server;
        private readonly string[] RegisteredPlayerColumnArray = new string[8] { "XUID:", "Username:", "Permission:", "Whitelisted:", "Ignores max players:", "First connected on:", "Last connected on:", "Time spent in game:" };
        private List<Player> playersFound = new List<Player>();
        private List<Player> modifiedPlayers = new List<Player>();
        private Player playerToEdit;

        public PlayerManagerForm(ServerInfo server)
        {
            InitializeComponent();
            Server = server;
            playersFound = Server.KnownPlayers;

            gridView.Columns.Clear();
            gridView.Rows.Clear();
            foreach (string s in RegisteredPlayerColumnArray)
            {
                gridView.Columns.Add(s.Replace(" ", "").Replace(":", ""), s);
            }
            gridView.Columns[5].ReadOnly = true;
            gridView.Columns[6].ReadOnly = true;
            gridView.Columns[7].ReadOnly = true;
            gridView.Columns[5].CellTemplate.Style.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
            gridView.Columns[6].CellTemplate.Style.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
            gridView.Columns[7].CellTemplate.Style.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
            gridView.AllowUserToAddRows = false;
            RefreshGridContents();
        }

        private void RefreshGridContents()
        {
            gridView.Rows.Clear();
            foreach (Player player in playersFound)
            {
                TimeSpan timeSpent = TimeSpan.FromTicks(long.Parse(player.LastDisconnectTime) - long.Parse(player.LastConnectedTime));
                string[] list = new string[] { player.XUID, player.Username, player.PermissionLevel, player.Whitelisted.ToString(), player.IgnorePlayerLimits.ToString(), player.FirstConnectedTime, player.LastConnectedTime, timeSpent.ToString("hhmmss") };
                gridView.Rows.Add(list);
            }
            gridView.Refresh();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (modifiedPlayers.Count > 0)
            {
                foreach (Player player in modifiedPlayers)
                {
                    Player replacedPlayer = Server.KnownPlayers.First(x => x.XUID == player.XUID);
                    Server.KnownPlayers.Remove(replacedPlayer);
                    Server.KnownPlayers.Add(player);
                }
            }
            byte[] sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(modifiedPlayers));
            FormManager.GetTCPClient.SendData(sendBytes, Service.Networking.NetworkMessageSource.Client, Service.Networking.NetworkMessageDestination.Server, (byte)FormManager.GetMainWindow.connectedHost.Servers.IndexOf(Server), Service.Networking.NetworkMessageTypes.PlayersUpdate);
            FormManager.GetMainWindow.WaitForCallbackInvoked();
            Close();
            Dispose();
        }

        private void searchEntryBox_TextChanged(object sender, EventArgs e)
        {
            playersFound = Server.KnownPlayers;
            string curText = searchEntryBox.Text;
            List<Player> tempList = new List<Player>();
            string[] splitCommands;
            string cmd;
            string value;

            if (curText.Contains(":"))
            {
                splitCommands = curText.Split(',');
                if (splitCommands.Length > 1)
                {
                    foreach (string s in splitCommands)
                    {
                        if (s.Contains(":"))
                        {
                            string[] finalSplit = s.Split(':');
                            cmd = finalSplit[0];
                            value = finalSplit[1];
                            tempList = new List<Player>();
                            foreach (Player player in playersFound)
                            {
                                if (player.CommandStringTranslator(cmd).Contains(value))
                                {
                                    tempList.Add(player);
                                }
                            }
                            playersFound = tempList;
                            gridView.Refresh();
                        }
                    }
                }
                splitCommands = curText.Split(':');
                cmd = splitCommands[0];
                value = splitCommands[1];
                foreach (Player player in playersFound)
                {
                    if (player.CommandStringTranslator(cmd).Contains(value))
                    {
                        tempList.Add(player);
                    }
                }
                playersFound = tempList;
                gridView.Refresh();
                RefreshGridContents();
            }
        }

        private void gridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridViewRow focusedRow = gridView.Rows[e.RowIndex];
            playerToEdit = Server.KnownPlayers.First(p => p.XUID == (string)focusedRow.Cells[0].Value);
        }

        private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow focusedRow = gridView.Rows[e.RowIndex];
            if ((string)focusedRow.Cells[0].Value != playerToEdit.XUID || (string)focusedRow.Cells[1].Value != playerToEdit.Username || (string)focusedRow.Cells[2].Value != playerToEdit.PermissionLevel || (string)focusedRow.Cells[3].Value != playerToEdit.Whitelisted.ToString() || (string)focusedRow.Cells[4].Value != playerToEdit.IgnorePlayerLimits.ToString())
            {
                playerToEdit.XUID = (string)focusedRow.Cells[0].Value;
                playerToEdit.Username = (string)focusedRow.Cells[1].Value;
                playerToEdit.PermissionLevel = (string)focusedRow.Cells[2].Value;
                playerToEdit.Whitelisted = bool.Parse((string)focusedRow.Cells[3].Value);
                playerToEdit.IgnorePlayerLimits = bool.Parse((string)focusedRow.Cells[4].Value);
                playerToEdit.FromConfig = true;
                modifiedPlayers.Add(playerToEdit);
            }
        }

        private void registerPlayerBtn_Click(object sender, EventArgs e)
        {
            using (NewPlayerRegistrationForm form = new NewPlayerRegistrationForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Server.KnownPlayers.Add(form.AddPlayer);
                    modifiedPlayers.Add(form.AddPlayer);
                    RefreshGridContents();
                }
            }
        }
    }
}
