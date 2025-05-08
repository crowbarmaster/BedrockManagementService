// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using MinecraftService.Client.Management;
using MinecraftService.Shared.Classes.Networking;
using MinecraftService.Shared.Classes.Server;
using MinecraftService.Shared.Classes.Service.Configuration;
using MinecraftService.Shared.Classes.Service.Core;
using MinecraftService.Shared.Interfaces;
using MinecraftService.Shared.SerializeModels;
using MinecraftService.Shared.Utilities;
using Newtonsoft.Json;
using static MinecraftService.Shared.Classes.Service.Core.SharedStringBase;
using Message = MinecraftService.Shared.Classes.Networking.Message;

namespace MinecraftService.Client.Forms {
    public partial class MainWindow : Form {
        public ServiceConfigurator connectedHost;
        public IServerConfiguration SelectedServer;
        public ClientSideServiceConfiguration clientSideServiceConfiguration;
        public ServiceStatusModel ServiceStatus;
        private List<string> _localCommandHistory = new();
        public bool ServerBusy = false;
        private int _distanceFromLastCommand = 0;
        public MmsLogger ClientLogger;
        public ConfigManager ConfigManager;
        private int _connectTimeout;
        private bool _followTail = false;
        private bool _blockConnect = false;
        private bool _enableLogUpdating = false;
        private AddNewServerForm _newServerForm;
        private BackupManagerForm _backupManager;
        private const int _connectTimeoutLimit = 3;
        private readonly ProcessInfo _processInfo;
        private readonly System.Timers.Timer _connectTimer = new(100.0);
        private ProgressDialog _uiWaitDialog;
        private System.Timers.Timer _uiWaitTimer = new(250);
        private readonly LogManager _logManager;
        private List<Shared.PackParser.MinecraftPackContainer> _incomingPacks;
        private byte _manPacksServer;

        public MainWindow(ProcessInfo processInfo, MmsLogger logger) {
            _processInfo = processInfo;
            ClientLogger = logger;
            ClientLogger.Initialize();
            _logManager = new LogManager(ClientLogger);
            ConfigManager = new ConfigManager(ClientLogger);
            InitializeComponent();
            Text = $"Minecraft Management Service Client {Application.ProductVersion}";
            InitForm();
            _connectTimer.Elapsed += ConnectTimer_Elapsed;
            Shown += MainWindow_Shown;
            _uiWaitDialog = new(null);
        }

        private void MainWindow_Shown(object sender, EventArgs e) {
            _enableLogUpdating = true;
            _logManager.InitLogThread();
            _localCommandHistory = new(FileUtilities.ReadLines(GetServiceFilePath(MmsFileNameKeys.ClientCommandHistory)));
        }

        private void ConnectTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (_connectTimer.Enabled && !FormManager.TCPClient.EstablishedLink && !_blockConnect) {
                _connectTimer.Interval = 2000.0;
                _blockConnect = true;
                Invoke((MethodInvoker)delegate { FormManager.TCPClient.ConnectHost(ConfigManager.HostConnectList.FirstOrDefault(host => host.GetHostName() == (string)HostListBox.SelectedItem)); });
                Thread.Sleep(1000);
                if (connectedHost != null && FormManager.TCPClient.EstablishedLink) {
                    ServerBusy = false;
                    _connectTimer.Enabled = false;
                    _connectTimer.Stop();
                    _connectTimer.Close();
                    Invoke((MethodInvoker)delegate { RefreshAllCompenentStates(); });
                    RefreshServerBoxContents();
                    return;
                }
                _connectTimeout++;
                _blockConnect = false;
                if (_connectTimeout >= _connectTimeoutLimit) {
                    _connectTimer.Enabled = false;
                    _connectTimer.Stop();
                    _connectTimer.Close();
                    Invoke((MethodInvoker)delegate {
                        RefreshServerBoxContents();
                        HostInfoLabel.Text = $"Failed to connect to host!";
                        Connect.Enabled = true;
                        RefreshAllCompenentStates();
                    });
                    return;
                }
            }
        }

