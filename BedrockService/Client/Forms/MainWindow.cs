using BedrockService.Client.Management;
using BedrockService.Shared.Classes;
using BedrockService.Shared.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class MainWindow : Form
    {
        public IServiceConfiguration connectedHost;
        public IServerConfiguration selectedServer;
        public IClientSideServiceConfiguration clientSideServiceConfiguration;
        public bool ShowsSvcLog = false;
        public bool ServerBusy = false;
        private PropEditorForm _editDialog;
        private int _connectTimeout;
        private bool _followTail = false;
        private const int _connectTimeoutLimit = 3;
        private readonly IBedrockLogger _logger;
        private readonly IProcessInfo _processInfo;
        private readonly System.Timers.Timer _connectTimer = new System.Timers.Timer(100.0);
        private readonly LogManager _logManager;
        private readonly ConfigManager _configManager;
        public MainWindow(IProcessInfo processInfo, IBedrockLogger logger)
        {
            _processInfo = processInfo;
            _logger = logger;
            _logManager = new LogManager(_logger);
            _configManager = new ConfigManager(_logger);
            InitializeComponent();
            InitForm();
            SvcLog.CheckedChanged += SvcLog_CheckedChanged;
            _connectTimer.Elapsed += ConnectTimer_Elapsed;
        }

        private void ConnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_connectTimer.Enabled && !FormManager.TCPClient.EstablishedLink)
            {
                _connectTimer.Interval = 2000.0;
                Invoke((MethodInvoker)delegate { FormManager.TCPClient.ConnectHost(_configManager.HostConnectList.FirstOrDefault(host => host.GetHostName() == (string)HostListBox.SelectedItem)); });
                Thread.Sleep(500);
                if (connectedHost != null && FormManager.TCPClient.EstablishedLink)
                {
                    ServerBusy = false;
                    Invoke((MethodInvoker)delegate { ComponentEnableManager(); });
                    _connectTimer.Enabled = false;
                    _connectTimer.Stop();
                    _connectTimer.Close();
                    return;
                }
                _connectTimeout++;
                if (_connectTimeout >= _connectTimeoutLimit)
                {
                    _connectTimer.Enabled = false;
                    _connectTimer.Stop();
                    _connectTimer.Close();
                    Invoke((MethodInvoker)delegate
                    {
                        RefreshServerContents();
                        HostInfoLabel.Text = $"Failed to connect to host!";
                        Connect.Enabled = true;
                        ComponentEnableManager();
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

        struct ScrollInfo
        {
            public uint Size;
            public uint Mask;
            public int Min;
            public int Max;
            public uint Page;
            public int Pos;
            public int TrackPos;
        }

        enum ScrollInfoMask
        {
            Range = 0x1,
            Page = 0x2,
            Pos = 0x4,
            DisableEndScroll = 0x8,
            TrackPos = 0x10,
            All = Range + Page + Pos + TrackPos
        }

        enum ScrollBarDirection
        {
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
        const int GetEventMask = (User + 59);
        const int SetEventMask = (User + 69);

#pragma warning restore 649
#pragma warning restore 169
        #endregion


        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnExit;
            Application.Run(FormManager.MainWindow);
        }

        public void RefreshServerContents()
        {
            Invoke((MethodInvoker)delegate
            {
                Refresh();
                return;
            });
        }

        public override void Refresh()
        {
            HostInfoLabel.Text = $"Connected to host:";
            ServerSelectBox.Items.Clear();
            if (connectedHost != null)
            {
                _logManager.InitLogThread(connectedHost);
                foreach (ServerInfo server in connectedHost.GetServerList())
                    ServerSelectBox.Items.Add(server.ServerName);
                if (ServerSelectBox.Items.Count > 0)
                {
                    ServerSelectBox.SelectedIndex = 0;
                    selectedServer = connectedHost.GetServerInfoByName((string)ServerSelectBox.SelectedItem);
                }
            }
            ServerSelectBox.Refresh();
            base.Refresh();
        }

        public void InitForm()
        {
            _configManager.LoadConfigs();
            HostListBox.Items.Clear();
            foreach (IClientSideServiceConfiguration host in _configManager.HostConnectList)
            {
                HostListBox.Items.Add(host.GetHostName());
            }
            if (HostListBox.Items.Count > 0)
            {
                HostListBox.SelectedIndex = 0;
            }
            HostListBox.Refresh();
            FormClosing += MainWindow_FormClosing;
        }

        public void HeartbeatFailDisconnect()
        {
            Disconn_Click(null, null);
            try
            {
                HostInfoLabel.Invoke((MethodInvoker)delegate { HostInfoLabel.Text = "Lost connection to host!"; });
                ServerInfoBox.Invoke((MethodInvoker)delegate { ServerInfoBox.Text = "Lost connection to host!"; });
                ServerBusy = false;
                ComponentEnableManager();
            }
            catch (InvalidOperationException) { }

            selectedServer = null;
            connectedHost = null;
        }

        public void UpdateLogBox(string contents)
        {
            int curPos = HorizontalScrollPosition;
            LogBox.Text = contents;
            HorizontalScrollPosition = curPos;
            if (_followTail)
                ScrollToEnd();
        }

        private static void OnExit(object sender, EventArgs e)
        {
            FormManager.TCPClient.Dispose();
        }

        private void SvcLog_CheckedChanged(object sender, EventArgs e)
        {
            ShowsSvcLog = SvcLog.Checked;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            _logger.AppendLine("Stopping log thread...");
            if (_logManager.StopLogThread())
            {
                _logger.AppendLine("Sending disconnect msg...");
                FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.Disconnect);
                _logger.AppendLine("Closing connection...");
                FormManager.TCPClient.CloseConnection();
                selectedServer = null;
                connectedHost = null;
            }
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            HostInfoLabel.Text = $"Connecting to host {(string)HostListBox.SelectedItem}...";
            Connect.Enabled = false;
            _connectTimeout = 0;
            _connectTimer.Interval = 100.0;
            _connectTimer.Start();
        }

        private void ServerSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (connectedHost != null)
            {
                foreach (ServerInfo server in connectedHost.GetServerList())
                {
                    if (ServerSelectBox.SelectedItem != null && ServerSelectBox.SelectedItem.ToString() == server.GetServerName())
                    {
                        selectedServer = server;
                        ServerInfoBox.Text = server.GetServerName();
                        ComponentEnableManager();
                    }
                }
            }
        }

        private void SingBackup_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.Backup);
            DisableUI();
        }

        private void EditCfg_Click(object sender, EventArgs e)
        {
            _editDialog = new PropEditorForm();
            _editDialog.PopulateBoxes(selectedServer.GetAllProps());
            if (_editDialog.ShowDialog() == DialogResult.OK)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PropUpdate);
                selectedServer.SetAllProps(_editDialog.workingProps);
                _editDialog.Close();
                _editDialog.Dispose();
                RestartSrv_Click(null, null);
            }
        }

        private void RestartSrv_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.Restart);
            DisableUI();
        }

        private void EditGlobals_Click(object sender, EventArgs e)
        {
            _editDialog = new PropEditorForm();
            _editDialog.PopulateBoxes(connectedHost.GetAllProps());
            if (_editDialog.ShowDialog() == DialogResult.OK)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_editDialog.workingProps, Formatting.Indented, settings));
                FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.PropUpdate);
                connectedHost.SetAllProps(_editDialog.workingProps);
                _editDialog.Close();
                _editDialog.Dispose();
                RestartSrv_Click(null, null);
            }
        }

        private void GlobBackup_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.BackupAll);
            DisableUI();
        }

        private void newSrvBtn_Click(object sender, EventArgs e)
        {
            if (clientSideServiceConfiguration == null)
            {
                clientSideServiceConfiguration = _configManager.HostConnectList.First(host => host.GetHostName() == HostListBox.Text);
            }
            AddNewServerForm newServerForm = new AddNewServerForm(clientSideServiceConfiguration, connectedHost.GetServerList());
            if (newServerForm.ShowDialog() == DialogResult.OK)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newServerForm.DefaultProps, Formatting.Indented, settings));
                FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.AddNewServer);
                newServerForm.Close();
                newServerForm.Dispose();
            }
        }

        private void RemoveSrvBtn_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.RemoveServer, NetworkMessageFlags.RemoveAll);
            DisableUI();
        }

        private void PlayerManager_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PlayersRequest);
            DisableUI();
            WaitForServerData().Wait();
            FormManager.TCPClient.PlayerInfoArrived = false;
            PlayerManagerForm form = new PlayerManagerForm(selectedServer);
            if(form.ShowDialog() != DialogResult.OK)
            {
                ServerBusy = false;
            }
        }

        private void Disconn_Click(object sender, EventArgs e)
        {
            _connectTimer.Stop();
            if (_logManager.StopLogThread())
            {
                try
                {
                    if (FormManager.TCPClient.Connected)
                    {
                        Thread.Sleep(500);
                        FormManager.TCPClient.CloseConnection();
                    }
                    selectedServer = null;
                    connectedHost = null;
                    LogBox.Invoke((MethodInvoker)delegate { LogBox.Text = ""; });
                    FormManager.MainWindow.Invoke((MethodInvoker)delegate
                    {
                        ComponentEnableManager();
                        ServerSelectBox.Items.Clear();
                        ServerSelectBox.SelectedIndex = -1;
                        ServerInfoBox.Text = "";
                        HostInfoLabel.Text = $"Select a host below:";
                    });
                }
                catch (Exception) { }

            }
        }

        private void SendCmd_Click(object sender, EventArgs e)
        {
            if (cmdTextBox.Text.Length > 0)
            {
                byte[] msg = Encoding.UTF8.GetBytes(cmdTextBox.Text);
                FormManager.TCPClient.SendData(msg, NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.Command);
            }
            cmdTextBox.Text = "";
        }

        private void EditStCmd_Click(object sender, EventArgs e)
        {
            PropEditorForm editSrvDialog = new PropEditorForm();
            editSrvDialog.PopulateStartCmds(selectedServer.GetStartCommands());
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(editSrvDialog.startCmds, Formatting.Indented, settings));
            if (editSrvDialog.ShowDialog() == DialogResult.OK)
            {
                DisableUI();
                FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.StartCmdUpdate);
                selectedServer.SetStartCommands(editSrvDialog.startCmds);
                editSrvDialog.Close();
                editSrvDialog.Dispose();
                RestartSrv_Click(null, null);
            }
        }

        private void ChkUpdates_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.CheckUpdates);
            DisableUI();
        }

        private int HorizontalScrollPosition
        {
            get
            {
                ScrollInfo si = new ScrollInfo();
                si.Size = (uint)Marshal.SizeOf(si);
                si.Mask = (uint)ScrollInfoMask.All;
                GetScrollInfo(LogBox.Handle, (int)ScrollBarDirection.Vertical, ref si);
                return si.Pos;
            }
            set
            {
                ScrollInfo si = new ScrollInfo();
                si.Size = (uint)Marshal.SizeOf(si);
                si.Mask = (uint)ScrollInfoMask.All;
                GetScrollInfo(LogBox.Handle, (int)ScrollBarDirection.Vertical, ref si);
                si.Pos = value;
                SetScrollInfo(LogBox.Handle, (int)ScrollBarDirection.Vertical, ref si, true);
                SendMessage(LogBox.Handle, VerticalScroll, new IntPtr(Thumbtrack + 0x10000 * si.Pos), new IntPtr(0));
            }
        }

        public void ScrollToEnd()
        {

            // Get the current scroll info.
            ScrollInfo si = new ScrollInfo();
            si.Size = (uint)Marshal.SizeOf(si);
            si.Mask = (uint)ScrollInfoMask.All;
            GetScrollInfo(LogBox.Handle, (int)ScrollBarDirection.Vertical, ref si);

            // Set the scroll position to maximum.
            si.Pos = si.Max - (int)si.Page;
            SetScrollInfo(LogBox.Handle, (int)ScrollBarDirection.Vertical, ref si, true);
            SendMessage(LogBox.Handle, VerticalScroll, new IntPtr(Thumbtrack + 0x10000 * si.Pos), new IntPtr(0));

            _followTail = true;
        }

        public void PerformBackupTests()
        {
            HostListBox.SelectedIndex = 0;
            Connect_Click(null, null);
            GlobBackup_Click(null, null);
            DisableUI();
            ServerSelectBox.SelectedIndex = 0;
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.EnumBackups);
            FormManager.TCPClient.EnumBackupsArrived = false;
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            byte[] serializeToBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new string[] { FormManager.TCPClient.BackupList[0].ToString() }, Formatting.Indented, settings));
            FormManager.TCPClient.SendData(serializeToBytes, NetworkMessageSource.Client, NetworkMessageDestination.Service, FormManager.MainWindow.connectedHost.GetServerIndex(FormManager.MainWindow.selectedServer), NetworkMessageTypes.DelBackups);
        }

        private void ComponentEnableManager()
        {
            Connect.Enabled = connectedHost == null;
            Disconn.Enabled = connectedHost != null;
            newSrvBtn.Enabled = connectedHost != null && !ServerBusy;
            ChkUpdates.Enabled = connectedHost != null && !ServerBusy;
            GlobBackup.Enabled = connectedHost != null && !ServerBusy;
            EditGlobals.Enabled = connectedHost != null && !ServerBusy;
            BackupManagerBtn.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            nbtStudioBtn.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            ManPacks.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            scrollLockChkBox.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            removeSrvBtn.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            EditCfg.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            PlayerManagerBtn.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            EditStCmd.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            SingBackup.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            RestartSrv.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            ServerInfoBox.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            SendCmd.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            cmdTextBox.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
            SvcLog.Enabled = (connectedHost != null && selectedServer != null && !ServerBusy);
        }

        public Task DisableUI()
        {
            ServerBusy = true;
            return Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate { ComponentEnableManager(); });
                while (ServerBusy)
                {
                    Task.Delay(250);
                }
                Invoke((MethodInvoker)delegate { ComponentEnableManager(); });
            });
        }

        public Task WaitForServerData()
        {
            return Task.Run(() =>
            {
                while (!FormManager.TCPClient.EnumBackupsArrived && !FormManager.TCPClient.PlayerInfoArrived && FormManager.TCPClient.RecievedPacks == null)
                {
                    Task.Delay(250);
                }
            });
        }

        public class ServerConnectException : Exception
        {
            public ServerConnectException() { }

            public ServerConnectException(string message)
                : base(message)
            {

            }

            public ServerConnectException(string message, Exception inner)
                : base(message, inner)
            {

            }
        }

        private void scrollLockChkBox_CheckedChanged(object sender, EventArgs e) => _followTail = scrollLockChkBox.Checked;

        private void cmdTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendCmd_Click(null, null);
            }
        }

        private void HostListBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                Connect_Click(null, null);
            }
        }

        private void BackupManager_Click(object sender, EventArgs e)
        {
            using (PropEditorForm editDialog = new PropEditorForm())
            {
                editDialog.EnableBackupManager();
                FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.EnumBackups);
                DisableUI();
                WaitForServerData().Wait();
                FormManager.TCPClient.EnumBackupsArrived = false;
                editDialog.PopulateBoxes(FormManager.TCPClient.BackupList);
                if (editDialog.ShowDialog() == DialogResult.OK)
                {
                    FormManager.TCPClient.SendData(Encoding.UTF8.GetBytes(editDialog.RollbackFolderName), NetworkMessageSource.Client, NetworkMessageDestination.Service, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.BackupRollback);
                    ServerBusy = true;
                }
                else
                {
                    ServerBusy = false;
                }
                editDialog.Close();
            }
        }

        private void ManPacks_Click(object sender, EventArgs e)
        {
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.PackList);
            DisableUI();
            WaitForServerData().Wait();
            using (ManagePacksForms form = new ManagePacksForms(connectedHost.GetServerIndex(selectedServer), _logger, _processInfo))
            {
                form.PopulateServerPacks(FormManager.TCPClient.RecievedPacks);
                if (form.ShowDialog() != DialogResult.OK)
                {
                    ServerBusy = false;
                }
                form.Close();
            }
            FormManager.TCPClient.RecievedPacks = null;
        }

        private void nbtStudioBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_configManager.NBTStudioPath))
                using (OpenFileDialog openFile = new OpenFileDialog())
                {
                    openFile.FileName = "NBTStudio.exe";
                    openFile.Title = "Please locate NBT Studio executable...";
                    openFile.Filter = "NBTStudio.exe|NBTStudio.exe";
                    if (openFile.ShowDialog() == DialogResult.OK)
                    {
                        _configManager.NBTStudioPath = openFile.FileName;
                        _configManager.SaveConfigFile();
                    }
                    else return;
                }
            ServerBusy = true;
            FormManager.TCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.LevelEditRequest);
            DisableUI();
            using (Process nbtStudioProcess = new Process())
            {
                string tempPath = $@"{Path.GetTempPath()}level.dat";
                nbtStudioProcess.StartInfo = new ProcessStartInfo(_configManager.NBTStudioPath, tempPath);
                nbtStudioProcess.Start();
                nbtStudioProcess.WaitForExit();
                FormManager.TCPClient.SendData(File.ReadAllBytes(tempPath), NetworkMessageSource.Client, NetworkMessageDestination.Server, connectedHost.GetServerIndex(selectedServer), NetworkMessageTypes.LevelEditFile);
            }
            ServerBusy = false;
        }

        private void HostListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (HostListBox.SelectedIndex != -1)
            {
                clientSideServiceConfiguration = _configManager.HostConnectList.FirstOrDefault(host => host.GetHostName() == (string)HostListBox.SelectedItem);
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void clientConfigBtn_Click(object sender, EventArgs e)
        {
            using (ClientConfigForm form = new ClientConfigForm(_configManager))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    form.Close();
                    InitForm();
                }
            }
        }
    }
}
