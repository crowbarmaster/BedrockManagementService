namespace BedrockService.Client.Forms {
    partial class MainWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.Connect = new System.Windows.Forms.Button();
            this.HostInfoLabel = new System.Windows.Forms.Label();
            this.HostListBox = new System.Windows.Forms.ComboBox();
            this.ServerSelectBox = new System.Windows.Forms.ListBox();
            this.Disconn = new System.Windows.Forms.Button();
            this.EditGlobals = new System.Windows.Forms.Button();
            this.removeSrvBtn = new System.Windows.Forms.Button();
            this.ChkUpdates = new System.Windows.Forms.Button();
            this.GlobBackup = new System.Windows.Forms.Button();
            this.EditCfg = new System.Windows.Forms.Button();
            this.PlayerManagerBtn = new System.Windows.Forms.Button();
            this.EditStCmd = new System.Windows.Forms.Button();
            this.SingBackup = new System.Windows.Forms.Button();
            this.RestartSrv = new System.Windows.Forms.Button();
            this.BackupManagerBtn = new System.Windows.Forms.Button();
            this.ServerInfoBox = new System.Windows.Forms.TextBox();
            this.newSrvBtn = new System.Windows.Forms.Button();
            this.nbtStudioBtn = new System.Windows.Forms.Button();
            this.startStopBtn = new System.Windows.Forms.Button();
            this.clientPage = new System.Windows.Forms.TabPage();
            this.clientLogBox = new System.Windows.Forms.TextBox();
            this.servicePage = new System.Windows.Forms.TabPage();
            this.serviceTextbox = new System.Windows.Forms.TextBox();
            this.serverPage = new System.Windows.Forms.TabPage();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.logPageControl = new System.Windows.Forms.TabControl();
            this.startStopBtnToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cmdTextBox = new System.Windows.Forms.TextBox();
            this.SendCmd = new System.Windows.Forms.Button();
            this.ManPacks = new System.Windows.Forms.Button();
            this.scrollLockChkBox = new System.Windows.Forms.CheckBox();
            this.clientConfigBtn = new System.Windows.Forms.Button();
            this.expertOptionsBtn = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.clientPage.SuspendLayout();
            this.servicePage.SuspendLayout();
            this.serverPage.SuspendLayout();
            this.logPageControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // Connect
            // 
            this.Connect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Connect.Location = new System.Drawing.Point(592, 55);
            this.Connect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(198, 29);
            this.Connect.TabIndex = 64;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // HostInfoLabel
            // 
            this.HostInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HostInfoLabel.AutoSize = true;
            this.HostInfoLabel.Location = new System.Drawing.Point(592, 8);
            this.HostInfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.HostInfoLabel.Name = "HostInfoLabel";
            this.HostInfoLabel.Size = new System.Drawing.Size(98, 15);
            this.HostInfoLabel.TabIndex = 65;
            this.HostInfoLabel.Text = "HostConnectInfo";
            // 
            // HostListBox
            // 
            this.HostListBox.AllowDrop = true;
            this.HostListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HostListBox.FormattingEnabled = true;
            this.HostListBox.Location = new System.Drawing.Point(592, 26);
            this.HostListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.HostListBox.Name = "HostListBox";
            this.HostListBox.Size = new System.Drawing.Size(403, 23);
            this.HostListBox.TabIndex = 66;
            this.HostListBox.SelectedIndexChanged += new System.EventHandler(this.HostListBox_SelectedIndexChanged);
            this.HostListBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HostListBox_KeyPress);
            // 
            // ServerSelectBox
            // 
            this.ServerSelectBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerSelectBox.FormattingEnabled = true;
            this.ServerSelectBox.ItemHeight = 15;
            this.ServerSelectBox.Location = new System.Drawing.Point(798, 89);
            this.ServerSelectBox.Margin = new System.Windows.Forms.Padding(7, 2, 3, 6);
            this.ServerSelectBox.Name = "ServerSelectBox";
            this.ServerSelectBox.Size = new System.Drawing.Size(198, 124);
            this.ServerSelectBox.TabIndex = 67;
            this.ServerSelectBox.SelectedIndexChanged += new System.EventHandler(this.ServerSelectBox_SelectedIndexChanged);
            // 
            // Disconn
            // 
            this.Disconn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Disconn.Enabled = false;
            this.Disconn.Location = new System.Drawing.Point(800, 55);
            this.Disconn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Disconn.Name = "Disconn";
            this.Disconn.Size = new System.Drawing.Size(196, 29);
            this.Disconn.TabIndex = 68;
            this.Disconn.Text = "Disconnect";
            this.Disconn.UseVisualStyleBackColor = true;
            this.Disconn.Click += new System.EventHandler(this.Disconn_Click);
            // 
            // EditGlobals
            // 
            this.EditGlobals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.EditGlobals.Enabled = false;
            this.EditGlobals.Location = new System.Drawing.Point(798, 283);
            this.EditGlobals.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.EditGlobals.Name = "EditGlobals";
            this.EditGlobals.Size = new System.Drawing.Size(198, 26);
            this.EditGlobals.TabIndex = 69;
            this.EditGlobals.Text = "Edit global service settings";
            this.EditGlobals.UseVisualStyleBackColor = true;
            this.EditGlobals.Click += new System.EventHandler(this.EditGlobals_Click);
            // 
            // removeSrvBtn
            // 
            this.removeSrvBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSrvBtn.Enabled = false;
            this.removeSrvBtn.Location = new System.Drawing.Point(798, 369);
            this.removeSrvBtn.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.removeSrvBtn.Name = "removeSrvBtn";
            this.removeSrvBtn.Size = new System.Drawing.Size(198, 26);
            this.removeSrvBtn.TabIndex = 70;
            this.removeSrvBtn.Text = "Remove selected server";
            this.removeSrvBtn.UseVisualStyleBackColor = true;
            this.removeSrvBtn.Click += new System.EventHandler(this.RemoveSrvBtn_Click);
            // 
            // ChkUpdates
            // 
            this.ChkUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkUpdates.Enabled = false;
            this.ChkUpdates.Location = new System.Drawing.Point(798, 311);
            this.ChkUpdates.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.ChkUpdates.Name = "ChkUpdates";
            this.ChkUpdates.Size = new System.Drawing.Size(198, 26);
            this.ChkUpdates.TabIndex = 71;
            this.ChkUpdates.Text = "Check for updates";
            this.ChkUpdates.UseVisualStyleBackColor = true;
            this.ChkUpdates.Click += new System.EventHandler(this.ChkUpdates_Click);
            // 
            // GlobBackup
            // 
            this.GlobBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobBackup.Enabled = false;
            this.GlobBackup.Location = new System.Drawing.Point(798, 253);
            this.GlobBackup.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.GlobBackup.Name = "GlobBackup";
            this.GlobBackup.Size = new System.Drawing.Size(198, 26);
            this.GlobBackup.TabIndex = 72;
            this.GlobBackup.Text = "Backup all servers";
            this.GlobBackup.UseVisualStyleBackColor = true;
            this.GlobBackup.Click += new System.EventHandler(this.GlobBackup_Click);
            // 
            // EditCfg
            // 
            this.EditCfg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.EditCfg.Enabled = false;
            this.EditCfg.Location = new System.Drawing.Point(592, 223);
            this.EditCfg.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.EditCfg.Name = "EditCfg";
            this.EditCfg.Size = new System.Drawing.Size(198, 26);
            this.EditCfg.TabIndex = 75;
            this.EditCfg.Text = "Edit server config";
            this.EditCfg.UseVisualStyleBackColor = true;
            this.EditCfg.Click += new System.EventHandler(this.EditCfg_Click);
            // 
            // PlayerManagerBtn
            // 
            this.PlayerManagerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayerManagerBtn.Enabled = false;
            this.PlayerManagerBtn.Location = new System.Drawing.Point(592, 371);
            this.PlayerManagerBtn.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.PlayerManagerBtn.Name = "PlayerManagerBtn";
            this.PlayerManagerBtn.Size = new System.Drawing.Size(198, 26);
            this.PlayerManagerBtn.TabIndex = 76;
            this.PlayerManagerBtn.Text = "Player Manager";
            this.PlayerManagerBtn.UseVisualStyleBackColor = true;
            this.PlayerManagerBtn.Click += new System.EventHandler(this.PlayerManager_Click);
            // 
            // EditStCmd
            // 
            this.EditStCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.EditStCmd.Enabled = false;
            this.EditStCmd.Location = new System.Drawing.Point(592, 253);
            this.EditStCmd.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.EditStCmd.Name = "EditStCmd";
            this.EditStCmd.Size = new System.Drawing.Size(198, 26);
            this.EditStCmd.TabIndex = 77;
            this.EditStCmd.Text = "Edit start commands";
            this.EditStCmd.UseVisualStyleBackColor = true;
            this.EditStCmd.Click += new System.EventHandler(this.EditStCmd_Click);
            // 
            // SingBackup
            // 
            this.SingBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SingBackup.Enabled = false;
            this.SingBackup.Location = new System.Drawing.Point(592, 283);
            this.SingBackup.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.SingBackup.Name = "SingBackup";
            this.SingBackup.Size = new System.Drawing.Size(198, 26);
            this.SingBackup.TabIndex = 79;
            this.SingBackup.Text = "Backup selected server";
            this.SingBackup.UseVisualStyleBackColor = true;
            this.SingBackup.Click += new System.EventHandler(this.SingBackup_Click);
            // 
            // RestartSrv
            // 
            this.RestartSrv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.RestartSrv.Enabled = false;
            this.RestartSrv.Location = new System.Drawing.Point(702, 313);
            this.RestartSrv.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.RestartSrv.Name = "RestartSrv";
            this.RestartSrv.Size = new System.Drawing.Size(88, 24);
            this.RestartSrv.TabIndex = 80;
            this.RestartSrv.Text = "Restart";
            this.RestartSrv.UseVisualStyleBackColor = true;
            this.RestartSrv.Click += new System.EventHandler(this.RestartSrv_Click);
            // 
            // BackupManagerBtn
            // 
            this.BackupManagerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BackupManagerBtn.Enabled = false;
            this.BackupManagerBtn.Location = new System.Drawing.Point(592, 341);
            this.BackupManagerBtn.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.BackupManagerBtn.Name = "BackupManagerBtn";
            this.BackupManagerBtn.Size = new System.Drawing.Size(198, 26);
            this.BackupManagerBtn.TabIndex = 81;
            this.BackupManagerBtn.Text = "Backup Manager";
            this.BackupManagerBtn.UseVisualStyleBackColor = true;
            this.BackupManagerBtn.Click += new System.EventHandler(this.BackupManager_Click);
            // 
            // ServerInfoBox
            // 
            this.ServerInfoBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerInfoBox.Location = new System.Drawing.Point(592, 89);
            this.ServerInfoBox.Margin = new System.Windows.Forms.Padding(3, 2, 7, 6);
            this.ServerInfoBox.Multiline = true;
            this.ServerInfoBox.Name = "ServerInfoBox";
            this.ServerInfoBox.ReadOnly = true;
            this.ServerInfoBox.Size = new System.Drawing.Size(198, 124);
            this.ServerInfoBox.TabIndex = 82;
            // 
            // newSrvBtn
            // 
            this.newSrvBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.newSrvBtn.Enabled = false;
            this.newSrvBtn.Location = new System.Drawing.Point(798, 223);
            this.newSrvBtn.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.newSrvBtn.Name = "newSrvBtn";
            this.newSrvBtn.Size = new System.Drawing.Size(198, 26);
            this.newSrvBtn.TabIndex = 83;
            this.newSrvBtn.Text = "Deploy new server";
            this.newSrvBtn.UseVisualStyleBackColor = true;
            this.newSrvBtn.Click += new System.EventHandler(this.newSrvBtn_Click);
            // 
            // nbtStudioBtn
            // 
            this.nbtStudioBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nbtStudioBtn.Enabled = false;
            this.nbtStudioBtn.Location = new System.Drawing.Point(798, 340);
            this.nbtStudioBtn.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.nbtStudioBtn.Name = "nbtStudioBtn";
            this.nbtStudioBtn.Size = new System.Drawing.Size(198, 26);
            this.nbtStudioBtn.TabIndex = 85;
            this.nbtStudioBtn.Text = "Edit world via NBTStudio";
            this.nbtStudioBtn.UseVisualStyleBackColor = true;
            this.nbtStudioBtn.Click += new System.EventHandler(this.nbtStudioBtn_Click);
            // 
            // startStopBtn
            // 
            this.startStopBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.startStopBtn.Enabled = false;
            this.startStopBtn.Location = new System.Drawing.Point(592, 313);
            this.startStopBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.startStopBtn.Name = "startStopBtn";
            this.startStopBtn.Size = new System.Drawing.Size(88, 24);
            this.startStopBtn.TabIndex = 87;
            this.startStopBtn.Text = "Start/Stop";
            this.startStopBtn.UseVisualStyleBackColor = true;
            this.startStopBtn.Click += new System.EventHandler(this.startStopBtn_Click);
            // 
            // clientPage
            // 
            this.clientPage.BackColor = System.Drawing.SystemColors.Control;
            this.clientPage.Controls.Add(this.clientLogBox);
            this.clientPage.Location = new System.Drawing.Point(4, 24);
            this.clientPage.Margin = new System.Windows.Forms.Padding(9, 13, 9, 13);
            this.clientPage.Name = "clientPage";
            this.clientPage.Size = new System.Drawing.Size(563, 476);
            this.clientPage.TabIndex = 2;
            this.clientPage.Text = "Client Log";
            // 
            // clientLogBox
            // 
            this.clientLogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clientLogBox.Location = new System.Drawing.Point(0, 0);
            this.clientLogBox.Margin = new System.Windows.Forms.Padding(0);
            this.clientLogBox.Multiline = true;
            this.clientLogBox.Name = "clientLogBox";
            this.clientLogBox.ReadOnly = true;
            this.clientLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.clientLogBox.Size = new System.Drawing.Size(563, 476);
            this.clientLogBox.TabIndex = 6;
            this.clientLogBox.WordWrap = false;
            // 
            // servicePage
            // 
            this.servicePage.BackColor = System.Drawing.SystemColors.Control;
            this.servicePage.Controls.Add(this.serviceTextbox);
            this.servicePage.Location = new System.Drawing.Point(4, 24);
            this.servicePage.Margin = new System.Windows.Forms.Padding(9, 13, 9, 13);
            this.servicePage.Name = "servicePage";
            this.servicePage.Padding = new System.Windows.Forms.Padding(9, 13, 9, 13);
            this.servicePage.Size = new System.Drawing.Size(563, 476);
            this.servicePage.TabIndex = 1;
            this.servicePage.Text = "Service log";
            // 
            // serviceTextbox
            // 
            this.serviceTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serviceTextbox.Location = new System.Drawing.Point(0, 0);
            this.serviceTextbox.Margin = new System.Windows.Forms.Padding(0);
            this.serviceTextbox.Multiline = true;
            this.serviceTextbox.Name = "serviceTextbox";
            this.serviceTextbox.ReadOnly = true;
            this.serviceTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.serviceTextbox.Size = new System.Drawing.Size(563, 476);
            this.serviceTextbox.TabIndex = 5;
            this.serviceTextbox.WordWrap = false;
            // 
            // serverPage
            // 
            this.serverPage.BackColor = System.Drawing.SystemColors.Control;
            this.serverPage.Controls.Add(this.LogBox);
            this.serverPage.Location = new System.Drawing.Point(4, 24);
            this.serverPage.Margin = new System.Windows.Forms.Padding(9, 13, 9, 13);
            this.serverPage.Name = "serverPage";
            this.serverPage.Padding = new System.Windows.Forms.Padding(9, 13, 9, 13);
            this.serverPage.Size = new System.Drawing.Size(563, 476);
            this.serverPage.TabIndex = 0;
            this.serverPage.Text = "Server log";
            // 
            // LogBox
            // 
            this.LogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogBox.Location = new System.Drawing.Point(0, 0);
            this.LogBox.Margin = new System.Windows.Forms.Padding(0);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogBox.Size = new System.Drawing.Size(563, 476);
            this.LogBox.TabIndex = 4;
            this.LogBox.WordWrap = false;
            // 
            // logPageControl
            // 
            this.logPageControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logPageControl.Controls.Add(this.serverPage);
            this.logPageControl.Controls.Add(this.servicePage);
            this.logPageControl.Controls.Add(this.clientPage);
            this.logPageControl.Location = new System.Drawing.Point(13, 7);
            this.logPageControl.Margin = new System.Windows.Forms.Padding(4);
            this.logPageControl.Name = "logPageControl";
            this.logPageControl.SelectedIndex = 0;
            this.logPageControl.Size = new System.Drawing.Size(571, 504);
            this.logPageControl.TabIndex = 89;
            // 
            // cmdTextBox
            // 
            this.cmdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdTextBox.Enabled = false;
            this.cmdTextBox.Location = new System.Drawing.Point(593, 487);
            this.cmdTextBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 2);
            this.cmdTextBox.Name = "cmdTextBox";
            this.cmdTextBox.Size = new System.Drawing.Size(197, 23);
            this.cmdTextBox.TabIndex = 73;
            this.cmdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmdTextBox_KeyPress);
            // 
            // SendCmd
            // 
            this.SendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendCmd.Enabled = false;
            this.SendCmd.Location = new System.Drawing.Point(798, 485);
            this.SendCmd.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.SendCmd.Name = "SendCmd";
            this.SendCmd.Size = new System.Drawing.Size(198, 26);
            this.SendCmd.TabIndex = 74;
            this.SendCmd.Text = "Send command to server";
            this.SendCmd.UseVisualStyleBackColor = true;
            this.SendCmd.Click += new System.EventHandler(this.SendCmd_Click);
            // 
            // ManPacks
            // 
            this.ManPacks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ManPacks.Enabled = false;
            this.ManPacks.Location = new System.Drawing.Point(592, 400);
            this.ManPacks.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.ManPacks.Name = "ManPacks";
            this.ManPacks.Size = new System.Drawing.Size(198, 26);
            this.ManPacks.TabIndex = 78;
            this.ManPacks.Text = "R/B Pack Manager";
            this.ManPacks.UseVisualStyleBackColor = true;
            this.ManPacks.Click += new System.EventHandler(this.ManPacks_Click);
            // 
            // scrollLockChkBox
            // 
            this.scrollLockChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollLockChkBox.Enabled = false;
            this.scrollLockChkBox.Location = new System.Drawing.Point(594, 459);
            this.scrollLockChkBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.scrollLockChkBox.Name = "scrollLockChkBox";
            this.scrollLockChkBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.scrollLockChkBox.Size = new System.Drawing.Size(196, 24);
            this.scrollLockChkBox.TabIndex = 84;
            this.scrollLockChkBox.Text = "Lock textbox scrollbar to end";
            this.scrollLockChkBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.scrollLockChkBox.UseVisualStyleBackColor = true;
            this.scrollLockChkBox.CheckedChanged += new System.EventHandler(this.scrollLockChkBox_CheckedChanged);
            // 
            // clientConfigBtn
            // 
            this.clientConfigBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.clientConfigBtn.Location = new System.Drawing.Point(798, 430);
            this.clientConfigBtn.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.clientConfigBtn.Name = "clientConfigBtn";
            this.clientConfigBtn.Size = new System.Drawing.Size(198, 26);
            this.clientConfigBtn.TabIndex = 86;
            this.clientConfigBtn.Text = "Edit client config";
            this.clientConfigBtn.UseVisualStyleBackColor = true;
            this.clientConfigBtn.Click += new System.EventHandler(this.clientConfigBtn_Click);
            // 
            // expertOptionsBtn
            // 
            this.expertOptionsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.expertOptionsBtn.Enabled = false;
            this.expertOptionsBtn.Location = new System.Drawing.Point(592, 429);
            this.expertOptionsBtn.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.expertOptionsBtn.Name = "expertOptionsBtn";
            this.expertOptionsBtn.Size = new System.Drawing.Size(198, 26);
            this.expertOptionsBtn.TabIndex = 88;
            this.expertOptionsBtn.Text = "Expert server options";
            this.expertOptionsBtn.UseVisualStyleBackColor = true;
            this.expertOptionsBtn.Click += new System.EventHandler(this.expertOptionsBtn_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(798, 400);
            this.button3.Margin = new System.Windows.Forms.Padding(7, 2, 3, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(198, 26);
            this.button3.TabIndex = 91;
            this.button3.Text = "Reserved";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 524);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.logPageControl);
            this.Controls.Add(this.expertOptionsBtn);
            this.Controls.Add(this.startStopBtn);
            this.Controls.Add(this.clientConfigBtn);
            this.Controls.Add(this.nbtStudioBtn);
            this.Controls.Add(this.scrollLockChkBox);
            this.Controls.Add(this.newSrvBtn);
            this.Controls.Add(this.ServerInfoBox);
            this.Controls.Add(this.BackupManagerBtn);
            this.Controls.Add(this.RestartSrv);
            this.Controls.Add(this.SingBackup);
            this.Controls.Add(this.ManPacks);
            this.Controls.Add(this.EditStCmd);
            this.Controls.Add(this.PlayerManagerBtn);
            this.Controls.Add(this.EditCfg);
            this.Controls.Add(this.SendCmd);
            this.Controls.Add(this.cmdTextBox);
            this.Controls.Add(this.GlobBackup);
            this.Controls.Add(this.ChkUpdates);
            this.Controls.Add(this.removeSrvBtn);
            this.Controls.Add(this.EditGlobals);
            this.Controls.Add(this.Disconn);
            this.Controls.Add(this.ServerSelectBox);
            this.Controls.Add(this.HostListBox);
            this.Controls.Add(this.HostInfoLabel);
            this.Controls.Add(this.Connect);
            this.Margin = new System.Windows.Forms.Padding(7, 10, 7, 10);
            this.MinimumSize = new System.Drawing.Size(915, 457);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bedrock Service Management";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.clientPage.ResumeLayout(false);
            this.clientPage.PerformLayout();
            this.servicePage.ResumeLayout(false);
            this.servicePage.PerformLayout();
            this.serverPage.ResumeLayout(false);
            this.serverPage.PerformLayout();
            this.logPageControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Label HostInfoLabel;
        public System.Windows.Forms.ComboBox HostListBox;
        private System.Windows.Forms.ListBox ServerSelectBox;
        private System.Windows.Forms.Button Disconn;
        private System.Windows.Forms.Button EditGlobals;
        private System.Windows.Forms.Button removeSrvBtn;
        private System.Windows.Forms.Button ChkUpdates;
        private System.Windows.Forms.Button GlobBackup;
        private System.Windows.Forms.Button EditCfg;
        private System.Windows.Forms.Button PlayerManagerBtn;
        private System.Windows.Forms.Button EditStCmd;
        private System.Windows.Forms.Button SingBackup;
        private System.Windows.Forms.Button RestartSrv;
        private System.Windows.Forms.Button BackupManagerBtn;
        public System.Windows.Forms.TextBox ServerInfoBox;
        private System.Windows.Forms.Button newSrvBtn;
        private System.Windows.Forms.Button nbtStudioBtn;
        private System.Windows.Forms.Button startStopBtn;
        private System.Windows.Forms.TabPage clientPage;
        public System.Windows.Forms.TextBox clientLogBox;
        private System.Windows.Forms.TabPage servicePage;
        public System.Windows.Forms.TextBox serviceTextbox;
        private System.Windows.Forms.TabPage serverPage;
        public System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.TabControl logPageControl;
        private System.Windows.Forms.ToolTip startStopBtnToolTip;
        private System.Windows.Forms.TextBox cmdTextBox;
        private System.Windows.Forms.Button SendCmd;
        private System.Windows.Forms.Button ManPacks;
        private System.Windows.Forms.CheckBox scrollLockChkBox;
        private System.Windows.Forms.Button clientConfigBtn;
        private System.Windows.Forms.Button expertOptionsBtn;
        private System.Windows.Forms.Button button3;
    }
}

