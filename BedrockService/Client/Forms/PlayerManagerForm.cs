using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class PlayerManagerForm : Form {
        private readonly IServerConfiguration _server;
        private readonly string[] RegisteredPlayerColumnArray = new string[8] { "XUID:", "Username:", "Permission:", "Whitelisted:", "Ignores max players:", "First connected on:", "Last connected on:", "Time spent in game:" };
        private List<IPlayer> playersFound = new List<IPlayer>();
        private readonly List<IPlayer> modifiedPlayers = new List<IPlayer>();
        private IPlayer playerToEdit;
        private bool _loaded = false;
        private const string _searchLegendText =
            "Search Legend:\n" +
            "Syntax: \"function:search text\"\n" +
            "Use commas for extra filters. Case insensitive.\n" +
            "Useable filter patterns:\n" +
            "By username: \"username\", \"name\", or \"un\"\n" +
            "By perm level: \"permission\", \"perm\", or \"pl\"\n" +
            "By Whitelist state: \"whitelist\", \"white\", \"wl\"\n" +
            "By ignores max player limits: \"ignoreslimit\" or \"il\"\n" +
            "Example: To find Crowbarmast3r with level operator:\n" +
            "un:crowbarmas3r, pl:operator";


        public PlayerManagerForm(IServerConfiguration server) {
            InitializeComponent();
            _server = server;
            playersFound = _server.GetPlayerList();
            gridView.Rows.Clear();
            for (int i = 5; i < 8; i++) {
                gridView.Columns[i].ReadOnly = true;
                gridView.Columns[i].CellTemplate.Style.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
            }
            gridView.AllowUserToAddRows = false;
            RefreshGridContents();
            entryTextToolTip.InitialDelay = 0;
            entryTextToolTip.ReshowDelay = 0;
            searchEntryBox.Click += SearchEntryBox_Click;
            gridView.CellStateChanged += GridView_CellStateChanged;
            _loaded = true;
        }

        private void GridView_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            if (_loaded) {
                DataGridViewRow focusedRow = e.Cell.OwningRow;
                playerToEdit = _server.GetOrCreatePlayer((string)focusedRow.Cells[0].Value);
                var playerTimes = playerToEdit.GetTimes();
                string playerWhitelist = playerToEdit.IsPlayerWhitelisted().ToString();
                string playerPermission = playerToEdit.GetPermissionLevel();
                string playerIgnoreLimit = playerToEdit.PlayerIgnoresLimit().ToString();
                if ((string)focusedRow.Cells[0].Value != playerToEdit.GetXUID() || (string)focusedRow.Cells[1].Value != playerToEdit.GetUsername() || (string)focusedRow.Cells[2].Value != playerPermission || (string)focusedRow.Cells[3].Value != playerWhitelist || (string)focusedRow.Cells[4].Value != playerIgnoreLimit) {
                    playerToEdit = new Player((string)focusedRow.Cells[0].Value, (string)focusedRow.Cells[1].Value, playerTimes.First, playerTimes.Conn, playerTimes.Disconn, bool.Parse((string)focusedRow.Cells[3].Value), (string)focusedRow.Cells[2].Value, bool.Parse((string)focusedRow.Cells[4].Value));
                    if (modifiedPlayers.Contains(playerToEdit)) {
                        modifiedPlayers[modifiedPlayers.IndexOf(playerToEdit)] = playerToEdit;
                    } else {
                        modifiedPlayers.Add(playerToEdit);
                    }
                }
            }
        }

        private void SearchEntryBox_Click(object sender, EventArgs e) {

            entryTextToolTip.Show(_searchLegendText, searchEntryBox, 30000);
        }

        private void RefreshGridContents() {
            gridView.Rows.Clear();
            foreach (IPlayer ply in playersFound) {
                IPlayer player = ply;
                if (modifiedPlayers.Contains(player)) {
                    player = modifiedPlayers[modifiedPlayers.IndexOf(player)];
                }
                var playerTimes = player.GetTimes();
                string playerPermission = player.GetPermissionLevel();
                TimeSpan timeSpent = TimeSpan.FromTicks(playerTimes.Disconn - playerTimes.Conn);
                DateTime firstConnDateTime = new DateTime(playerTimes.First);
                DateTime connectDateTime = new DateTime(playerTimes.Conn);
                string timeString = timeSpent.TotalSeconds > 59.5 ? $"{timeSpent.TotalMinutes.ToString("N2")} Minutes" : $"{timeSpent.TotalSeconds.ToString("N2")} Seconds";
                if (playerTimes.Conn == playerTimes.Disconn) {
                    timeString = "N/A";
                }
                if (playerTimes.Conn > playerTimes.Disconn) {
                    timeString = "Active now";
                }
                string[] list = new string[] { player.GetXUID(), player.GetUsername(), playerPermission, player.IsPlayerWhitelisted().ToString(), player.PlayerIgnoresLimit().ToString(), firstConnDateTime.ToString("G"), connectDateTime.ToString("G"), timeString };
                gridView.Rows.Add(list);
            }
            gridView.Refresh();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            gridView.ClearSelection();
            if (modifiedPlayers.Count > 0) {
                foreach (IPlayer player in modifiedPlayers) {
                    _server.AddUpdatePlayer(player);
                }
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(modifiedPlayers, Formatting.Indented, settings));
                FormManager.TCPClient.SendData(sendBytes, FormManager.MainWindow.connectedHost.GetServerIndex(_server), NetworkMessageTypes.PlayersUpdate);
                FormManager.MainWindow.DisableUI();
                DialogResult = DialogResult.OK;
            }
        }

        private void searchEntryBox_TextChanged(object sender, EventArgs e) {
            playersFound = _server.GetPlayerList();
            string curText = searchEntryBox.Text.ToLower();
            List<IPlayer> tempList = new List<IPlayer>();
            string[] splitCommands = new string[1] { curText };
            string cmd;
            string value;

            if (curText.Contains(":")) {
                if (curText.Contains(",")) {
                    splitCommands = curText.Split(',', StringSplitOptions.TrimEntries);
                }
                if (splitCommands.Length > 0) {
                    foreach (string s in splitCommands) {
                        if (s.Contains(":")) {
                            string[] finalSplit = s.Split(':', StringSplitOptions.TrimEntries);
                            cmd = finalSplit[0].ToLower();
                            value = finalSplit[1].ToLower();
                            tempList = new List<IPlayer>();
                            foreach (IPlayer player in playersFound) {
                                string key = player.SearchForProperty(cmd);
                                if (key != null && key.Contains(value)) {
                                    tempList.Add(player);
                                }
                            }
                            playersFound = tempList;
                            gridView.Refresh();
                        }
                    }
                }
                playersFound = tempList;
            } else {
                playersFound = _server.GetPlayerList();
            }
            gridView.Refresh();
            RefreshGridContents();
        }

        private void registerPlayerBtn_Click(object sender, EventArgs e) {
            gridView.ClearSelection();
            using (NewPlayerRegistrationForm form = new NewPlayerRegistrationForm()) {
                if (form.ShowDialog() == DialogResult.OK) {
                    _server.GetPlayerList().Add(form.PlayerToAdd);
                    modifiedPlayers.Add(form.PlayerToAdd);
                    RefreshGridContents();
                }
            }
        }
    }
}
