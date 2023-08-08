using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Classes.Configurations;
using BedrockService.Shared.Interfaces;
using BedrockService.Shared.SerializeModels;
using Newtonsoft.Json;
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
using static BedrockService.Shared.Classes.SharedStringBase;

namespace BedrockService.Client.Forms
{
    public partial class MainWindow : Form {
        public IServiceConfiguration connectedHost;
        public IServerConfiguration selectedServer;
        public IClientSideServiceConfiguration clientSideServiceConfiguration;
        public ServiceStatusModel ServiceStatus;
        public bool ServerBusy = false;
        private int _connectTimeout;
        private bool _followTail = false;
        private bool _blockConnect = false;
        private bool _enableLogUpdating = false;
        private const int _connectTimeoutLimit = 3;
        private BackupManagerForm _backupManager;
        public IServerLogger ClientLogger;
        private readonly IProcessInfo _processInfo;
        private readonly System.Timers.Timer _connectTimer = new(100.0);
        private readonly LogManager _logManager;
        public ConfigManager ConfigManager;
        public MainWindow(IProcessInfo processInfo, IServerLogger logger) {
            _processInfo = processInfo;
            ClientLogger = logger;
            ClientLogger.Initialize();
            _logManager = new LogManager(ClientLogger);
            ConfigManager = new ConfigManager(ClientLogger);
            InitializeComponent();
            InitForm();
            _connectTimer.Elapsed += ConnectTimer_Elapsed;
            Shown += MainWindow_Shown;
        }

        private void MainWindow_Shown(object sender, EventArgs e) {
            _enableLogUpdating = true;
            _logManager.InitLogThread();
        }

        private void ConnectTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (_connectTimer.Enabled && !FormManager.TCPClient.EstablishedLink && !_blockConnect) {
                _connectTimer.Interval = 2000.0;
                _blockConnect = true;
                Invoke((MethodInvoker)delegate { FormManager.TCPClient.ConnectHost(ConfigManager.HostConnectList.FirstOrDefault(host => host.GetHostName() == (string)HostListBox.SelectedItem)); });
                Thread.Sleep(1000);
                if (connectedHost != null && FormManager.TCPClient.EstablishedLink) {
                    ServerBusy = false;
                    Invoke((MethodInvoker)delegate { RefreshAllCompenentStates(); });
                    _connectTimer.Enabled = false;
                    _connectTimer.Stop();
                    _connectTimer.Close();
                    return;
                }
                _connectTimeout++;
                _blockConnect = false;
                if (_connectTimeout >= _connectTimeoutLimit) {
                    _connectTimer.Enabled = false;
                    _connectTimer.Stop();
                    _connectTimer.Close();
                    Invoke((MethodInvoker)delegate {
                        RefreshServerContents();
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

        public void RefreshServerContents() {
            Invoke((MethodInvoker)delegate {
                Refresh();
                return;
            });
        }

        public void RecieveExportData(ExportImportFileModel file) {
            if (file == null) {
                return;
            }
            if (file.FileType == FileTypeFlags.Backup && _backupManager != null) {
                _backupManager.RecieveExportData(file.Data);
            }
            if (file.FileType == FileTypeFlags.ServerPackage) {
                SaveFileDialog saveFileDialog = new() {
                    Filter = "Zip file|*.zip",
                    FileName = $"{file.FileType}-{selectedServer.GetServerName()}-{DateTime.Now:yyyyMMdd_hhmmssff}.zip",
                    RestoreDirectory = true,
                    Title = "Save exported file..."
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    File.WriteAllBytes(saveFileDialog.FileName, file.Data);
                }
            }
            if (file.FileType == FileTypeFlags.ServicePackage) {
                SaveFileDialog saveFileDialog = new() {
                    Filter = "Zip file|*.zip",
                    FileName = $"BMS_{file.FileType}-{DateTime.Now:yyyyMMdd_hhmmssff}.zip",
                    RestoreDirectory = true,
                    Title = "Save exported file..."
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    File.WriteAllBytes(saveFileDialog.FileName, file.Data);
                }
            }
        }

        public override void Refresh() {
            HostInfoLabel.Text = $"Connected to host:";
            ServerSelectBox.Items.Clear();
            if (connectedHost != null) {
                _logManager.SetConnectedHost(connectedHost);
                foreach (BedrockConfiguration server in connectedHost.GetServerList()) {
                    ServerSelectBox.Items.Add(server.GetSettingsProp(ServerPropertyKeys.ServerName).ToString());
                }

                if (ServerSelectBox.Items.Count > 0) {
                    ServerSelectBox.SelectedIndex = 0;
                    selectedServer = connectedHost.GetServerInfoByName((string)ServerSelectBox.SelectedItem);
                }
            }
        }

        public void InitForm() {
            ConfigManager.LoadConfigs();
            HostListBox.Items.Clear();
            foreach (IClientSideServiceConfiguration host in ConfigManager.HostConnectList) {
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
            if(!_enableLogUpdating) { return; }
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
                if (selectedServer != null && _backupManager != null && _backupManager.Created) {
                    _backupManager.Close();
                    _backupManager.Dispose();
                }
                HostInfoLabel.Invoke((MethodInvoker)delegate { HostInfoLabel.Text = "Lost connection to host!"; });
                ServerInfoBox.Invoke((MethodInvoker)delegate { ServerInfoBox.Text = "Lost connection to host!"; });
                ServerBusy = false;
                RefreshAllCompenentStates();
            } catch (InvalidOperationException) { }

            selectedServer = null;
            connectedHost = null;
        }

        public void UpdateServerLogBox(TextBox targetBox, string contents) {
            int curPos;
            if (contents.Length > 0 && targetBox.TextLength != contents.Length) {
                curPos = GetScrollPosition(targetBox);
                targetBox.Text = contents;
                SetScrollPosition(targetBox, curPos);
            }
            if (_followTail)
                ScrollToEnd(targetBox);
        }

        public void RecievePlayerData(byte serverIndex, List<IPlayer> playerList) {
            connectedHost.GetServerInfoByIndex(serverIndex).SetPlayerList(playerList);

            PlayerManagerForm form = new(selectedServer);
            if (form.ShowDialog() != DialogResult.OK) {
                ServerBusy = false;
            }
        }

        private static void OnExit(object sender, EventArgs e) {
            FormManager.TCPClient.Dispose();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e) {
            ClientLogger.AppendLine("Stopping log thread...");
            if (_logManager.StopLogThread()) {
                ClientLogger.AppendLine("Sending disconnect msg...");
                FormManager.TCPClient.SendData(NetworkMessageTypes.Disconnect);
                ClientLogger.AppendLine("Closing connection...");
                FormManager.TCPClient.CloseConnection();
                selectedServer = null;
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
                foreach (BedrockConfiguration server in connectedHost.GetServerList()) {
                    if (ServerSelectBox.SelectedItem != null && ServerSelectBox.SelectedItem.ToString() == server.GetServerName()) {
                        selectedServer = server;
                        FormManager.TCPClient.SendData(connectedHost.GetServerIndex(server), NetworkMessageTypes.EnumBackups);
                    }
                }
                RefreshAllCompenentStates();
            }
        }

        private void SingBackup_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.Backup);
            DisableUI();
        }

        private void EditCfg_Click(object sender, EventArgs e) {
            serverConfigBtnMenu.Show(EditCfg, new System.Drawing.Point { X = 0, Y = EditCfg.Height });
        }

        private void RestartSrv_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.Restart);
            DisableUI();
        }

