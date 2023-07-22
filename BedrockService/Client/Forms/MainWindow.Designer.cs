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
            components = new System.ComponentModel.Container();
            Connect = new System.Windows.Forms.Button();
            HostInfoLabel = new System.Windows.Forms.Label();
            HostListBox = new System.Windows.Forms.ComboBox();
            ServerSelectBox = new System.Windows.Forms.ListBox();
            Disconn = new System.Windows.Forms.Button();
            removeSrvBtn = new System.Windows.Forms.Button();
            ChkUpdates = new System.Windows.Forms.Button();
            GlobBackup = new System.Windows.Forms.Button();
            EditCfg = new System.Windows.Forms.Button();
            PlayerManagerBtn = new System.Windows.Forms.Button();
            SingBackup = new System.Windows.Forms.Button();
            RestartSrv = new System.Windows.Forms.Button();
            BackupManagerBtn = new System.Windows.Forms.Button();
            ServerInfoBox = new System.Windows.Forms.TextBox();
            newSrvBtn = new System.Windows.Forms.Button();
            nbtStudioBtn = new System.Windows.Forms.Button();
            startStopBtn = new System.Windows.Forms.Button();
            clientPage = new System.Windows.Forms.TabPage();
            clientLogBox = new System.Windows.Forms.TextBox();
            servicePage = new System.Windows.Forms.TabPage();
            serviceTextbox = new System.Windows.Forms.TextBox();
            serverPage = new System.Windows.Forms.TabPage();
            LogBox = new System.Windows.Forms.TextBox();
            logPageControl = new System.Windows.Forms.TabControl();
            startStopBtnToolTip = new System.Windows.Forms.ToolTip(components);
            cmdTextBox = new System.Windows.Forms.TextBox();
            SendCmd = new System.Windows.Forms.Button();
            ManPacks = new System.Windows.Forms.Button();
            scrollLockChkBox = new System.Windows.Forms.CheckBox();
            clientConfigBtn = new System.Windows.Forms.Button();
            serverConfigBtnMenu = new System.Windows.Forms.ContextMenuStrip(components);
            serverPropMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            startCmdMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            servicePropMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editCoreServicePropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            serverPackageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            serviceConfigFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            serverExporterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asConfigOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importableBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importableBackupWithPacksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fullServerPackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            serviceConfigFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            clientPage.SuspendLayout();
            servicePage.SuspendLayout();
            serverPage.SuspendLayout();
            logPageControl.SuspendLayout();
            serverConfigBtnMenu.SuspendLayout();
            SuspendLayout();
            // 
            // Connect
            // 
            Connect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            Connect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            Connect.Location = new System.Drawing.Point(1099, 117);
            Connect.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            Connect.Name = "Connect";
            Connect.Size = new System.Drawing.Size(368, 62);
            Connect.TabIndex = 64;
            Connect.Text = "Connect";
            Connect.UseVisualStyleBackColor = true;
            Connect.Click += Connect_Click;
            // 
            // HostInfoLabel
            // 
            HostInfoLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            HostInfoLabel.AutoSize = true;
            HostInfoLabel.Location = new System.Drawing.Point(1099, 17);
            HostInfoLabel.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            HostInfoLabel.Name = "HostInfoLabel";
            HostInfoLabel.Size = new System.Drawing.Size(193, 32);
            HostInfoLabel.TabIndex = 65;
            HostInfoLabel.Text = "Host Connection";
            // 
            // HostListBox
            // 
            HostListBox.AllowDrop = true;
            HostListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            HostListBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            HostListBox.FormattingEnabled = true;
            HostListBox.Location = new System.Drawing.Point(1099, 55);
            HostListBox.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            HostListBox.Name = "HostListBox";
            HostListBox.Size = new System.Drawing.Size(491, 40);
            HostListBox.TabIndex = 66;
            HostListBox.SelectedIndexChanged += HostListBox_SelectedIndexChanged;
            HostListBox.KeyPress += HostListBox_KeyPress;
            // 
            // ServerSelectBox
            // 
            ServerSelectBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            ServerSelectBox.FormattingEnabled = true;
            ServerSelectBox.ItemHeight = 32;
            ServerSelectBox.Location = new System.Drawing.Point(1490, 201);
            ServerSelectBox.Margin = new System.Windows.Forms.Padding(13, 4, 6, 13);
            ServerSelectBox.Name = "ServerSelectBox";
            ServerSelectBox.Size = new System.Drawing.Size(364, 228);
            ServerSelectBox.TabIndex = 67;
            ServerSelectBox.SelectedIndexChanged += ServerSelectBox_SelectedIndexChanged;
            // 
            // Disconn
            // 
            Disconn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            Disconn.Enabled = false;
            Disconn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            Disconn.Location = new System.Drawing.Point(1490, 117);
            Disconn.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            Disconn.Name = "Disconn";
            Disconn.Size = new System.Drawing.Size(364, 62);
            Disconn.TabIndex = 68;
            Disconn.Text = "Disconnect";
            Disconn.UseVisualStyleBackColor = true;
            Disconn.Click += Disconn_Click;
            // 
            // removeSrvBtn
            // 
            removeSrvBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            removeSrvBtn.Enabled = false;
            removeSrvBtn.Location = new System.Drawing.Point(1486, 570);
            removeSrvBtn.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            removeSrvBtn.Name = "removeSrvBtn";
            removeSrvBtn.Size = new System.Drawing.Size(368, 55);
            removeSrvBtn.TabIndex = 70;
            removeSrvBtn.Text = "Remove Selected Server";
            removeSrvBtn.UseVisualStyleBackColor = true;
            removeSrvBtn.Click += RemoveSrvBtn_Click;
            // 
            // ChkUpdates
            // 
            ChkUpdates.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            ChkUpdates.Enabled = false;
            ChkUpdates.Location = new System.Drawing.Point(1485, 761);
            ChkUpdates.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            ChkUpdates.Name = "ChkUpdates";
            ChkUpdates.Size = new System.Drawing.Size(368, 55);
            ChkUpdates.TabIndex = 71;
            ChkUpdates.Text = "Check for Updates";
            ChkUpdates.UseVisualStyleBackColor = true;
            ChkUpdates.Click += ChkUpdates_Click;
            // 
            // GlobBackup
            // 
            GlobBackup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            GlobBackup.Enabled = false;
            GlobBackup.Location = new System.Drawing.Point(1103, 571);
            GlobBackup.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            GlobBackup.Name = "GlobBackup";
            GlobBackup.Size = new System.Drawing.Size(368, 54);
            GlobBackup.TabIndex = 72;
            GlobBackup.Text = "Backup All Servers";
            GlobBackup.UseVisualStyleBackColor = true;
            GlobBackup.Click += GlobBackup_Click;
            // 
            // EditCfg
            // 
            EditCfg.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            EditCfg.Enabled = false;
            EditCfg.Location = new System.Drawing.Point(1486, 635);
            EditCfg.Margin = new System.Windows.Forms.Padding(6, 4, 13, 4);
            EditCfg.Name = "EditCfg";
            EditCfg.Size = new System.Drawing.Size(368, 55);
            EditCfg.TabIndex = 75;
            EditCfg.Text = "Edit BDS/BMS Configs";
            EditCfg.UseVisualStyleBackColor = true;
            EditCfg.Click += EditCfg_Click;
            // 
            // PlayerManagerBtn
            // 
            PlayerManagerBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            PlayerManagerBtn.Enabled = false;
            PlayerManagerBtn.Location = new System.Drawing.Point(1103, 698);
            PlayerManagerBtn.Margin = new System.Windows.Forms.Padding(6, 4, 13, 4);
            PlayerManagerBtn.Name = "PlayerManagerBtn";
            PlayerManagerBtn.Size = new System.Drawing.Size(368, 55);
            PlayerManagerBtn.TabIndex = 76;
            PlayerManagerBtn.Text = "Player Manager";
            PlayerManagerBtn.UseVisualStyleBackColor = true;
            PlayerManagerBtn.Click += PlayerManager_Click;
            // 
            // SingBackup
            // 
            SingBackup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            SingBackup.Enabled = false;
            SingBackup.Location = new System.Drawing.Point(1103, 509);
            SingBackup.Margin = new System.Windows.Forms.Padding(6, 4, 13, 4);
            SingBackup.Name = "SingBackup";
            SingBackup.Size = new System.Drawing.Size(368, 55);
            SingBackup.TabIndex = 79;
            SingBackup.Text = "Backup Selected Server";
            SingBackup.UseVisualStyleBackColor = true;
            SingBackup.Click += SingBackup_Click;
            // 
            // RestartSrv
            // 
            RestartSrv.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            RestartSrv.Enabled = false;
            RestartSrv.Location = new System.Drawing.Point(1691, 447);
            RestartSrv.Margin = new System.Windows.Forms.Padding(6, 4, 13, 4);
            RestartSrv.Name = "RestartSrv";
            RestartSrv.Size = new System.Drawing.Size(163, 51);
            RestartSrv.TabIndex = 80;
            RestartSrv.Text = "Restart";
            RestartSrv.UseVisualStyleBackColor = true;
            RestartSrv.Click += RestartSrv_Click;
            // 
            // BackupManagerBtn
            // 
            BackupManagerBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            BackupManagerBtn.Enabled = false;
            BackupManagerBtn.Location = new System.Drawing.Point(1103, 635);
            BackupManagerBtn.Margin = new System.Windows.Forms.Padding(6, 4, 13, 4);
            BackupManagerBtn.Name = "BackupManagerBtn";
            BackupManagerBtn.Size = new System.Drawing.Size(368, 55);
            BackupManagerBtn.TabIndex = 81;
            BackupManagerBtn.Text = "Backup Manager";
            BackupManagerBtn.UseVisualStyleBackColor = true;
            BackupManagerBtn.Click += BackupManager_Click;
            // 
            // ServerInfoBox
            // 
            ServerInfoBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            ServerInfoBox.Location = new System.Drawing.Point(1103, 201);
            ServerInfoBox.Margin = new System.Windows.Forms.Padding(6, 4, 13, 13);
            ServerInfoBox.Multiline = true;
            ServerInfoBox.Name = "ServerInfoBox";
            ServerInfoBox.ReadOnly = true;
            ServerInfoBox.Size = new System.Drawing.Size(364, 228);
            ServerInfoBox.TabIndex = 82;
            // 
            // newSrvBtn
            // 
            newSrvBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            newSrvBtn.Enabled = false;
            newSrvBtn.Location = new System.Drawing.Point(1485, 509);
            newSrvBtn.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            newSrvBtn.Name = "newSrvBtn";
            newSrvBtn.Size = new System.Drawing.Size(368, 55);
            newSrvBtn.TabIndex = 83;
            newSrvBtn.Text = "Add New Server";
            newSrvBtn.UseVisualStyleBackColor = true;
            newSrvBtn.Click += newSrvBtn_Click;
            // 
            // nbtStudioBtn
            // 
            nbtStudioBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            nbtStudioBtn.Enabled = false;
            nbtStudioBtn.Location = new System.Drawing.Point(1485, 698);
            nbtStudioBtn.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            nbtStudioBtn.Name = "nbtStudioBtn";
            nbtStudioBtn.Size = new System.Drawing.Size(368, 55);
            nbtStudioBtn.TabIndex = 85;
            nbtStudioBtn.Text = "Edit World via NBTStudio";
            nbtStudioBtn.UseVisualStyleBackColor = true;
            nbtStudioBtn.Click += nbtStudioBtn_Click;
            // 
            // startStopBtn
            // 
            startStopBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            startStopBtn.Enabled = false;
            startStopBtn.Location = new System.Drawing.Point(1489, 449);
            startStopBtn.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            startStopBtn.Name = "startStopBtn";
            startStopBtn.Size = new System.Drawing.Size(163, 51);
            startStopBtn.TabIndex = 87;
            startStopBtn.Text = "Start/Stop";
            startStopBtn.UseVisualStyleBackColor = true;
            startStopBtn.Click += startStopBtn_Click;
            // 
            // clientPage
            // 
            clientPage.BackColor = System.Drawing.SystemColors.Control;
            clientPage.Controls.Add(clientLogBox);
            clientPage.Location = new System.Drawing.Point(8, 46);
            clientPage.Margin = new System.Windows.Forms.Padding(17, 28, 17, 28);
            clientPage.Name = "clientPage";
            clientPage.Size = new System.Drawing.Size(1044, 745);
            clientPage.TabIndex = 2;
            clientPage.Text = "Client Log";
            // 
            // clientLogBox
            // 
            clientLogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            clientLogBox.Location = new System.Drawing.Point(0, 0);
            clientLogBox.Margin = new System.Windows.Forms.Padding(0);
            clientLogBox.Multiline = true;
            clientLogBox.Name = "clientLogBox";
            clientLogBox.ReadOnly = true;
            clientLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            clientLogBox.Size = new System.Drawing.Size(1044, 745);
            clientLogBox.TabIndex = 6;
            clientLogBox.WordWrap = false;
            // 
            // servicePage
            // 
            servicePage.BackColor = System.Drawing.SystemColors.Control;
            servicePage.Controls.Add(serviceTextbox);
            servicePage.Location = new System.Drawing.Point(8, 46);
            servicePage.Margin = new System.Windows.Forms.Padding(17, 28, 17, 28);
            servicePage.Name = "servicePage";
            servicePage.Size = new System.Drawing.Size(1044, 745);
            servicePage.TabIndex = 1;
            servicePage.Text = "Service log";
            // 
            // serviceTextbox
            // 
            serviceTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            serviceTextbox.Location = new System.Drawing.Point(0, 0);
            serviceTextbox.Margin = new System.Windows.Forms.Padding(0);
            serviceTextbox.Multiline = true;
            serviceTextbox.Name = "serviceTextbox";
            serviceTextbox.ReadOnly = true;
            serviceTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            serviceTextbox.Size = new System.Drawing.Size(1044, 745);
            serviceTextbox.TabIndex = 5;
            serviceTextbox.WordWrap = false;
            // 
            // serverPage
            // 
            serverPage.BackColor = System.Drawing.SystemColors.Control;
            serverPage.Controls.Add(LogBox);
            serverPage.Location = new System.Drawing.Point(8, 46);
            serverPage.Margin = new System.Windows.Forms.Padding(17, 28, 17, 28);
            serverPage.Name = "serverPage";
            serverPage.Size = new System.Drawing.Size(1044, 745);
            serverPage.TabIndex = 0;
            serverPage.Text = "Server log";
            // 
            // LogBox
            // 
            LogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            LogBox.Location = new System.Drawing.Point(0, 0);
            LogBox.Margin = new System.Windows.Forms.Padding(0);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.ReadOnly = true;
            LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            LogBox.Size = new System.Drawing.Size(1044, 745);
            LogBox.TabIndex = 4;
            LogBox.WordWrap = false;
            // 
            // logPageControl
            // 
            logPageControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            logPageControl.Controls.Add(serverPage);
            logPageControl.Controls.Add(servicePage);
            logPageControl.Controls.Add(clientPage);
            logPageControl.Location = new System.Drawing.Point(16, 17);
            logPageControl.Margin = new System.Windows.Forms.Padding(7, 9, 7, 9);
            logPageControl.Name = "logPageControl";
            logPageControl.SelectedIndex = 0;
            logPageControl.Size = new System.Drawing.Size(1060, 799);
            logPageControl.TabIndex = 89;
            // 
            // cmdTextBox
            // 
            cmdTextBox.AcceptsTab = true;
            cmdTextBox.AllowDrop = true;
            cmdTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cmdTextBox.BackColor = System.Drawing.SystemColors.WindowText;
            cmdTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            cmdTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            cmdTextBox.Enabled = false;
            cmdTextBox.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cmdTextBox.ForeColor = System.Drawing.SystemColors.Window;
            cmdTextBox.Location = new System.Drawing.Point(15, 836);
            cmdTextBox.Margin = new System.Windows.Forms.Padding(6, 13, 6, 4);
            cmdTextBox.MinimumSize = new System.Drawing.Size(966, 44);
            cmdTextBox.Name = "cmdTextBox";
            cmdTextBox.Size = new System.Drawing.Size(966, 45);
            cmdTextBox.TabIndex = 73;
            cmdTextBox.KeyPress += cmdTextBox_KeyPress;
            cmdTextBox.PreviewKeyDown += cmdTextBox_PreviewKeyDown;
            // 
            // SendCmd
            // 
            SendCmd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            SendCmd.Enabled = false;
            SendCmd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            SendCmd.Location = new System.Drawing.Point(1000, 836);
            SendCmd.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            SendCmd.Name = "SendCmd";
            SendCmd.Size = new System.Drawing.Size(76, 44);
            SendCmd.TabIndex = 74;
            SendCmd.Text = "↲";
            SendCmd.UseVisualStyleBackColor = true;
            SendCmd.Click += SendCmd_Click;
            // 
            // ManPacks
            // 
            ManPacks.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            ManPacks.Enabled = false;
            ManPacks.Location = new System.Drawing.Point(1103, 761);
            ManPacks.Margin = new System.Windows.Forms.Padding(6, 4, 13, 4);
            ManPacks.Name = "ManPacks";
            ManPacks.Size = new System.Drawing.Size(368, 55);
            ManPacks.TabIndex = 78;
            ManPacks.Text = "R/B Pack Manager";
            ManPacks.UseVisualStyleBackColor = true;
            ManPacks.Click += ManPacks_Click;
            // 
            // scrollLockChkBox
            // 
            scrollLockChkBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            scrollLockChkBox.AutoSize = true;
            scrollLockChkBox.Enabled = false;
            scrollLockChkBox.Location = new System.Drawing.Point(1103, 840);
            scrollLockChkBox.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            scrollLockChkBox.Name = "scrollLockChkBox";
            scrollLockChkBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            scrollLockChkBox.Size = new System.Drawing.Size(353, 36);
            scrollLockChkBox.TabIndex = 84;
            scrollLockChkBox.Text = "Lock textbox scrollbar to end";
            scrollLockChkBox.UseVisualStyleBackColor = true;
            scrollLockChkBox.CheckedChanged += scrollLockChkBox_CheckedChanged;
            // 
            // clientConfigBtn
            // 
            clientConfigBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            clientConfigBtn.Location = new System.Drawing.Point(1610, 46);
            clientConfigBtn.Margin = new System.Windows.Forms.Padding(13, 4, 6, 4);
            clientConfigBtn.Name = "clientConfigBtn";
            clientConfigBtn.Size = new System.Drawing.Size(244, 49);
            clientConfigBtn.TabIndex = 86;
            clientConfigBtn.Text = "Edit client config";
            clientConfigBtn.UseVisualStyleBackColor = true;
            clientConfigBtn.Click += clientConfigBtn_Click;
            // 
            // serverConfigBtnMenu
            // 
            serverConfigBtnMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            serverConfigBtnMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { serverPropMenuItem, startCmdMenuItem, servicePropMenuItem, editCoreServicePropertiesToolStripMenuItem, importToolStripMenuItem, exportToolStripMenuItem });
            serverConfigBtnMenu.Name = "serverConfigBtnMenu";
            serverConfigBtnMenu.Size = new System.Drawing.Size(387, 232);
            // 
            // serverPropMenuItem
            // 
            serverPropMenuItem.Name = "serverPropMenuItem";
            serverPropMenuItem.Size = new System.Drawing.Size(386, 38);
            serverPropMenuItem.Text = "Edit server BDS properties";
            serverPropMenuItem.Click += serverPropMenuItem_Click;
            // 
            // startCmdMenuItem
            // 
            startCmdMenuItem.Name = "startCmdMenuItem";
            startCmdMenuItem.Size = new System.Drawing.Size(386, 38);
            startCmdMenuItem.Text = "Edit BDS Startup commands";
            startCmdMenuItem.Click += startCmdMenuItem_Click;
            // 
            // servicePropMenuItem
            // 
            servicePropMenuItem.Name = "servicePropMenuItem";
            servicePropMenuItem.Size = new System.Drawing.Size(386, 38);
            servicePropMenuItem.Text = "Edit BMS server properties";
            servicePropMenuItem.Click += servicePropMenuItem_Click;
            // 
            // editCoreServicePropertiesToolStripMenuItem
            // 
            editCoreServicePropertiesToolStripMenuItem.Name = "editCoreServicePropertiesToolStripMenuItem";
            editCoreServicePropertiesToolStripMenuItem.Size = new System.Drawing.Size(386, 38);
            editCoreServicePropertiesToolStripMenuItem.Text = "Edit BMS Service properties";
            editCoreServicePropertiesToolStripMenuItem.Click += editCoreServicePropertiesToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { serverPackageFileToolStripMenuItem, serviceConfigFileToolStripMenuItem });
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(386, 38);
            importToolStripMenuItem.Text = "Import...";
            // 
            // serverPackageFileToolStripMenuItem
            // 
            serverPackageFileToolStripMenuItem.Name = "serverPackageFileToolStripMenuItem";
            serverPackageFileToolStripMenuItem.Size = new System.Drawing.Size(349, 44);
            serverPackageFileToolStripMenuItem.Text = "Server package file";
            serverPackageFileToolStripMenuItem.Click += serverPackageFileToolStripMenuItem_Click;
            // 
            // serviceConfigFileToolStripMenuItem
            // 
            serviceConfigFileToolStripMenuItem.Name = "serviceConfigFileToolStripMenuItem";
            serviceConfigFileToolStripMenuItem.Size = new System.Drawing.Size(349, 44);
            serviceConfigFileToolStripMenuItem.Text = "Service config file";
            serviceConfigFileToolStripMenuItem.Click += serviceConfigFileToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { serverExporterToolStripMenuItem, serviceConfigFileToolStripMenuItem1 });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(386, 38);
            exportToolStripMenuItem.Text = "Export...";
            // 
            // serverExporterToolStripMenuItem
            // 
            serverExporterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asConfigOnlyToolStripMenuItem, importableBackupToolStripMenuItem, importableBackupWithPacksToolStripMenuItem, fullServerPackageToolStripMenuItem });
            serverExporterToolStripMenuItem.Name = "serverExporterToolStripMenuItem";
            serverExporterToolStripMenuItem.Size = new System.Drawing.Size(379, 44);
            serverExporterToolStripMenuItem.Text = "Server Exporter Menu";
            // 
            // asConfigOnlyToolStripMenuItem
            // 
            asConfigOnlyToolStripMenuItem.Name = "asConfigOnlyToolStripMenuItem";
            asConfigOnlyToolStripMenuItem.Size = new System.Drawing.Size(465, 44);
            asConfigOnlyToolStripMenuItem.Text = "Configuration file only";
            asConfigOnlyToolStripMenuItem.Click += asConfigOnlyToolStripMenuItem_Click;
            // 
            // importableBackupToolStripMenuItem
            // 
            importableBackupToolStripMenuItem.Name = "importableBackupToolStripMenuItem";
            importableBackupToolStripMenuItem.Size = new System.Drawing.Size(465, 44);
            importableBackupToolStripMenuItem.Text = "Importable backup";
            importableBackupToolStripMenuItem.Click += importableBackupToolStripMenuItem_Click;
            // 
            // importableBackupWithPacksToolStripMenuItem
            // 
            importableBackupWithPacksToolStripMenuItem.Name = "importableBackupWithPacksToolStripMenuItem";
            importableBackupWithPacksToolStripMenuItem.Size = new System.Drawing.Size(465, 44);
            importableBackupWithPacksToolStripMenuItem.Text = "Importable backup with packs";
            importableBackupWithPacksToolStripMenuItem.Click += importableBackupWithPacksToolStripMenuItem_Click;
            // 
            // fullServerPackageToolStripMenuItem
            // 
            fullServerPackageToolStripMenuItem.Name = "fullServerPackageToolStripMenuItem";
            fullServerPackageToolStripMenuItem.Size = new System.Drawing.Size(465, 44);
            fullServerPackageToolStripMenuItem.Text = "Full server package";
            fullServerPackageToolStripMenuItem.Click += fullServerPackageToolStripMenuItem_Click;
            // 
            // serviceConfigFileToolStripMenuItem1
            // 
            serviceConfigFileToolStripMenuItem1.Name = "serviceConfigFileToolStripMenuItem1";
            serviceConfigFileToolStripMenuItem1.Size = new System.Drawing.Size(379, 44);
            serviceConfigFileToolStripMenuItem1.Text = "Service config file";
            serviceConfigFileToolStripMenuItem1.Click += serviceConfigFileToolStripMenuItem1_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1868, 889);
            Controls.Add(logPageControl);
            Controls.Add(startStopBtn);
            Controls.Add(clientConfigBtn);
            Controls.Add(nbtStudioBtn);
            Controls.Add(scrollLockChkBox);
            Controls.Add(newSrvBtn);
            Controls.Add(ServerInfoBox);
            Controls.Add(BackupManagerBtn);
            Controls.Add(RestartSrv);
            Controls.Add(SingBackup);
            Controls.Add(ManPacks);
            Controls.Add(PlayerManagerBtn);
            Controls.Add(EditCfg);
            Controls.Add(SendCmd);
            Controls.Add(cmdTextBox);
            Controls.Add(GlobBackup);
            Controls.Add(ChkUpdates);
            Controls.Add(removeSrvBtn);
            Controls.Add(Disconn);
            Controls.Add(ServerSelectBox);
            Controls.Add(HostListBox);
            Controls.Add(HostInfoLabel);
            Controls.Add(Connect);
            Margin = new System.Windows.Forms.Padding(13, 21, 13, 21);
            MinimumSize = new System.Drawing.Size(1876, 960);
            Name = "MainWindow";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Bedrock Service Management";
            Load += MainWindow_Load;
            clientPage.ResumeLayout(false);
            clientPage.PerformLayout();
            servicePage.ResumeLayout(false);
            servicePage.PerformLayout();
            serverPage.ResumeLayout(false);
            serverPage.PerformLayout();
            logPageControl.ResumeLayout(false);
            serverConfigBtnMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Label HostInfoLabel;
        public System.Windows.Forms.ComboBox HostListBox;
        private System.Windows.Forms.ListBox ServerSelectBox;
        private System.Windows.Forms.Button Disconn;
        private System.Windows.Forms.Button removeSrvBtn;
        private System.Windows.Forms.Button ChkUpdates;
        private System.Windows.Forms.Button GlobBackup;
        private System.Windows.Forms.Button EditCfg;
        private System.Windows.Forms.Button PlayerManagerBtn;
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
        private System.Windows.Forms.ContextMenuStrip serverConfigBtnMenu;
        private System.Windows.Forms.ToolStripMenuItem serverPropMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startCmdMenuItem;
        private System.Windows.Forms.ToolStripMenuItem servicePropMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editCoreServicePropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serverPackageFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serviceConfigFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serviceConfigFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem serverExporterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asConfigOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importableBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importableBackupWithPacksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullServerPackageToolStripMenuItem;
    }
}

