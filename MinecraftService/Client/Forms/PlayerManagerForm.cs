// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using Newtonsoft.Json;

namespace MinecraftService.Client.Forms {
    public partial class PlayerManagerForm : Form {
        private readonly IServerConfiguration _server;
        private List<Player> playersFound = new();
        private readonly List<Player> modifiedPlayers = new();
        private Player playerToEdit;
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
            if (server == null) throw new ArgumentNullException(nameof(server));
            _server = server;
            playersFound = _server.GetPlayerList() ?? new List<Player>();
            gridView.Rows.Clear();

            // Attach DataError handler
            gridView.DataError += (s, e) => { e.ThrowException = false; };

            for (int i = 5; i < 8; i++) {
                gridView.Columns[i].ReadOnly = true;
                gridView.Columns[i].CellTemplate.Style.BackColor = System.Drawing.Color.FromArgb(225, 225, 225);
            }
            gridView.AllowUserToAddRows = false;
            RefreshGridContents();
            entryTextToolTip.InitialDelay = 0;
            entryTextToolTip.ReshowDelay = 0;
            searchEntryBox.Click += SearchEntryBox_Click;
            _loaded = true;
        }

        private void SearchEntryBox_Click(object sender, EventArgs e) {
            entryTextToolTip.Show(_searchLegendText, searchEntryBox, 30000);
        }

        private string TranslatePermLevel(string permLevel) {
            switch (permLevel) {
                case "visitor":
                    return "2";
                case "member":
                    return "3";
                case "operator":
                    return "4";
                case "2":
                    return "visitor";
                case "3":
                    return "member";
                case "4":
                    return "operator";
                default:
                    return "";
            }
        }

        private void RefreshGridContents() {
            gridView.Rows.Clear();
            foreach (Player ply in playersFound) {
                Player player = ply;
                if (modifiedPlayers.Contains(player)) {
                    player = modifiedPlayers[modifiedPlayers.IndexOf(player)];
                }
                var playerTimes = player.GetTimes();
                string playerPermission = player.GetPermissionLevel();
                if (FormManager.MainWindow.SelectedServer.GetServerArch() == SharedStringBase.MinecraftServerArch.Java) {
                    playerPermission = TranslatePermLevel(player.GetPermissionLevel());
                }
                TimeSpan timeSpent = TimeSpan.FromTicks(playerTimes.Disconn - playerTimes.Conn);
                DateTime firstConnDateTime = new(playerTimes.First);
                DateTime connectDateTime = new(playerTimes.Conn);
                string timeString = timeSpent.TotalSeconds > 59.5 ? $"{timeSpent.TotalMinutes.ToString("N2")} Minutes" : $"{timeSpent.TotalSeconds.ToString("N2")} Seconds";
                if (playerTimes.Conn == playerTimes.Disconn) {
                    timeString = "N/A";
                }
                if (playerTimes.Conn > playerTimes.Disconn) {
                    timeString = "Active now";
                }
                string[] list = [player.GetPlayerID(), player.GetUsername(), playerPermission, player.IsPlayerWhitelisted().ToString(), player.PlayerIgnoresLimit().ToString(), firstConnDateTime.ToString("G"), connectDateTime.ToString("G"), timeString];
                gridView.Rows.Add(list);
            }
            RefreshRowColors();
            gridView.Refresh();
        }

