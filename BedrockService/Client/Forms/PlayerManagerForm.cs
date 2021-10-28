using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class PlayerManagerForm : Form
    {
        private readonly IServerConfiguration Server;
        private readonly string[] RegisteredPlayerColumnArray = new string[8] { "XUID:", "Username:", "Permission:", "Whitelisted:", "Ignores max players:", "First connected on:", "Last connected on:", "Time spent in game:" };
        private List<IPlayer> playersFound = new List<IPlayer>();
        private List<IPlayer> modifiedPlayers = new List<IPlayer>();
        private IPlayer playerToEdit;

        public PlayerManagerForm(IServerConfiguration server)
        {
            InitializeComponent();
            Server = server;
            playersFound = Server.GetPlayerList();

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
            foreach (IPlayer player in playersFound)
            {
                string[] playerReg = player.GetRegistration();
                string[] playerTimes = player.GetTimes();
                string playerFirstConnect = playerTimes[0];
                string playerConnectTime = playerTimes[1];
                string playerDisconnectTime = playerTimes[2];
                string playerWhitelist = playerReg[0];
                string playerPermission = playerReg[1];
                string playerIgnoreLimit = playerReg[2];
                TimeSpan timeSpent = TimeSpan.FromTicks(long.Parse(playerConnectTime) - long.Parse(playerDisconnectTime));
                string[] list = new string[] { player.GetXUID(), player.GetUsername(), playerPermission, playerWhitelist, playerIgnoreLimit, playerFirstConnect, playerConnectTime, timeSpent.ToString("hhmmss") };
                gridView.Rows.Add(list);
            }
            gridView.Refresh();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (modifiedPlayers.Count > 0)
            {
                foreach (IPlayer player in modifiedPlayers)
                {
                    Server.AddUpdatePlayer(player);
                }
            }
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            byte[] sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(modifiedPlayers, Formatting.Indented, settings));
            FormManager.GetTCPClient.SendData(sendBytes, NetworkMessageSource.Client, NetworkMessageDestination.Server, FormManager.GetMainWindow.connectedHost.GetServerIndex(Server), NetworkMessageTypes.PlayersUpdate);
            FormManager.GetMainWindow.DisableUI();
            Close();
            Dispose();
        }

        private void searchEntryBox_TextChanged(object sender, EventArgs e)
        {
            playersFound = Server.GetPlayerList();
            string curText = searchEntryBox.Text;
            List<IPlayer> tempList = new List<IPlayer>();
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
                            tempList = new List<IPlayer>();
                            foreach (IPlayer player in playersFound)
                            {
                                if (player.SearchForProperty(cmd).Contains(value))
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
                foreach (IPlayer player in playersFound)
                {
                    if (player.SearchForProperty(cmd).Contains(value))
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
            playerToEdit = Server.GetPlayerByXuid((string)focusedRow.Cells[0].Value);
        }

        private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow focusedRow = gridView.Rows[e.RowIndex];
            string[] playerReg = playerToEdit.GetRegistration();
            string[] playerTimes = playerToEdit.GetTimes();
            string playerFirstConnect = playerTimes[0];
            string playerConnectTime = playerTimes[1];
            string playerDisconnectTime = playerTimes[2];
            string playerWhitelist = playerReg[0];
            string playerPermission = playerReg[1];
            string playerIgnoreLimit = playerReg[2];
            if ((string)focusedRow.Cells[0].Value != playerToEdit.GetXUID() || (string)focusedRow.Cells[1].Value != playerToEdit.GetUsername() || (string)focusedRow.Cells[2].Value != playerPermission || (string)focusedRow.Cells[3].Value != playerWhitelist || (string)focusedRow.Cells[4].Value != playerIgnoreLimit)
            {
                playerToEdit = new Player((string)focusedRow.Cells[0].Value, (string)focusedRow.Cells[1].Value, playerFirstConnect, playerConnectTime, playerDisconnectTime, bool.Parse((string)focusedRow.Cells[3].Value), (string)focusedRow.Cells[2].Value, bool.Parse((string)focusedRow.Cells[4].Value), true);
                modifiedPlayers.Add(playerToEdit);
            }
        }

        private void registerPlayerBtn_Click(object sender, EventArgs e)
        {
            using (NewPlayerRegistrationForm form = new NewPlayerRegistrationForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    Server.GetPlayerList().Add(form.PlayerToAdd);
                    modifiedPlayers.Add(form.PlayerToAdd);
                    RefreshGridContents();
                }
            }
        }
    }
}
