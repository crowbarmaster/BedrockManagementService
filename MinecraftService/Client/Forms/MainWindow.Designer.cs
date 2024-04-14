
using System.Windows.Forms;

namespace MinecraftService.Client.Forms {
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
            Connect = new Button();
            HostInfoLabel = new Label();
            HostListBox = new ComboBox();
            ServerSelectBox = new ListBox();
            Disconn = new Button();
            removeSrvBtn = new Button();
            ChkUpdates = new Button();
            GlobBackup = new Button();
            EditCfg = new Button();
            PlayerManagerBtn = new Button();
            SingBackup = new Button();
            RestartSrv = new Button();
            BackupManagerBtn = new Button();
            ServerInfoBox = new TextBox();
            newSrvBtn = new Button();
            nbtStudioBtn = new Button();
            startStopBtn = new Button();
            clientPage = new TabPage();
            clientLogBox = new TextBox();
            servicePage = new TabPage();
            serviceTextbox = new TextBox();
            serverPage = new TabPage();
            LogBox = new TextBox();
            logPageControl = new TabControl();
            startStopBtnToolTip = new ToolTip(components);
            cmdTextBox = new RichTextBox();
            SendCmd = new Button();
            ManPacks = new Button();
            scrollLockChkBox = new CheckBox();
            clientConfigBtn = new Button();
            serverConfigBtnMenu = new ContextMenuStrip(components);
            serverPropMenuItem = new ToolStripMenuItem();
            startCmdMenuItem = new ToolStripMenuItem();
            servicePropMenuItem = new ToolStripMenuItem();
            editCoreServicePropertiesToolStripMenuItem = new ToolStripMenuItem();
            importToolStripMenuItem = new ToolStripMenuItem();
            serverPackageFileToolStripMenuItem = new ToolStripMenuItem();
            serviceConfigFileToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            serverExporterToolStripMenuItem = new ToolStripMenuItem();
            asConfigOnlyToolStripMenuItem = new ToolStripMenuItem();
            importableBackupToolStripMenuItem = new ToolStripMenuItem();
            importableBackupWithPacksToolStripMenuItem = new ToolStripMenuItem();
            fullServerPackageToolStripMenuItem = new ToolStripMenuItem();
            serviceConfigFileToolStripMenuItem1 = new ToolStripMenuItem();
            clientPage.SuspendLayout();
            servicePage.SuspendLayout();
            serverPage.SuspendLayout();
            logPageControl.SuspendLayout();
            serverConfigBtnMenu.SuspendLayout();
            SuspendLayout();
            // 
            // Connect
            // 
            Connect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Connect.Location = new System.Drawing.Point(592, 55);
            Connect.Margin = new Padding(4, 3, 4, 3);
            Connect.Name = "Connect";
            Connect.Size = new System.Drawing.Size(198, 29);
            Connect.TabIndex = 64;
            Connect.Text = "Connect";
            Connect.UseVisualStyleBackColor = true;
            Connect.Click += Connect_Click;
            // 
            // HostInfoLabel
            // 
            HostInfoLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HostInfoLabel.AutoSize = true;
            HostInfoLabel.Location = new System.Drawing.Point(592, 8);
            HostInfoLabel.Margin = new Padding(4, 0, 4, 0);
            HostInfoLabel.Name = "HostInfoLabel";
            HostInfoLabel.Size = new System.Drawing.Size(97, 15);
            HostInfoLabel.TabIndex = 65;
            HostInfoLabel.Text = "Host Connection";
            // 
            // HostListBox
            // 
            HostListBox.AllowDrop = true;
            HostListBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HostListBox.FlatStyle = FlatStyle.System;
            HostListBox.FormattingEnabled = true;
            HostListBox.Location = new System.Drawing.Point(592, 26);
            HostListBox.Margin = new Padding(4, 3, 4, 3);
            HostListBox.Name = "HostListBox";
            HostListBox.Size = new System.Drawing.Size(406, 23);
            HostListBox.TabIndex = 66;
            HostListBox.SelectedIndexChanged += HostListBox_SelectedIndexChanged;
            HostListBox.KeyPress += HostListBox_KeyPress;
            // 
            // ServerSelectBox
            // 
            ServerSelectBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ServerSelectBox.FormattingEnabled = true;
            ServerSelectBox.ItemHeight = 15;
            ServerSelectBox.Location = new System.Drawing.Point(802, 94);
            ServerSelectBox.Margin = new Padding(7, 2, 3, 6);
            ServerSelectBox.Name = "ServerSelectBox";
            ServerSelectBox.Size = new System.Drawing.Size(196, 109);
            ServerSelectBox.TabIndex = 67;
            ServerSelectBox.SelectedIndexChanged += ServerSelectBox_SelectedIndexChanged;
            // 
            // Disconn
            // 
            Disconn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Disconn.Enabled = false;
            Disconn.Location = new System.Drawing.Point(802, 55);
            Disconn.Margin = new Padding(4, 3, 4, 3);
            Disconn.Name = "Disconn";
            Disconn.Size = new System.Drawing.Size(196, 29);
            Disconn.TabIndex = 68;
            Disconn.Text = "Disconnect";
            Disconn.UseVisualStyleBackColor = true;
            Disconn.Click += Disconn_Click;
            // 
            // removeSrvBtn
            // 
            removeSrvBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            removeSrvBtn.Enabled = false;
            removeSrvBtn.Location = new System.Drawing.Point(800, 271);
            removeSrvBtn.Margin = new Padding(7, 2, 3, 2);
            removeSrvBtn.Name = "removeSrvBtn";
            removeSrvBtn.Size = new System.Drawing.Size(198, 26);
            removeSrvBtn.TabIndex = 70;
            removeSrvBtn.Text = "Remove Selected Server";
            removeSrvBtn.UseVisualStyleBackColor = true;
            removeSrvBtn.Click += RemoveSrvBtn_Click;
            // 
            // ChkUpdates
            // 
            ChkUpdates.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ChkUpdates.Enabled = false;
            ChkUpdates.Location = new System.Drawing.Point(800, 361);
            ChkUpdates.Margin = new Padding(7, 2, 3, 2);
            ChkUpdates.Name = "ChkUpdates";
            ChkUpdates.Size = new System.Drawing.Size(198, 26);
            ChkUpdates.TabIndex = 71;
            ChkUpdates.Text = "Check for Updates";
            ChkUpdates.UseVisualStyleBackColor = true;
            ChkUpdates.Click += ChkUpdates_Click;
            // 
            // GlobBackup
            // 
            GlobBackup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            GlobBackup.Enabled = false;
            GlobBackup.Location = new System.Drawing.Point(594, 272);
            GlobBackup.Margin = new Padding(7, 2, 3, 2);
            GlobBackup.Name = "GlobBackup";
            GlobBackup.Size = new System.Drawing.Size(198, 25);
            GlobBackup.TabIndex = 72;
            GlobBackup.Text = "Backup All Servers";
            GlobBackup.UseVisualStyleBackColor = true;
            GlobBackup.Click += GlobBackup_Click;
            // 
            // EditCfg
            // 
            EditCfg.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            EditCfg.Enabled = false;
            EditCfg.Location = new System.Drawing.Point(800, 301);
            EditCfg.Margin = new Padding(3, 2, 7, 2);
            EditCfg.Name = "EditCfg";
            EditCfg.Size = new System.Drawing.Size(198, 26);
            EditCfg.TabIndex = 75;
            EditCfg.Text = "Edit BDS/MMS Configs";
            EditCfg.UseVisualStyleBackColor = true;
            EditCfg.Click += EditCfg_Click;
            // 
            // PlayerManagerBtn
            // 
            PlayerManagerBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PlayerManagerBtn.Enabled = false;
            PlayerManagerBtn.Location = new System.Drawing.Point(594, 331);
            PlayerManagerBtn.Margin = new Padding(3, 2, 7, 2);
            PlayerManagerBtn.Name = "PlayerManagerBtn";
            PlayerManagerBtn.Size = new System.Drawing.Size(198, 26);
            PlayerManagerBtn.TabIndex = 76;
            PlayerManagerBtn.Text = "Player Manager";
            PlayerManagerBtn.UseVisualStyleBackColor = true;
            PlayerManagerBtn.Click += PlayerManager_Click;
            // 
            // SingBackup
            // 
            SingBackup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SingBackup.Enabled = false;
            SingBackup.Location = new System.Drawing.Point(594, 241);
            SingBackup.Margin = new Padding(3, 2, 7, 2);
            SingBackup.Name = "SingBackup";
            SingBackup.Size = new System.Drawing.Size(198, 26);
            SingBackup.TabIndex = 79;
            SingBackup.Text = "Backup Selected Server";
            SingBackup.UseVisualStyleBackColor = true;
            SingBackup.Click += SingBackup_Click;
            // 
            // RestartSrv
            // 
            RestartSrv.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            RestartSrv.Enabled = false;
            RestartSrv.Location = new System.Drawing.Point(910, 211);
            RestartSrv.Margin = new Padding(3, 2, 7, 2);
            RestartSrv.Name = "RestartSrv";
            RestartSrv.Size = new System.Drawing.Size(88, 26);
            RestartSrv.TabIndex = 80;
            RestartSrv.Text = "Restart";
            RestartSrv.UseVisualStyleBackColor = true;
            RestartSrv.Click += RestartSrv_Click;
            // 
            // BackupManagerBtn
            // 
            BackupManagerBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BackupManagerBtn.Enabled = false;
            BackupManagerBtn.Location = new System.Drawing.Point(594, 301);
            BackupManagerBtn.Margin = new Padding(3, 2, 7, 2);
            BackupManagerBtn.Name = "BackupManagerBtn";
            BackupManagerBtn.Size = new System.Drawing.Size(198, 26);
            BackupManagerBtn.TabIndex = 81;
            BackupManagerBtn.Text = "Backup Manager";
            BackupManagerBtn.UseVisualStyleBackColor = true;
            BackupManagerBtn.Click += BackupManager_Click;
            // 
            // ServerInfoBox
            // 
            ServerInfoBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ServerInfoBox.Location = new System.Drawing.Point(594, 94);
            ServerInfoBox.Margin = new Padding(3, 2, 7, 6);
            ServerInfoBox.Multiline = true;
            ServerInfoBox.Name = "ServerInfoBox";
            ServerInfoBox.ReadOnly = true;
            ServerInfoBox.Size = new System.Drawing.Size(198, 109);
            ServerInfoBox.TabIndex = 82;
            // 
            // newSrvBtn
            // 
            newSrvBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newSrvBtn.Enabled = false;
            newSrvBtn.Location = new System.Drawing.Point(800, 241);
            newSrvBtn.Margin = new Padding(7, 2, 3, 2);
            newSrvBtn.Name = "newSrvBtn";
            newSrvBtn.Size = new System.Drawing.Size(198, 26);
            newSrvBtn.TabIndex = 83;
            newSrvBtn.Text = "Add New Server";
            newSrvBtn.UseVisualStyleBackColor = true;
            newSrvBtn.Click += newSrvBtn_Click;
            // 
            // nbtStudioBtn
            // 
            nbtStudioBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nbtStudioBtn.Enabled = false;
            nbtStudioBtn.Location = new System.Drawing.Point(800, 331);
            nbtStudioBtn.Margin = new Padding(7, 2, 3, 2);
            nbtStudioBtn.Name = "nbtStudioBtn";
            nbtStudioBtn.Size = new System.Drawing.Size(198, 26);
            nbtStudioBtn.TabIndex = 85;
            nbtStudioBtn.Text = "Edit World via NBTStudio";
            nbtStudioBtn.UseVisualStyleBackColor = true;
            nbtStudioBtn.Click += nbtStudioBtn_Click;
            // 
            // startStopBtn
            // 
            startStopBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startStopBtn.Enabled = false;
            startStopBtn.Location = new System.Drawing.Point(802, 211);
            startStopBtn.Margin = new Padding(3, 2, 3, 2);
            startStopBtn.Name = "startStopBtn";
            startStopBtn.Size = new System.Drawing.Size(88, 26);
            startStopBtn.TabIndex = 87;
            startStopBtn.Text = "Start/Stop";
            startStopBtn.UseVisualStyleBackColor = true;
            startStopBtn.Click += startStopBtn_Click;
            // 
            // clientPage
            // 
            clientPage.BackColor = System.Drawing.SystemColors.Control;
            clientPage.Controls.Add(clientLogBox);
            clientPage.Location = new System.Drawing.Point(4, 24);
            clientPage.Margin = new Padding(9, 13, 9, 13);
            clientPage.Name = "clientPage";
            clientPage.Size = new System.Drawing.Size(563, 372);
            clientPage.TabIndex = 2;
            clientPage.Text = "Client Log";
            // 
            // clientLogBox
            // 
            clientLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            clientLogBox.Location = new System.Drawing.Point(0, 0);
            clientLogBox.Margin = new Padding(0);
            clientLogBox.Multiline = true;
            clientLogBox.Name = "clientLogBox";
            clientLogBox.ReadOnly = true;
            clientLogBox.ScrollBars = ScrollBars.Both;
            clientLogBox.Size = new System.Drawing.Size(563, 372);
            clientLogBox.TabIndex = 6;
            clientLogBox.WordWrap = false;
            // 
            // servicePage
            // 
            servicePage.BackColor = System.Drawing.SystemColors.Control;
            servicePage.Controls.Add(serviceTextbox);
            servicePage.Location = new System.Drawing.Point(4, 24);
            servicePage.Margin = new Padding(9, 13, 9, 13);
            servicePage.Name = "servicePage";
            servicePage.Size = new System.Drawing.Size(563, 372);
            servicePage.TabIndex = 1;
            servicePage.Text = "Service log";
            // 
            // serviceTextbox
            // 
            serviceTextbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            serviceTextbox.Location = new System.Drawing.Point(0, 0);
            serviceTextbox.Margin = new Padding(0);
            serviceTextbox.Multiline = true;
            serviceTextbox.Name = "serviceTextbox";
            serviceTextbox.ReadOnly = true;
            serviceTextbox.ScrollBars = ScrollBars.Both;
            serviceTextbox.Size = new System.Drawing.Size(563, 372);
            serviceTextbox.TabIndex = 5;
            serviceTextbox.WordWrap = false;
            // 
            // serverPage
            // 
            serverPage.BackColor = System.Drawing.SystemColors.Control;
            serverPage.Controls.Add(LogBox);
            serverPage.Location = new System.Drawing.Point(4, 24);
            serverPage.Margin = new Padding(9, 13, 9, 13);
            serverPage.Name = "serverPage";
            serverPage.Size = new System.Drawing.Size(563, 372);
            serverPage.TabIndex = 0;
            serverPage.Text = "Server log";
            // 
            // LogBox
            // 
            LogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LogBox.Location = new System.Drawing.Point(0, 0);
            LogBox.Margin = new Padding(0);
            LogBox.Multiline = true;
            LogBox.Name = "LogBox";
            LogBox.ReadOnly = true;
            LogBox.ScrollBars = ScrollBars.Both;
            LogBox.Size = new System.Drawing.Size(563, 372);
            LogBox.TabIndex = 4;
            LogBox.WordWrap = false;
            // 
            // logPageControl
            // 
            logPageControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logPageControl.Controls.Add(serverPage);
            logPageControl.Controls.Add(servicePage);
            logPageControl.Controls.Add(clientPage);
            logPageControl.Location = new System.Drawing.Point(9, 8);
            logPageControl.Margin = new Padding(4);
            logPageControl.Name = "logPageControl";
            logPageControl.SelectedIndex = 0;
            logPageControl.Size = new System.Drawing.Size(571, 400);
            logPageControl.TabIndex = 89;
            // 
            // cmdTextBox
            // 
            cmdTextBox.AcceptsTab = true;
            cmdTextBox.AllowDrop = true;
            cmdTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cmdTextBox.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            cmdTextBox.BorderStyle = BorderStyle.FixedSingle;
            cmdTextBox.Cursor = Cursors.IBeam;
            cmdTextBox.Enabled = false;
            cmdTextBox.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            cmdTextBox.ForeColor = System.Drawing.SystemColors.Window;
            cmdTextBox.Location = new System.Drawing.Point(8, 417);
            cmdTextBox.Margin = new Padding(3, 6, 3, 2);
            cmdTextBox.Name = "cmdTextBox";
            cmdTextBox.Size = new System.Drawing.Size(521, 26);
            cmdTextBox.TabIndex = 73;
            cmdTextBox.Text = "";
            cmdTextBox.KeyPress += cmdTextBox_KeyPress;
            cmdTextBox.PreviewKeyDown += cmdTextBox_PreviewKeyDown;
            // 
            // SendCmd
            // 
            SendCmd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            SendCmd.Enabled = false;
            SendCmd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            SendCmd.Location = new System.Drawing.Point(539, 417);
            SendCmd.Margin = new Padding(7, 2, 3, 2);
            SendCmd.Name = "SendCmd";
            SendCmd.Size = new System.Drawing.Size(41, 26);
            SendCmd.TabIndex = 74;
            SendCmd.Text = "↲";
            SendCmd.UseVisualStyleBackColor = true;
            SendCmd.Click += SendCmd_Click;
            // 
            // ManPacks
            // 
            ManPacks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ManPacks.Enabled = false;
            ManPacks.Location = new System.Drawing.Point(594, 361);
            ManPacks.Margin = new Padding(3, 2, 7, 2);
            ManPacks.Name = "ManPacks";
            ManPacks.Size = new System.Drawing.Size(198, 26);
            ManPacks.TabIndex = 78;
            ManPacks.Text = "R/B Pack Manager";
            ManPacks.UseVisualStyleBackColor = true;
            ManPacks.Click += ManPacks_Click;
            // 
            // scrollLockChkBox
            // 
            scrollLockChkBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            scrollLockChkBox.AutoSize = true;
            scrollLockChkBox.Enabled = false;
            scrollLockChkBox.Location = new System.Drawing.Point(592, 400);
            scrollLockChkBox.Margin = new Padding(3, 2, 3, 2);
            scrollLockChkBox.MinimumSize = new System.Drawing.Size(180, 22);
            scrollLockChkBox.Name = "scrollLockChkBox";
            scrollLockChkBox.Size = new System.Drawing.Size(188, 22);
            scrollLockChkBox.TabIndex = 84;
            scrollLockChkBox.Text = "Lock textbox scrollbar to end   ";
            scrollLockChkBox.UseVisualStyleBackColor = true;
            scrollLockChkBox.CheckedChanged += scrollLockChkBox_CheckedChanged;
            // 
            // clientConfigBtn
            // 
            clientConfigBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            clientConfigBtn.Location = new System.Drawing.Point(594, 211);
            clientConfigBtn.Margin = new Padding(7, 2, 3, 2);
            clientConfigBtn.Name = "clientConfigBtn";
            clientConfigBtn.Size = new System.Drawing.Size(198, 26);
            clientConfigBtn.TabIndex = 86;
            clientConfigBtn.Text = "Edit client config";
            clientConfigBtn.UseVisualStyleBackColor = true;
            clientConfigBtn.Click += clientConfigBtn_Click;
            // 
            // serverConfigBtnMenu
            // 
            serverConfigBtnMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            serverConfigBtnMenu.Items.AddRange(new ToolStripItem[] { serverPropMenuItem, startCmdMenuItem, servicePropMenuItem, editCoreServicePropertiesToolStripMenuItem, importToolStripMenuItem, exportToolStripMenuItem });
            serverConfigBtnMenu.Name = "serverConfigBtnMenu";
            serverConfigBtnMenu.Size = new System.Drawing.Size(223, 136);
            // 
            // serverPropMenuItem
            // 
            serverPropMenuItem.Name = "serverPropMenuItem";
            serverPropMenuItem.Size = new System.Drawing.Size(222, 22);
            serverPropMenuItem.Text = "Edit server BDS properties";
            serverPropMenuItem.Click += serverPropMenuItem_Click;
            // 
            // startCmdMenuItem
            // 
            startCmdMenuItem.Name = "startCmdMenuItem";
            startCmdMenuItem.Size = new System.Drawing.Size(222, 22);
            startCmdMenuItem.Text = "Edit BDS Startup commands";
            startCmdMenuItem.Click += startCmdMenuItem_Click;
            // 
            // servicePropMenuItem
            // 
            servicePropMenuItem.Name = "servicePropMenuItem";
            servicePropMenuItem.Size = new System.Drawing.Size(222, 22);
            servicePropMenuItem.Text = "Edit MMS server properties";
            servicePropMenuItem.Click += servicePropMenuItem_Click;
            // 
            // editCoreServicePropertiesToolStripMenuItem
            // 
            editCoreServicePropertiesToolStripMenuItem.Name = "editCoreServicePropertiesToolStripMenuItem";
            editCoreServicePropertiesToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            editCoreServicePropertiesToolStripMenuItem.Text = "Edit MMS Service properties";
            editCoreServicePropertiesToolStripMenuItem.Click += editCoreServicePropertiesToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { serverPackageFileToolStripMenuItem, serviceConfigFileToolStripMenuItem });
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            importToolStripMenuItem.Text = "Import...";
            // 
            // serverPackageFileToolStripMenuItem
            // 
            serverPackageFileToolStripMenuItem.Name = "serverPackageFileToolStripMenuItem";
            serverPackageFileToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            serverPackageFileToolStripMenuItem.Text = "Server package file";
            serverPackageFileToolStripMenuItem.Click += serverPackageFileToolStripMenuItem_Click;
            // 
            // serviceConfigFileToolStripMenuItem
            // 
            serviceConfigFileToolStripMenuItem.Name = "serviceConfigFileToolStripMenuItem";
            serviceConfigFileToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            serviceConfigFileToolStripMenuItem.Text = "Service config file";
            serviceConfigFileToolStripMenuItem.Click += serviceConfigFileToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { serverExporterToolStripMenuItem, serviceConfigFileToolStripMenuItem1 });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            exportToolStripMenuItem.Text = "Export...";
            // 
            // serverExporterToolStripMenuItem
            // 
            serverExporterToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { asConfigOnlyToolStripMenuItem, importableBackupToolStripMenuItem, importableBackupWithPacksToolStripMenuItem, fullServerPackageToolStripMenuItem });
            serverExporterToolStripMenuItem.Name = "serverExporterToolStripMenuItem";
            serverExporterToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            serverExporterToolStripMenuItem.Text = "Server Exporter Menu";
            // 
            // asConfigOnlyToolStripMenuItem
            // 
            asConfigOnlyToolStripMenuItem.Name = "asConfigOnlyToolStripMenuItem";
            asConfigOnlyToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            asConfigOnlyToolStripMenuItem.Text = "Configuration file only";
            asConfigOnlyToolStripMenuItem.Click += asConfigOnlyToolStripMenuItem_Click;
            // 
            // importableBackupToolStripMenuItem
            // 
            importableBackupToolStripMenuItem.Name = "importableBackupToolStripMenuItem";
            importableBackupToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            importableBackupToolStripMenuItem.Text = "Importable backup";
            importableBackupToolStripMenuItem.Click += importableBackupToolStripMenuItem_Click;
            // 
            // importableBackupWithPacksToolStripMenuItem
            // 
            importableBackupWithPacksToolStripMenuItem.Name = "importableBackupWithPacksToolStripMenuItem";
            importableBackupWithPacksToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            importableBackupWithPacksToolStripMenuItem.Text = "Importable backup with packs";
            importableBackupWithPacksToolStripMenuItem.Click += importableBackupWithPacksToolStripMenuItem_Click;
            // 
            // fullServerPackageToolStripMenuItem
            // 
            fullServerPackageToolStripMenuItem.Name = "fullServerPackageToolStripMenuItem";
            fullServerPackageToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            fullServerPackageToolStripMenuItem.Text = "Full server package";
            fullServerPackageToolStripMenuItem.Click += fullServerPackageToolStripMenuItem_Click;
            // 
            // serviceConfigFileToolStripMenuItem1
            // 
            serviceConfigFileToolStripMenuItem1.Name = "serviceConfigFileToolStripMenuItem1";
            serviceConfigFileToolStripMenuItem1.Size = new System.Drawing.Size(187, 22);
            serviceConfigFileToolStripMenuItem1.Text = "Service config file";
            serviceConfigFileToolStripMenuItem1.Click += serviceConfigFileToolStripMenuItem1_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1009, 454);
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
            Margin = new Padding(7, 10, 7, 10);
            MinimumSize = new System.Drawing.Size(745, 460);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Minecraft Management Service Client";
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
        public System.Windows.Forms.Label HostInfoLabel;
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
        private System.Windows.Forms.RichTextBox cmdTextBox;       
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