        #region Win32 API
#pragma warning disable 649
#pragma warning disable 169

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetScrollInfo(IntPtr hWnd, int scrollDirection, ref ScrollInfo si); //Thanks goes out to stever@GitHub for this!

        [DllImport("user32.dll")]
        static extern int SetScrollInfo(IntPtr hWnd, int scrollDirection, [In] ref ScrollInfo si, bool redraw);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        struct ScrollInfo {
            public uint Size;
            public uint Mask;
            public int Min;
            public int Max;
            public uint Page;
            public int Pos;
            public int TrackPos;
        }

        enum ScrollInfoMask {
            Range = 0x1,
            Page = 0x2,
            Pos = 0x4,
            DisableEndScroll = 0x8,
            TrackPos = 0x10,
            All = Range + Page + Pos + TrackPos
        }

        enum ScrollBarDirection {
            Horizontal = 0,
            Vertical = 1,
            Ctl = 2,
            Both = 3
        }

        new const int VerticalScroll = 277;
        const int LineUp = 0;
        const int LineDown = 1;
        const int ThumbPosition = 4;
        const int Thumbtrack = 5;
        const int ScrollTop = 6;
        const int ScrolBottom = 7;
        const int EndScroll = 8;

        const int SetRedraw = 0x000B;
        const int User = 0x400;
        const int GetEventMask = User + 59;
        const int SetEventMask = User + 69;

#pragma warning restore 649
#pragma warning restore 169
        #endregion


        [STAThread]
        public static void Main() {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnExit;
            Application.Run(FormManager.MainWindow);
        }