        private void GlobBackup_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(NetworkMessageTypes.BackupAll);
            DisableUI();
        }

        private void newSrvBtn_Click(object sender, EventArgs e) {
            if (clientSideServiceConfiguration == null) {
                clientSideServiceConfiguration = ConfigManager.HostConnectList.First(host => host.GetHostName() == HostListBox.Text);
            }
            DisableUI();
            using (AddNewServerForm newServerForm = new(clientSideServiceConfiguration, connectedHost.GetServerList())) {
                if (newServerForm.ShowDialog() == DialogResult.OK) {
                    JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newServerForm.ServerCombinedPropModel, Formatting.Indented, settings));
                    FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageTypes.AddNewServer);
                    newServerForm.Close();
                    newServerForm.Dispose();
                }
            }
            ServerBusy = false;
        }

        private void RemoveSrvBtn_Click(object sender, EventArgs e) {
            using (RemoveServerControl form = new()) {
                if (form.ShowDialog() == DialogResult.OK) {
                    FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.RemoveServer, form.SelectedFlag);
                    form.Close();
                }
            }
            connectedHost.RemoveServerInfo(selectedServer);
            DisableUI();
        }

        private void PlayerManager_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PlayersRequest);
            DisableUI();
        }

        private void Disconn_Click(object sender, EventArgs e) {
            _connectTimer.Stop();
            if (_logManager.StopLogThread()) {
                try {
                    if (FormManager.TCPClient.Connected) {
                        FormManager.TCPClient.SendData(NetworkMessageTypes.Disconnect);
                        Thread.Sleep(500);
                        FormManager.TCPClient.CloseConnection();
                    }
                    selectedServer = null;
                    connectedHost = null;
                    _logManager.SetConnectedHost(null);
                    LogBox.Invoke((MethodInvoker)delegate { LogBox.Text = ""; });
                    FormManager.MainWindow.Invoke((MethodInvoker)delegate {
                        RefreshAllCompenentStates();
                        ServerSelectBox.Items.Clear();
                        ServerSelectBox.SelectedIndex = -1;
                        ServerInfoBox.Text = "";
                        HostInfoLabel.Text = $"Select a host below:";
                    });
                } catch (Exception) { }

            }
        }

        private void SendCmd_Click(object sender, EventArgs e) {
            if (cmdTextBox.Text.Length > 0) {
                byte[] msg = Encoding.UTF8.GetBytes(cmdTextBox.Text);
                FormManager.TCPClient.SendData(msg, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.Command);
            }
            cmdTextBox.Text = "";
        }

        private void EditStCmd_Click(object sender, EventArgs e) {
        }

        private void ChkUpdates_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.CheckUpdates);
            DisableUI();
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
            bool serverConnectedIdle = connectedHost != null && selectedServer != null && !ServerBusy;
            Connect.Enabled = connectedHost == null;
            Disconn.Enabled = connectedHost != null;
            newSrvBtn.Enabled = hostConnectedIdle;
            ChkUpdates.Enabled = hostConnectedIdle;
            GlobBackup.Enabled = hostConnectedIdle;
            BackupManagerBtn.Enabled = serverConnectedIdle;
            nbtStudioBtn.Enabled = serverConnectedIdle;
            ManPacks.Enabled = serverConnectedIdle;
            scrollLockChkBox.Enabled = serverConnectedIdle;
            removeSrvBtn.Enabled = serverConnectedIdle;
            EditCfg.Enabled = serverConnectedIdle;
            PlayerManagerBtn.Enabled = serverConnectedIdle;
            SingBackup.Enabled = serverConnectedIdle;
            ServerInfoBox.Enabled = serverConnectedIdle;
            SendCmd.Enabled = serverConnectedIdle;
            cmdTextBox.Enabled = serverConnectedIdle;
            RestartSrv.Enabled = serverConnectedIdle && selectedServer.GetStatus() != null && selectedServer.GetStatus().ServerStatus == ServerStatus.Started;
            startStopBtn.Enabled = serverConnectedIdle;
            startStopBtn.ForeColor = serverConnectedIdle && !IsPrimaryServer() ?
                System.Drawing.Color.Black :
                System.Drawing.Color.LightGray;

            startStopBtn.Text = selectedServer == null ?
                "Start/Stop" :
                selectedServer.GetStatus() != null && selectedServer.GetStatus().ServerStatus == ServerStatus.Stopped ?
                "Start" :
                "Stop";

        }

        public Task DisableUI() {
            ServerBusy = true;
            return Task.Run(() => {
                Invoke((MethodInvoker)delegate { RefreshAllCompenentStates(); });
                while (ServerBusy) {
                    Task.Delay(250).Wait();
                }
                Invoke((MethodInvoker)delegate { RefreshAllCompenentStates(); });
            });
        }

        public Task WaitForServerData() {
            return Task.Run(() => {
                while (!FormManager.TCPClient.PlayerInfoArrived && FormManager.TCPClient.RecievedPacks == null) {
                    Task.Delay(250).Wait();
                }
            });
        }

        public class ServerConnectException : Exception {
            public ServerConnectException() { }

            public ServerConnectException(string message)
                : base(message) {

            }

            public ServerConnectException(string message, Exception inner)
                : base(message, inner) {

            }
        }

        public void RecievePackData(byte serverIndex, List<Shared.PackParser.MinecraftPackContainer> incomingPacks) {
            Invoke((MethodInvoker)delegate {
                using (ManagePacksForms form = new(serverIndex, ClientLogger, _processInfo)) {
                    form.PopulateServerData(incomingPacks);
                    form.ShowDialog();
                    ServerBusy = false;
                }
            });
        }

        private void scrollLockChkBox_CheckedChanged(object sender, EventArgs e) => _followTail = scrollLockChkBox.Checked;

        private void cmdTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                SendCmd_Click(null, null);
            }
        }

        private void HostListBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                Connect_Click(null, null);
            }
        }

        private void BackupManager_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.EnumBackups);
            if (_backupManager == null || _backupManager.IsDisposed) {
                _backupManager = new BackupManagerForm();
            }
            _backupManager.ShowDialog();

        }

        public void UpdateBackupManagerData() => _backupManager?.UpdateBackupManagerData();

        private void ManPacks_Click(object sender, EventArgs e) {
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PackList);
            DisableUI();
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
            FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.LevelEditRequest);
            DisableUI();
            using (Process nbtStudioProcess = new()) {
                string tempPath = $@"{Path.GetTempPath()}level.dat";
                nbtStudioProcess.StartInfo = new ProcessStartInfo(ConfigManager.NBTStudioPath, tempPath);
                nbtStudioProcess.Start();
                nbtStudioProcess.WaitForExit();
                FormManager.TCPClient.SendData(File.ReadAllBytes(tempPath), connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.LevelEditFile);
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
                DisableUI();
                FormManager.TCPClient.SendData(connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.StartStop);
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
            return selectedServer.GetProp(BmsDependServerPropKeys.PortI4).StringValue == "19132" ||
            selectedServer.GetProp(BmsDependServerPropKeys.PortI4).StringValue == "19133" ||
            selectedServer.GetProp(BmsDependServerPropKeys.PortI6).StringValue == "19132" ||
            selectedServer.GetProp(BmsDependServerPropKeys.PortI6).StringValue == "19133";
        }

        private void serverPropMenuItem_Click(object sender, EventArgs e) {
            using (PropEditorForm _editDialog = new()) {
                _editDialog.PopulateBoxes(selectedServer.GetAllProps());
                if (_editDialog.ShowDialog() == DialogResult.OK) {
                    JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                    FormManager.TCPClient.SendData(serializeToBytes, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PropUpdate);
                    selectedServer.SetAllProps(_editDialog.workingProps);
                }
            }
            ServerBusy = false;
        }

        private void startCmdMenuItem_Click(object sender, EventArgs e) {
            PropEditorForm editSrvDialog = new();
            editSrvDialog.PopulateStartCmds(selectedServer.GetStartCommands());
            if (editSrvDialog.ShowDialog() == DialogResult.OK) {
                JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(editSrvDialog.startCmds, Formatting.Indented, settings));
                DisableUI();
                FormManager.TCPClient.SendData(serializeToBytes, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.StartCmdUpdate);
                selectedServer.SetStartCommands(editSrvDialog.startCmds);
                editSrvDialog.Close();
                editSrvDialog.Dispose();
            }
        }

        private void servicePropMenuItem_Click(object sender, EventArgs e) {
            using (PropEditorForm _editDialog = new()) {
                List<string> serviceConfigExcludeList = new() { "ServerName", "ServerExeName", "FileName", "ServerPath", "DeployedVersion" };
                List<Property> filteredProps = new List<Property>(selectedServer.GetSettingsList()
                            .Where(x => !serviceConfigExcludeList.Contains(x.KeyName)));
                _editDialog.PopulateBoxes(filteredProps);
                if (_editDialog.ShowDialog() == DialogResult.OK) {
                    JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.All };
                    byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                    FormManager.TCPClient.SendData(serializeToBytes, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PropUpdate);
                    selectedServer.SetAllSettings(_editDialog.workingProps);
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
                    FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageTypes.PropUpdate);
                    connectedHost.SetAllProps(_editDialog.workingProps);
                    ServerBusy = true;
                    DisableUI();
                }
            }
            ServerBusy = false;
        }

        private void asConfigOnlyToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportFileModel model = new() {
                FileType = FileTypeFlags.ServerPackage,
                PackageFlags = PackageFlags.ConfigFile
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(serializedBytes, FormManager.MainWindow.connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.ExportFile);
        }

        private void importableBackupToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportFileModel model = new() {
                FileType = FileTypeFlags.ServerPackage,
                PackageFlags = PackageFlags.LastBackup
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(serializedBytes, FormManager.MainWindow.connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.ExportFile);
        }

        private void importableBackupWithPacksToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportFileModel model = new() {
                FileType = FileTypeFlags.ServerPackage,
                PackageFlags = PackageFlags.WorldPacks
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(serializedBytes, FormManager.MainWindow.connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.ExportFile);
        }

        private void fullServerPackageToolStripMenuItem_Click(object sender, EventArgs e) {
            ExportImportFileModel model = new() {
                FileType = FileTypeFlags.ServerPackage,
                PackageFlags = PackageFlags.Full
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(serializedBytes, FormManager.MainWindow.connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.ExportFile);
        }

        private void serviceConfigFileToolStripMenuItem1_Click(object sender, EventArgs e) {
            ExportImportFileModel model = new() {
                FileType = FileTypeFlags.ServicePackage,
                PackageFlags = PackageFlags.ConfigFile
            };
            byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            FormManager.TCPClient.SendData(serializedBytes, FormManager.MainWindow.connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.ExportFile);
        }

        private void serverPackageFileToolStripMenuItem_Click(object sender, EventArgs e) {
            byte[] fileBytes = OpenPackageFile();
            if (fileBytes != null) {
                ExportImportFileModel fileModel = new() {
                    Data = fileBytes,
                    FileType = FileTypeFlags.ServerPackage
                };
                byte[] serializedBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fileModel));
                FormManager.TCPClient.SendData(serializedBytes, 0xFF, NetworkMessageTypes.ImportFile);
            }
        }

        private void serviceConfigFileToolStripMenuItem_Click(object sender, EventArgs e) {

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