        private void RefreshRowColors() {
            foreach (DataGridViewRow row in gridView.Rows) {
                Player player = _server.GetOrCreatePlayer((string)row.Cells[0].Value);
                if (player.GetPermissionLevel() == "4" || player.GetPermissionLevel() == "operator") {
                    row.Cells[0].Style.BackColor = Color.IndianRed;
                    row.Cells[1].Style.BackColor = Color.IndianRed;
                }
                if (player.GetPermissionLevel() == "3" || player.GetPermissionLevel() == "member") {
                    row.Cells[0].Style.BackColor = Color.CadetBlue;
                    row.Cells[1].Style.BackColor = Color.CadetBlue;
                }
                if (player.IsDefaultRegistration()) {
                    row.Cells[0].Style.BackColor = Color.Empty;
                    row.Cells[1].Style.BackColor = Color.Empty;
                }
                if (modifiedPlayers.Contains(player)) {
                    row.Cells[0].Style.BackColor = Color.LightGoldenrodYellow;
                    row.Cells[1].Style.BackColor = Color.LightGoldenrodYellow;
                }
            }
        }
        private void saveBtn_Click(object sender, EventArgs e) {
            gridView.ClearSelection();
            if (modifiedPlayers.Count > 0) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(modifiedPlayers, Formatting.Indented, settings));
                FormManager.TCPClient.SendData(new() {
                    Data = sendBytes,
                    ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.SelectedServer),
                    Type = MessageTypes.PlayersUpdate
                });
                FormManager.MainWindow.DisableUI("Service is registering new player data...");
                DialogResult = DialogResult.OK;
            }
        }

        private void searchEntryBox_TextChanged(object sender, EventArgs e) {
            playersFound = _server.GetPlayerList();
            string curText = searchEntryBox.Text.ToLower();
            List<Player> tempList = [];
            string[] splitCommands = [curText];
            string cmd;
            string value;

            if (curText.Contains(':')) {
                if (curText.Contains(',')) {
                    splitCommands = curText.Split(',', StringSplitOptions.TrimEntries);
                }
                if (splitCommands.Length > 0) {
                    foreach (string s in splitCommands) {
                        if (s.Contains(':')) {
                            string[] finalSplit = s.Split(':', StringSplitOptions.TrimEntries);
                            cmd = finalSplit[0].ToLower();
                            value = finalSplit[1].ToLower();
                            tempList = [];
                            foreach (Player player in playersFound) {
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
            using (NewPlayerRegistrationForm form = new()) {
                if (form.ShowDialog() == DialogResult.OK) {
                    _server.GetPlayerList().Add(form.PlayerToAdd);
                    modifiedPlayers.Add(form.PlayerToAdd);
                    RefreshGridContents();
                }
            }
        }

        private void gridView_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            RefreshRowColors();
        }

        private void gridView_CellStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            RefreshRowColors();
        }

        private void gridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if (_loaded) {
                DataGridViewRow focusedRow = gridView.Rows[e.RowIndex];
                playerToEdit = _server.GetOrCreatePlayer((string)focusedRow.Cells[0].Value);
                var playerTimes = playerToEdit.GetTimes();
                string playerWhitelist = playerToEdit.IsPlayerWhitelisted().ToString();
                string playerPermission = playerToEdit.GetPermissionLevel();
                if (FormManager.MainWindow.SelectedServer.GetServerArch() == SharedStringBase.MinecraftServerArch.Java) {
                    playerPermission = TranslatePermLevel(playerToEdit.GetPermissionLevel());
                }
                string playerIgnoreLimit = playerToEdit.PlayerIgnoresLimit().ToString();
                if ((string)focusedRow.Cells[0].Value != playerToEdit.GetPlayerID() || (string)focusedRow.Cells[1].Value != playerToEdit.GetUsername() || (string)focusedRow.Cells[2].Value != playerPermission || (string)focusedRow.Cells[3].Value != playerWhitelist || (string)focusedRow.Cells[4].Value != playerIgnoreLimit) {
                    playerToEdit = new Player((string)focusedRow.Cells[0].Value, (string)focusedRow.Cells[1].Value, playerTimes.First, playerTimes.Conn, playerTimes.Disconn, bool.Parse((string)focusedRow.Cells[3].Value), (string)focusedRow.Cells[2].Value, bool.Parse((string)focusedRow.Cells[4].Value));
                    if (modifiedPlayers.Contains(playerToEdit)) {
                        modifiedPlayers[modifiedPlayers.IndexOf(playerToEdit)] = playerToEdit;
                    } else {
                        modifiedPlayers.Add(playerToEdit);
                    }
                }
                RefreshRowColors();
            }
        }
    }
}
