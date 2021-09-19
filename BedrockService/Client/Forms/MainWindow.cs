using BedrockService.Client.Management;
using BedrockService.Client.Utilities;
using BedrockService.Service.Networking;
using BedrockService.Service.Server.HostInfoClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BedrockService.Client.Forms
{
    public partial class MainWindow : Form
    {
        public static HostInfo connectedHost;
        public static ServerInfo selectedServer;
        public static int ConnectTimeout;
        public static bool ShowsSvcLog = false;
        public static readonly int ConnectTimeoutLimit = 100;

        public MainWindow()
        {
            InitializeComponent();
            InitForm();
            SvcLog.CheckedChanged += SvcLog_CheckedChanged;
        }

        private void SvcLog_CheckedChanged(object sender, EventArgs e)
        {
            ShowsSvcLog = SvcLog.Checked;
        }

        [STAThread]
        static void Main()
        {
            ConfigManager.LoadConfigs();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(FormManager.GetMainWindow);
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
                        LogManager.InitLogThread(connectedHost);
                        HostInfoLabel.Text = $"Connected to host:";
                        foreach (ServerInfo server in connectedHost.GetServerInfos())
                        {
                            ServerSelectBox.Items.Add(server.ServerName);
                        }
                        ServerSelectBox.Refresh();
                        ServerSelectBox.SelectedIndex = 0;
                        selectedServer = connectedHost.GetServerInfo((string)ServerSelectBox.SelectedItem);
                        LogManager.StartLogThread();
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
        }

        private void ComponentEnableManager()
        {
            Connect.Enabled = connectedHost == null;
            Disconn.Enabled = connectedHost != null;
            //StopStartSvc.Enabled = connectedHost != null;
            //RestartSvc.Enabled = connectedHost != null;
            EditGlobals.Enabled = connectedHost != null;
            //EditService.Enabled = connectedHost != null;
            //ChkUpdates.Enabled = connectedHost != null;
            //GlobBackup.Enabled = connectedHost != null;
            EditCfg.Enabled = (connectedHost != null && selectedServer != null);
            PlayerManagerBtn.Enabled = (connectedHost != null && selectedServer != null);
            //EditStCmd.Enabled = (connectedHost != null && selectedServer != null);
            //ManPacks.Enabled = (connectedHost != null && selectedServer != null);
            SingBackup.Enabled = (connectedHost != null && selectedServer != null);
            //Rollbackup.Enabled = (connectedHost != null && selectedServer != null);
            RestartSrv.Enabled = (connectedHost != null && selectedServer != null);
            ServerInfoBox.Enabled = (connectedHost != null && selectedServer != null);
            SendCmd.Enabled = (connectedHost != null && selectedServer != null);
            cmdTextBox.Enabled = (connectedHost != null && selectedServer != null);
            SvcLog.Enabled = (connectedHost != null && selectedServer != null);
        }

        private class ServerConnectException : Exception
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
    }
}
