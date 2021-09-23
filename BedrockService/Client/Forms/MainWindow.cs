using BedrockService.Client.Management;
using BedrockService.Client.Utilities;
using BedrockService.Service.Networking;
using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class MainWindow : Form
    {
        public HostInfo connectedHost;
        public ServerInfo selectedServer;
        public int ConnectTimeout;
        public bool ShowsSvcLog = false;
        public bool FollowTail = false;
        public readonly int ConnectTimeoutLimit = 100;

        public MainWindow()
        {
            InitializeComponent();
            InitForm();
            SvcLog.CheckedChanged += SvcLog_CheckedChanged;
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

        const int VerticalScroll = 277;
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
        static void Main()
        {
            ConfigManager.LoadConfigs();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnExit;
            Application.Run(FormManager.GetMainWindow);
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
            LogManager.InitLogThread(connectedHost);
            HostInfoLabel.Text = $"Connected to host:";
            ServerSelectBox.Items.Clear();

            foreach (ServerInfo server in connectedHost.GetServerInfos())
            {
                ServerSelectBox.Items.Add(server.ServerName);
            }

            ServerSelectBox.Refresh();
            ServerSelectBox.SelectedIndex = 0;
            selectedServer = connectedHost.GetServerInfo((string)ServerSelectBox.SelectedItem);
            LogManager.StartLogThread();
            base.Refresh();
        }

        public void InitForm()
        {
            foreach (HostInfo host in ConfigManager.HostConnectList)
            {
                HostListBox.Items.Add(host.HostName);
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
            if (FollowTail)
                ScrollToEnd();
        }

        private static void OnExit(object sender, EventArgs e)
        {
            FormManager.GetTCPClient.Dispose();
        }

        private void SvcLog_CheckedChanged(object sender, EventArgs e)
        {
            ShowsSvcLog = SvcLog.Checked;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("Stopping log thread...");
            if (LogManager.StopLogThread())
            {
                Console.WriteLine("Sending disconnect msg...");
                if (FormManager.GetTCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.Disconnect))
                {
                    Console.WriteLine("Closing connection...");
                    FormManager.GetTCPClient.CloseConnection();
                    selectedServer = null;
                    connectedHost = null;
                }
            }
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if (HostListBox.SelectedIndex != -1)
            {
                try
                {
                    if (FormManager.GetTCPClient.ConnectHost(ConfigManager.HostConnectList.FirstOrDefault(host => host.HostName == (string)HostListBox.SelectedItem)))
                    {
                        while (connectedHost == null && FormManager.GetTCPClient.Connected)
                        {
                            Thread.Sleep(100);
                            ConnectTimeout++;
                            if (ConnectTimeout > ConnectTimeoutLimit)
                            {
                                FormManager.GetTCPClient.CloseConnection();
                                ConnectTimeout = 0;
                            }
                        }
                        ConnectTimeout = 0;
                        if (!FormManager.GetTCPClient.Connected)
                        {
                            HostInfoLabel.Text = $"Failed to connect to host!";
                            return;
                        }
                        RefreshServerContents();
                        ComponentEnableManager();
                    }
                    else
                    {
                        HostInfoLabel.Text = $"Failed to connect to host!";
                        return;
                    }
                }
                catch (ServerConnectException ex)
                {
                    HostInfoLabel.Text = ex.Message;
                }
            }
        }

        private void ServerSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (connectedHost != null)
            {
                foreach (ServerInfo server in connectedHost.GetServerInfos())
                {
                    if (ServerSelectBox.SelectedItem != null && ServerSelectBox.SelectedItem.ToString() == server.ServerName)
                    {
                        selectedServer = server;
                        ServerInfoBox.Text = server.ServerName;
                        LogBox.Text = selectedServer.ConsoleBuffer.ToString();
                        LogBox.Select(LogBox.Text.Length, 0);
                        ComponentEnableManager();
                    }
                }
            }
        }

        private void SingBackup_Click(object sender, EventArgs e)
        {
            byte[] serverName = Encoding.UTF8.GetBytes(selectedServer.ServerName);
            FormManager.GetTCPClient.SendData(serverName, NetworkMessageSource.Client, NetworkMessageDestination.Server, NetworkMessageTypes.Backup);
            Thread.Sleep(500);
        }

        private void EditCfg_Click(object sender, EventArgs e)
        {
            EditSrv editSrvDialog = new EditSrv();
            editSrvDialog.PopulateBoxes(selectedServer.ServerPropList);
            if (editSrvDialog.ShowDialog() == DialogResult.OK)
            {
                JsonUtilities.SendJsonMsg<List<Property>>(editSrvDialog.workingProps, NetworkMessageDestination.Service, NetworkMessageTypes.PropUpdate);
                selectedServer.ServerPropList = editSrvDialog.workingProps;
                editSrvDialog.Close();
                editSrvDialog.Dispose();
                RestartSrv_Click(null, null);
            }
        }

        private void RestartSrv_Click(object sender, EventArgs e)
        {
            byte[] serverName = Encoding.UTF8.GetBytes(selectedServer.ServerName);
            FormManager.GetTCPClient.SendData(serverName, NetworkMessageSource.Client, NetworkMessageDestination.Server, NetworkMessageTypes.Restart);
        }

        private void StopStartSvc_Click(object sender, EventArgs e)
        {

        }

        private void EditGlobals_Click(object sender, EventArgs e)
        {
            EditSrv editSrvDialog = new EditSrv();
            editSrvDialog.PopulateBoxes(connectedHost.GetGlobals());
            if (editSrvDialog.ShowDialog() == DialogResult.OK)
            {
                JsonUtilities.SendJsonMsg<List<Property>>(editSrvDialog.workingProps, NetworkMessageDestination.Service, NetworkMessageTypes.PropUpdate);
                connectedHost.SetGlobals(editSrvDialog.workingProps);
                editSrvDialog.Close();
                editSrvDialog.Dispose();
                RestartSrv_Click(null, null);
            }
        }

        private void GlobBackup_Click(object sender, EventArgs e)
        {
            FormManager.GetTCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.BackupAll);
        }

        private void newSrvBtn_Click(object sender, EventArgs e)
        {
            AddNewServerForm newServerForm = new AddNewServerForm();
            if(newServerForm.ShowDialog() == DialogResult.OK)
            {
                JsonUtilities.SendJsonMsg<List<Property>>(newServerForm.DefaultProps, NetworkMessageDestination.Service, NetworkMessageTypes.AddNewServer);
                newServerForm.Close();
                newServerForm.Dispose();
            }
        }

        private void removeSrvBtn_Click(object sender, EventArgs e)
        {
            byte[] serverName = Encoding.UTF8.GetBytes(selectedServer.ServerName);
            FormManager.GetTCPClient.SendData(serverName, NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.RemoveServer, NetworkMessageFlags.RemoveAll);
        }

        private void PlayerManager_Click(object sender, EventArgs e)
        {
            FormManager.GetTCPClient.SendData(Encoding.UTF8.GetBytes(selectedServer.ServerName), NetworkMessageSource.Client, NetworkMessageDestination.Server, NetworkMessageTypes.PlayersRequest);
            while (!FormManager.GetTCPClient.PlayerInfoArrived)
            {
                Thread.Sleep(100);
            }
            FormManager.GetTCPClient.PlayerInfoArrived = false;
            PlayerManagerForm form = new PlayerManagerForm(selectedServer);
            form.Show();
        }

        private void Disconn_Click(object sender, EventArgs e)
        {
            if (LogManager.StopLogThread())
            {
                try
                {
                    if (FormManager.GetTCPClient.Connected && FormManager.GetTCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.Disconnect))
                    {
                        Thread.Sleep(500);
                        FormManager.GetTCPClient.CloseConnection();
                    }
                    selectedServer = null;
                    connectedHost = null;
                    LogBox.Invoke((MethodInvoker)delegate { LogBox.Text = ""; });
                    FormManager.GetMainWindow.Invoke((MethodInvoker)delegate
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
                byte[] msg = Encoding.UTF8.GetBytes($"{selectedServer.ServerName};{cmdTextBox.Text}");
                FormManager.GetTCPClient.SendData(msg, NetworkMessageSource.Client, NetworkMessageDestination.Server, NetworkMessageTypes.Command);
            }
            cmdTextBox.Text = "";
        }

        private void EditStCmd_Click(object sender, EventArgs e)
        {
            EditSrv editSrvDialog = new EditSrv();
            editSrvDialog.PopulateStartCmds(selectedServer.StartCmds);
            if (editSrvDialog.ShowDialog() == DialogResult.OK)
            {
                JsonUtilities.SendJsonMsgToSrv<List<StartCmdEntry>>(selectedServer.ServerName, editSrvDialog.startCmds, NetworkMessageDestination.Service, NetworkMessageTypes.StartCmdUpdate);
                selectedServer.StartCmds = editSrvDialog.startCmds;
                editSrvDialog.Close();
                editSrvDialog.Dispose();
                RestartSrv_Click(null, null);
            }
        }

        private void ChkUpdates_Click(object sender, EventArgs e)
        {
            FormManager.GetTCPClient.SendData(NetworkMessageSource.Client, NetworkMessageDestination.Service, NetworkMessageTypes.CheckUpdates);
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

            FollowTail = true;
        }

        private void ComponentEnableManager()
        {
            Connect.Enabled = connectedHost == null;
            Disconn.Enabled = connectedHost != null;
            newSrvBtn.Enabled = connectedHost != null;
            ChkUpdates.Enabled = connectedHost != null;
            GlobBackup.Enabled = connectedHost != null;
            EditGlobals.Enabled = connectedHost != null;
            scrollLockChkBox.Enabled = (connectedHost != null && selectedServer != null);
            removeSrvBtn.Enabled = (connectedHost != null && selectedServer != null);
            EditCfg.Enabled = (connectedHost != null && selectedServer != null);
            PlayerManagerBtn.Enabled = (connectedHost != null && selectedServer != null);
            EditStCmd.Enabled = (connectedHost != null && selectedServer != null);
            SingBackup.Enabled = (connectedHost != null && selectedServer != null);
            RestartSrv.Enabled = (connectedHost != null && selectedServer != null);
            ServerInfoBox.Enabled = (connectedHost != null && selectedServer != null);
            SendCmd.Enabled = (connectedHost != null && selectedServer != null);
            cmdTextBox.Enabled = (connectedHost != null && selectedServer != null);
            SvcLog.Enabled = (connectedHost != null && selectedServer != null);
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

        private void scrollLockChkBox_CheckedChanged(object sender, EventArgs e) => FollowTail = scrollLockChkBox.Checked;

        private void cmdTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Enter)
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
    }
}