        public void RefreshServerBoxContents() {
            Invoke((MethodInvoker)delegate {
                Refresh();
                HostInfoLabel.Text = $"Connected to host:";
                if (_logManager.LogTask.Status != TaskStatus.Running) {
                    _logManager.InitLogThread();
                }
                ServerSelectBox.Items.Clear();
                if (connectedHost != null) {
                    _logManager.SetConnectedHost(connectedHost);
                    foreach (IServerConfiguration server in connectedHost.GetServerList()) {
                        ServerSelectBox.Items.Add(server.GetSettingsProp(ServerPropertyKeys.ServerName).ToString());
                    }

                    if (ServerSelectBox.Items.Count > 0) {
                        ServerSelectBox.SelectedIndex = 0;
                        SelectedServer = connectedHost.GetServerInfoByName((string)ServerSelectBox.SelectedItem);
                        FormManager.TCPClient.SendData(new() {
                            ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.SelectedServer),
                            Type = MessageTypes.ServerStatusRequest
                        });
                    } else {
                        SelectedServer = null;
                        Invoke(() => UpdateServerLogBox(LogBox, ""));
                    }
                }
                return;
            });
        }

        public Task RecieveExportData(ExportImportFileModel file) => Task.Run(() => {
            ExportImportManifestModel mainifest = file.Manifest;
            if (mainifest.FileType == FileTypes.WorldBackup && _backupManager != null) {
                _backupManager.RecieveExportData(file.Data);
            }
            if (mainifest.FileType == FileTypes.BedrockServer) {
                SaveFileDialog saveFileDialog = new() {
                    Filter = "Zip file|*.zip",
                    FileName = mainifest.Filename,
                    RestoreDirectory = true,
                    Title = "Save exported file..."
                };
                if (Invoke(saveFileDialog.ShowDialog) == DialogResult.OK) {
                    File.WriteAllBytes(saveFileDialog.FileName, file.Data);
                }
            }
            if (mainifest.FileType == FileTypes.ServiceConfig) {
                SaveFileDialog saveFileDialog = new() {
                    Filter = "Zip file|*.zip",
                    FileName = $"MMS_{mainifest.FileType}-{DateTime.Now:yyyyMMdd_hhmmssff}.zip",
                    RestoreDirectory = true,
                    Title = "Save exported file..."
                };
                if (Invoke(saveFileDialog.ShowDialog) == DialogResult.OK) {
                    File.WriteAllBytes(saveFileDialog.FileName, file.Data);
                }
            }
        });

        public void BackupRollbackCompleted(bool backupPassed) {
            if (_backupManager != null) {
                _backupManager.MarkRollbackComplete(backupPassed);
            }
        }

        public void InitForm() {
            ConfigManager.LoadConfigs();
            HostListBox.Items.Clear();
            foreach (ClientSideServiceConfiguration host in ConfigManager.HostConnectList) {
                HostListBox.Items.Add(host.GetHostName());
            }
            if (HostListBox.Items.Count > 0) {
                HostListBox.SelectedIndex = 0;
            }
            HostListBox.Refresh();
            FormClosing += MainWindow_FormClosing;
            if (ConfigManager.DefaultScrollLock) {
                scrollLockChkBox.Checked = true;
            }
        }

        public void ClientLogUpdate() {
            if (!_enableLogUpdating) { return; }
            try {
                Invoke(() => {
                    UpdateServerLogBox(clientLogBox, ProcessText(FormManager.ClientLogContainer.GetLog()));
                });
            } catch (Exception ex) {
                ClientLogger.AppendLine($"Error! {ex.Message} {ex.StackTrace}");
            }
        }

        private string ProcessText(List<LogEntry> targetLog) {
            if (FormManager.MainWindow.ConfigManager.DisplayTimestamps) {
                return string.Join("\r\n", targetLog.Select(x => $"[{x.TimeStamp:G}] {x.Text}").ToList());
            }
            return string.Join("\r\n", targetLog.Select(x => x.Text).ToList());
        }

        public void HeartbeatFailDisconnect() {
            Disconn_Click(null, null);
            try {
                HostInfoLabel.Invoke((MethodInvoker)delegate { HostInfoLabel.Text = "Lost connection to host!"; });
                ServerInfoBox.Invoke((MethodInvoker)delegate { ServerInfoBox.Text = "Lost connection to host!"; });
                ServerBusy = false;
                RefreshAllCompenentStates();
            } catch (InvalidOperationException) { }

            SelectedServer = null;
            connectedHost = null;
        }

        public void UpdateServerLogBox(TextBox targetBox, string contents) {
            int curPos;
            if (contents.Length > 0) {
                curPos = GetScrollPosition(targetBox);
                targetBox.Text = contents;
                SetScrollPosition(targetBox, curPos);
                if (_followTail) {
                    ScrollToEnd(targetBox);
                }
            } else {
                targetBox.Text = contents;
            }
        }

        private static void OnExit(object sender, EventArgs e) {
            FormManager.TCPClient.Dispose();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
            ClientLogger.AppendLine("Stopping log thread...");
            if (_logManager.StopLogThread()) {
                ClientLogger.AppendLine("Sending disconnect msg...");
                FormManager.TCPClient.SendData(Message.Empty(MessageTypes.Disconnect));
                ClientLogger.AppendLine("Closing connection...");
                FormManager.TCPClient.CloseConnection();
                SelectedServer = null;
                connectedHost = null;
            }
        }

        private void Connect_Click(object sender, EventArgs e) {
            _blockConnect = false;
            HostInfoLabel.Text = $"Connecting to host {(string)HostListBox.SelectedItem}...";
            Connect.Enabled = false;
            _connectTimeout = 0;
            _connectTimer.Interval = 100.0;
            _connectTimer.Start();
        }

        private void ServerSelectBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (connectedHost != null) {
                foreach (IServerConfiguration server in connectedHost.GetServerList()) {
                    if (ServerSelectBox.SelectedItem != null && ServerSelectBox.SelectedItem.ToString() == server.GetServerName()) {
                        SelectedServer = server;
                        FormManager.TCPClient.SendData(new() {
                            ServerIndex = connectedHost.GetServerIndex(server),
                            Type = MessageTypes.EnumBackups
                        });
                    }
                }
                RefreshAllCompenentStates();
            }
        }

        private void SingBackup_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(new() {
                ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.Backup
            });
            DisableUI("Performing server backup...");
        }

        private void EditCfg_Click(object sender, EventArgs e) {
            serverConfigBtnMenu.Show(EditCfg, new System.Drawing.Point { X = 0, Y = EditCfg.Height });
        }

        private void RestartSrv_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(new() {
                ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.Restart
            });
            DisableUI("Restarting a server...");
        }

        private void GlobBackup_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(Message.Empty(MessageTypes.BackupAll));
            DisableUI("Performing backups on all servers...");
        }

        private void newSrvBtn_Click(object sender, EventArgs e) {
            if (clientSideServiceConfiguration == null) {
                clientSideServiceConfiguration = ConfigManager.HostConnectList.First(host => host.GetHostName() == HostListBox.Text);
            }
            DisableUI("Service is preparing a new server...");
            FormManager.TCPClient.SendData(new() { Type = MessageTypes.VersionListRequest });
        }

        private void RemoveSrvBtn_Click(object sender, EventArgs e) {
            if (ServerSelectBox.Items.Count < 1) {
                FormManager.Logger.AppendLine("Server removal failed! You have no active servers!");
                MessageBox.Show("Server removal failed! You have no active servers!");
                return;
            }
            using (RemoveServerControl form = new()) {
                if (form.ShowDialog() == DialogResult.OK) {
                    FormManager.TCPClient.SendData(new Shared.Classes.Networking.Message { ServerIndex = connectedHost.GetServerIndex(SelectedServer), Type = MessageTypes.RemoveServer, Flag = form.SelectedFlag });
                    form.Close();
                    connectedHost.RemoveServerInfo(SelectedServer);
                }
            }
            RefreshServerBoxContents();
            DisableUI("Service is removing a server...");
        }

        private void PlayerManager_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(new() {
                ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.PlayersRequest
            });
            DisableUI("Gathering players registered to server...");
        }

        private void Disconn_Click(object sender, EventArgs e) {
            _connectTimer.Stop();
            try {
                if (SelectedServer != null && _backupManager != null) {
                    _backupManager.Close();
                    _backupManager.Dispose();
                }
                if (FormManager.TCPClient.Connected) {
                    FormManager.TCPClient.SendData(Message.Empty(MessageTypes.Disconnect));
                    Thread.Sleep(500);
                    FormManager.TCPClient.CloseConnection();
                }
                SelectedServer = null;
                connectedHost = null;
                _logManager.SetConnectedHost(null);
                FormManager.MainWindow.Invoke((MethodInvoker)delegate {
                    RefreshAllCompenentStates();
                    _logManager.ResetServerServiceLogCounts();
                    LogBox.Text = "";
                    serviceTextbox.Text = "";
                    ServerSelectBox.Items.Clear();
                    ServerSelectBox.SelectedIndex = -1;
                    ServerInfoBox.Text = "";
                    HostInfoLabel.Text = $"Select a host below:";
                });
            } catch (Exception) { }
        }

        private void SendCmd_Click(object sender, EventArgs e) {
            // store the last command in the consoleHistory.txt file
            if (_localCommandHistory.Count == 0 || (_localCommandHistory.Last() != cmdTextBox.Text && !string.IsNullOrEmpty(cmdTextBox.Text))) {
                _localCommandHistory.Add(cmdTextBox.Text);
            }
            if (_localCommandHistory.Count > 500) {
                // remove all lines that are not the last 500
                int linesToRemove = _localCommandHistory.Count - 500;
                _localCommandHistory.RemoveRange(0, linesToRemove);
            }
            File.WriteAllLines(GetServiceFilePath(MmsFileNameKeys.ClientCommandHistory), _localCommandHistory.ToArray());

            // send the command to the server
            if (cmdTextBox.Text.Length > 0 && connectedHost != null) {
                byte[] msg = Encoding.UTF8.GetBytes(cmdTextBox.Text);
                FormManager.TCPClient.SendData(new() {
                    Data = msg,
                    ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                    Type = MessageTypes.Command
                });
            }
            _distanceFromLastCommand = 0;
            logPageControl.SelectedTab = logPageControl.TabPages[0];

            cmdTextBox.Text = "";
        }

        private void EditStCmd_Click(object sender, EventArgs e) {
        }

        private void ChkUpdates_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(new() {
                ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.CheckUpdates
            });
            DisableUI("Service is checking updates for a server...");
        }

        public int GetScrollPosition(TextBox targetBox) {
            ScrollInfo si = new();
            si.Size = (uint)Marshal.SizeOf(si);
            si.Mask = (uint)ScrollInfoMask.All;
            GetScrollInfo(targetBox.Handle, (int)ScrollBarDirection.Vertical, ref si);
            return si.Pos;
        }

        public void SetScrollPosition(TextBox targetBox, int pos) {
            ScrollInfo si = new();
            si.Size = (uint)Marshal.SizeOf(si);
            si.Mask = (uint)ScrollInfoMask.All;
            GetScrollInfo(targetBox.Handle, (int)ScrollBarDirection.Vertical, ref si);
            si.Pos = pos;
            SetScrollInfo(targetBox.Handle, (int)ScrollBarDirection.Vertical, ref si, true);
            SendMessage(targetBox.Handle, VerticalScroll, new IntPtr(Thumbtrack + 0x10000 * si.Pos), new IntPtr(0));
        }

        public void ScrollToEnd(TextBox targetBox) {

            ScrollInfo si = new();
            si.Size = (uint)Marshal.SizeOf(si);
            si.Mask = (uint)ScrollInfoMask.All;
            GetScrollInfo(targetBox.Handle, (int)ScrollBarDirection.Vertical, ref si);

            si.Pos = si.Max - (int)si.Page;
            if (si.Pos > 0) {
                SetScrollInfo(targetBox.Handle, (int)ScrollBarDirection.Vertical, ref si, true);
                SendMessage(targetBox.Handle, VerticalScroll, new IntPtr(Thumbtrack + 0x20000 * si.Pos), new IntPtr(0));
            }
            _followTail = true;
        }

        public void RefreshAllCompenentStates() {
            bool hostConnectedIdle = connectedHost != null && !ServerBusy;
            bool serverConnectedIdle = connectedHost != null && SelectedServer != null && !ServerBusy;
            Connect.Enabled = connectedHost == null;
            Disconn.Enabled = connectedHost != null;
            newSrvBtn.Enabled = hostConnectedIdle;
            ChkUpdates.Enabled = serverConnectedIdle;
            GlobBackup.Enabled = serverConnectedIdle;
            BackupManagerBtn.Enabled = serverConnectedIdle;
            nbtStudioBtn.Enabled = serverConnectedIdle;
            ManPacks.Enabled = serverConnectedIdle;
            scrollLockChkBox.Enabled = serverConnectedIdle;
            removeSrvBtn.Enabled = serverConnectedIdle;
            EditCfg.Enabled = serverConnectedIdle;
            PlayerManagerBtn.Enabled = serverConnectedIdle;
            SingBackup.Enabled = serverConnectedIdle;
            SendCmd.Enabled = serverConnectedIdle;
            cmdTextBox.Enabled = serverConnectedIdle;
            RestartSrv.Enabled = serverConnectedIdle && SelectedServer.GetStatus() != null && SelectedServer.GetStatus().ServerStatus == ServerStatus.Started;
            startStopBtn.Enabled = serverConnectedIdle;
            startStopBtn.ForeColor = serverConnectedIdle && !IsPrimaryServer() ?
                System.Drawing.Color.Black :
                System.Drawing.Color.LightGray;

            startStopBtn.Text = SelectedServer == null ?
                "Start/Stop" :
                SelectedServer.GetStatus() != null && SelectedServer.GetStatus().ServerStatus == ServerStatus.Stopped ?
                "Start" :
                "Stop";

        }

        public void DisableUI(string message, Task onCompletion = null) {
            Invoke(() => {
                _uiWaitDialog = new(null);
                ServerBusy = true;
                _uiWaitTimer.AutoReset = false;
                int waitCount = 0;
                _uiWaitTimer.Elapsed += (s, e) => {
                    if (!ServerBusy) {
                        _uiWaitDialog.GetDialogProgress().Report(new("Message recieved.", 100));
                        _uiWaitDialog.EndProgress(new(() => {
                            Invoke((MethodInvoker)delegate { RefreshAllCompenentStates(); });
                            if (onCompletion != null && !onCompletion.IsCompleted) {
                                onCompletion.Start();
                                while (!onCompletion.IsCompleted) {
                                }
                            }
                            Invoke(_uiWaitDialog.Close);
                            Invoke(_uiWaitDialog.Dispose);
                        }));
                    } else {
                        waitCount += 5;
                        _uiWaitDialog.GetDialogProgress().Report(new(message, waitCount <= 100 ? waitCount : 100));
                        _uiWaitTimer.Start();
                    }
                };
                _uiWaitDialog.Show();
                _uiWaitDialog.GetDialogProgress().Report(new(message, waitCount));
                Invoke((MethodInvoker)delegate { RefreshAllCompenentStates(); });
                _uiWaitTimer.Start();
            });
        }

        public void VersionListArrived(Dictionary<MinecraftServerArch, SimpleVersionModel[]> verLists) {
            ServerBusy = false;
            using (_newServerForm = new(clientSideServiceConfiguration, connectedHost.GetServerList(), verLists)) {
                if (_newServerForm.ShowDialog() == DialogResult.OK) {
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_newServerForm.SelectedPropModel, Formatting.Indented, GlobalJsonSerialierSettings));
                    FormManager.TCPClient.SendData(new() {
                        Data = serializeToBytes,
                        Type = MessageTypes.AddNewServer
                    });
                    _newServerForm.Close();
                    _newServerForm.Dispose();
                }
            }
        }

        public void RecievePackData(byte serverIndex, List<Shared.PackParser.MinecraftPackContainer> incomingPacks) {
            _incomingPacks = incomingPacks;
            _manPacksServer = serverIndex;
            ServerBusy = false;
        }

        public void RecievePlayerData(byte serverIndex, List<Player> playerList) {
            ServerBusy = false;
            _uiWaitDialog.SetCallback(new(() => {
                connectedHost.GetServerInfoByIndex(serverIndex).SetPlayerList(playerList);
                PlayerManagerForm form = new(SelectedServer);
                form.ShowDialog();
            }));
        }

        private void scrollLockChkBox_CheckedChanged(object sender, EventArgs e) {
            _followTail = scrollLockChkBox.Checked;
            ScrollToEnd(LogBox);
            ScrollToEnd(clientLogBox);
            ScrollToEnd(serviceTextbox);
        }

        private void cmdTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                SendCmd_Click(null, null);
            }
        }

        private void cmdTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) {
                if (_localCommandHistory.Count > 0) {
                    // Ensure _distanceFromLastCommand is within valid range
                    _distanceFromLastCommand = _distanceFromLastCommand < 0 ? 0 : _distanceFromLastCommand;

                    if (e.KeyCode == Keys.Up) {
                        if (_distanceFromLastCommand < _localCommandHistory.Count) {
                            _distanceFromLastCommand++;
                        }
                    } else {
                        if (_distanceFromLastCommand > 0) {
                            _distanceFromLastCommand--;
                        }
                    }

                    int index = _localCommandHistory.Count - _distanceFromLastCommand;
                    if (index >= 0 && index < _localCommandHistory.Count) {
                        cmdTextBox.Text = _localCommandHistory.ElementAt(index);
                    }
                }
            }
        }

        private void cmdTextBox_TextUpdate(object sender, KeyPressEventArgs e) {
            // offer suggestions here
        }

        private void HostListBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                Connect_Click(null, null);
            }
        }

        private void BackupManager_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(new() {
                ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.EnumBackups
            });
            if (_backupManager == null || _backupManager.IsDisposed) {
                _backupManager = new BackupManagerForm();
            }
            _backupManager.ShowDialog();

        }

        public void UpdateBackupManagerData(List<BackupInfoModel> backupInfo) => _backupManager?.UpdateBackupManagerData(backupInfo);

        private void ManPacks_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(new() { 
                ServerIndex = connectedHost.GetServerIndex(SelectedServer), 
                Type = MessageTypes.PackList 
            });
            DisableUI("Service is gathering current pack information...", new(() => {
                ManagePacksForms form = new(_manPacksServer, ClientLogger, _processInfo);
                form.PopulateServerData(_incomingPacks);
                Invoke(() => form.Show(this));
            }));
        }

        private void nbtStudioBtn_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(ConfigManager.NBTStudioPath))
                using (OpenFileDialog openFile = new()) {
                    openFile.FileName = "NBTStudio.exe";
                    openFile.Title = "Please locate NBT Studio executable...";
                    openFile.Filter = "NBTStudio.exe|NBTStudio.exe";
                    if (openFile.ShowDialog() == DialogResult.OK) {
                        ConfigManager.NBTStudioPath = openFile.FileName;
                        ConfigManager.SaveConfigFile();
                    } else return;
                }
            ServerBusy = true;
            FormManager.TCPClient.SendData(new() {
                ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.LevelEditRequest
            });
            DisableUI("Service is gathering level.dat...");
        }

        public void LevelDatRecieved(string path) {
            using (Process nbtStudioProcess = new()) {
                nbtStudioProcess.StartInfo = new ProcessStartInfo(ConfigManager.NBTStudioPath, path);
                nbtStudioProcess.Start();
                nbtStudioProcess.WaitForExit();
                FormManager.TCPClient.SendData(new() {
                    Data = File.ReadAllBytes(path),
                    ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                    Type = MessageTypes.LevelEditFile
                });
            }
            ServerBusy = false;
        }

        private void HostListBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (HostListBox.SelectedIndex != -1) {
                clientSideServiceConfiguration = ConfigManager.HostConnectList.FirstOrDefault(host => host.GetHostName() == (string)HostListBox.SelectedItem);
            }
        }

        private void MainWindow_Load(object sender, EventArgs e) {
        }

        private void clientConfigBtn_Click(object sender, EventArgs e) {
            using (ClientConfigForm form = new(ConfigManager)) {
                if (form.ShowDialog() == DialogResult.OK) {
                    form.Close();
                    InitForm();
                }
            }
        }

        private void startStopBtn_Click(object sender, EventArgs e) {
            if (!IsPrimaryServer()) {
                DisableUI("Starting/Stopping a server...");
                FormManager.TCPClient.SendData(new() {
                    ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                    Type = MessageTypes.StartStop
                });
            } else {
                startStopBtnToolTip.Active = true;
                startStopBtnToolTip.Show("Start/Stop feature is disabled to servers using the primary ports 19132 and 19133.", startStopBtn, 2000);
                Task.Run(() => {
                    Task.Delay(2500).Wait();
                    startStopBtnToolTip.Active = false;
                });
            }
        }

        private bool IsPrimaryServer() {
            return SelectedServer.IsPrimaryServer();
        }

        private void serverPropMenuItem_Click(object sender, EventArgs e) {
            using (PropEditorForm _editDialog = new()) {
                _editDialog.PopulateBoxes(SelectedServer.GetAllProps());
                if (_editDialog.ShowDialog() == DialogResult.OK) {
                    JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                    FormManager.TCPClient.SendData(new() {
                        Data = serializeToBytes,
                        ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                        Type = MessageTypes.PropUpdate
                    });
                    SelectedServer.SetAllProps(_editDialog.workingProps);
                }
            }
            ServerBusy = false;
        }

        private void startCmdMenuItem_Click(object sender, EventArgs e) {
            PropEditorForm editSrvDialog = new();
            editSrvDialog.PopulateStartCmds(SelectedServer.GetStartCommands());
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(editSrvDialog.startCmds, Formatting.Indented, settings));
                DisableUI("Getting current server start commands from service...");
                FormManager.TCPClient.SendData(new() {
                    Data = serializeToBytes,
                    ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                    Type = MessageTypes.StartCmdUpdate
                });
                SelectedServer.SetStartCommands(editSrvDialog.startCmds);
                editSrvDialog.Close();
                editSrvDialog.Dispose();
            }
        }

        private void servicePropMenuItem_Click(object sender, EventArgs e) {
            using (PropEditorForm _editDialog = new()) {
                List<string> serviceConfigExcludeList = new() { "ServerName", "ServerExeName", "FileName", "ServerPath", "DeployedVersion" };
                List<Property> filteredProps = new List<Property>(SelectedServer.GetSettingsList()
                            .Where(x => !serviceConfigExcludeList.Contains(x.KeyName)));
                _editDialog.PopulateBoxes(filteredProps);
                if (_editDialog.ShowDialog() == DialogResult.OK) {
                    JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                    FormManager.TCPClient.SendData(new() {
                        Data = serializeToBytes,
                        ServerIndex = connectedHost.GetServerIndex(SelectedServer),
                        Type = MessageTypes.PropUpdate
                    });
                    SelectedServer.SetAllSettings(_editDialog.workingProps);
                }
            }
            ServerBusy = false;
        }

        private void editCoreServicePropertiesToolStripMenuItem_Click(object sender, EventArgs e) {
            using (PropEditorForm _editDialog = new()) {
                _editDialog.PopulateBoxes(connectedHost.GetAllProps());
                if (_editDialog.ShowDialog() == DialogResult.OK) {
                    JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                    FormManager.TCPClient.SendData(new() {
                        Data = serializeToBytes,
                        Type = MessageTypes.PropUpdate
                    });
                    connectedHost.SetAllProps(_editDialog.workingProps);
                    ServerBusy = true;
                    DisableUI("Gathering current service props...");
                }
            }
            ServerBusy = false;
        }

        private void asConfigOnlyToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportManifestModel manifest = new() {
                FileType = FileTypes.ServerConfig
            };
            ExportImportFileModel model = new() {
                Manifest = manifest,
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(new() {
                Data = serializedBytes,
                ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.ExportFile
            });
        }

        private void importableBackupToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportManifestModel manifestModel = new() {
                FileType = FileTypes.WorldBackup
            };
            byte[] serializedBytes = []; // Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(new() {
                Data = serializedBytes,
                ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.ExportFile
            });
        }

        private void importableBackupWithPacksToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportManifestModel manifestModel = new() {
                FileType = FileTypes.WorldBackup | FileTypes.ServerPacks,
            };
            ExportImportFileModel model = new() {
                Manifest = manifestModel
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(new() {
                Data = serializedBytes,
                ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.ExportFile
            });
        }

        private void fullServerPackageToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportManifestModel manifestModel = new() {
                FileType = FileTypes.BedrockServer
            };

            ExportImportFileModel model = new() {
                Manifest = manifestModel
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(new() {
                Data = serializedBytes,
                ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.ExportFile
            });
        }

        private void serviceConfigFileToolStripMenuItem1_Click(object sender, EventArgs e) {
            ExportImportManifestModel manifestModel = new() {
                FileType = FileTypes.ServiceConfig,
            };
            ExportImportFileModel model = new() {
                Manifest = manifestModel
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(new() {
                Data = serializedBytes,
                ServerIndex = FormManager.MainWindow.connectedHost.GetServerIndex(SelectedServer),
                Type = MessageTypes.ExportFile
            });
        }

        private void serverPackageFileToolStripMenuItem_Click(object sender, EventArgs e) {

        }

        private void ImportableZipFileItemClicked(object sender, EventArgs e) {
            byte[] fileBytes = OpenPackageFile();
            if (fileBytes != null) {
                ExportImportFileModel model = new(fileBytes);
                byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
                FormManager.TCPClient.SendData(new() {
                    Data = serializedBytes,
                    ServerIndex = 0xFF,
                    Type = MessageTypes.ImportFile
                });
            }
        }

        private byte[] OpenPackageFile() {
            OpenFileDialog ofd = new() {
                Filter = "Zip file|*.zip",
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Open package file..."
            };
            byte[] fileBytes = null;
            if (ofd.ShowDialog() == DialogResult.OK) {
                if (ofd.FileName == string.Empty) {
                    return null;
                }
                fileBytes = File.ReadAllBytes(ofd.FileName);
                if (fileBytes.Length < 3 && fileBytes[0] != 0x50 && fileBytes[1] != 0x4B) {
                    return null;
                }
            }
            return fileBytes;
        }
    }
}
