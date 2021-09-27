namespace BedrockService.Client.Forms
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Connect = new System.Windows.Forms.Button();
            this.HostInfoLabel = new System.Windows.Forms.Label();
            this.HostListBox = new System.Windows.Forms.ComboBox();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.ServerSelectBox = new System.Windows.Forms.ListBox();
            this.Disconn = new System.Windows.Forms.Button();
            this.EditGlobals = new System.Windows.Forms.Button();
            this.removeSrvBtn = new System.Windows.Forms.Button();
            this.ChkUpdates = new System.Windows.Forms.Button();
            this.GlobBackup = new System.Windows.Forms.Button();
            this.cmdTextBox = new System.Windows.Forms.TextBox();
            this.SendCmd = new System.Windows.Forms.Button();
            this.EditCfg = new System.Windows.Forms.Button();
            this.PlayerManagerBtn = new System.Windows.Forms.Button();
            this.EditStCmd = new System.Windows.Forms.Button();
            this.ManPacks = new System.Windows.Forms.Button();
            this.SingBackup = new System.Windows.Forms.Button();
            this.RestartSrv = new System.Windows.Forms.Button();
            this.BackupManagerBtn = new System.Windows.Forms.Button();
            this.SvcLog = new System.Windows.Forms.CheckBox();
            this.ServerInfoBox = new System.Windows.Forms.TextBox();
            this.newSrvBtn = new System.Windows.Forms.Button();
            this.scrollLockChkBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Connect
            // 
            this.Connect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Connect.Location = new System.Drawing.Point(601, 73);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(170, 25);
            this.Connect.TabIndex = 0;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // HostInfoLabel
            // 
            this.HostInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HostInfoLabel.AutoSize = true;
            this.HostInfoLabel.Location = new System.Drawing.Point(598, 28);
            this.HostInfoLabel.Name = "HostInfoLabel";
            this.HostInfoLabel.Size = new System.Drawing.Size(87, 13);
            this.HostInfoLabel.TabIndex = 1;
            this.HostInfoLabel.Text = "HostConnectInfo";
            // 
            // HostListBox
            // 
            this.HostListBox.AllowDrop = true;
            this.HostListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.HostListBox.FormattingEnabled = true;
            this.HostListBox.Location = new System.Drawing.Point(601, 44);
            this.HostListBox.Name = "HostListBox";
            this.HostListBox.Size = new System.Drawing.Size(355, 21);
            this.HostListBox.TabIndex = 2;
            this.HostListBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HostListBox_KeyPress);
            // 
            // LogBox
            // 
            this.LogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogBox.Location = new System.Drawing.Point(12, 44);
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogBox.Size = new System.Drawing.Size(559, 341);
            this.LogBox.TabIndex = 3;
            this.LogBox.WordWrap = false;
            // 
            // ServerSelectBox
            // 
            this.ServerSelectBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerSelectBox.FormattingEnabled = true;
            this.ServerSelectBox.Location = new System.Drawing.Point(786, 112);
            this.ServerSelectBox.Name = "ServerSelectBox";
            this.ServerSelectBox.Size = new System.Drawing.Size(170, 95);
            this.ServerSelectBox.TabIndex = 4;
            this.ServerSelectBox.SelectedIndexChanged += new System.EventHandler(this.ServerSelectBox_SelectedIndexChanged);
            // 
            // Disconn
            // 
            this.Disconn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Disconn.Enabled = false;
            this.Disconn.Location = new System.Drawing.Point(786, 73);
            this.Disconn.Name = "Disconn";
            this.Disconn.Size = new System.Drawing.Size(170, 25);
            this.Disconn.TabIndex = 5;
            this.Disconn.Text = "Disconnect";
            this.Disconn.UseVisualStyleBackColor = true;
            this.Disconn.Click += new System.EventHandler(this.Disconn_Click);
            // 
            // EditGlobals
            // 
            this.EditGlobals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditGlobals.Enabled = false;
            this.EditGlobals.Location = new System.Drawing.Point(786, 282);
            this.EditGlobals.Name = "EditGlobals";
            this.EditGlobals.Size = new System.Drawing.Size(170, 23);
            this.EditGlobals.TabIndex = 8;
            this.EditGlobals.Text = "Edit global service settings";
            this.EditGlobals.UseVisualStyleBackColor = true;
            this.EditGlobals.Click += new System.EventHandler(this.EditGlobals_Click);
            // 
            // removeSrvBtn
            // 
            this.removeSrvBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSrvBtn.Enabled = false;
            this.removeSrvBtn.Location = new System.Drawing.Point(786, 369);
            this.removeSrvBtn.Name = "removeSrvBtn";
            this.removeSrvBtn.Size = new System.Drawing.Size(170, 23);
            this.removeSrvBtn.TabIndex = 9;
            this.removeSrvBtn.Text = "Remove selected server";
            this.removeSrvBtn.UseVisualStyleBackColor = true;
            this.removeSrvBtn.Click += new System.EventHandler(this.removeSrvBtn_Click);
            // 
            // ChkUpdates
            // 
            this.ChkUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkUpdates.Enabled = false;
            this.ChkUpdates.Location = new System.Drawing.Point(786, 311);
            this.ChkUpdates.Name = "ChkUpdates";
            this.ChkUpdates.Size = new System.Drawing.Size(170, 23);
            this.ChkUpdates.TabIndex = 10;
            this.ChkUpdates.Text = "Check for updates";
            this.ChkUpdates.UseVisualStyleBackColor = true;
            this.ChkUpdates.Click += new System.EventHandler(this.ChkUpdates_Click);
            // 
            // GlobBackup
            // 
            this.GlobBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobBackup.Enabled = false;
            this.GlobBackup.Location = new System.Drawing.Point(786, 253);
            this.GlobBackup.Name = "GlobBackup";
            this.GlobBackup.Size = new System.Drawing.Size(170, 23);
            this.GlobBackup.TabIndex = 11;
            this.GlobBackup.Text = "Backup all servers";
            this.GlobBackup.UseVisualStyleBackColor = true;
            this.GlobBackup.Click += new System.EventHandler(this.GlobBackup_Click);
            // 
            // cmdTextBox
            // 
            this.cmdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdTextBox.Enabled = false;
            this.cmdTextBox.Location = new System.Drawing.Point(425, 428);
            this.cmdTextBox.Name = "cmdTextBox";
            this.cmdTextBox.Size = new System.Drawing.Size(355, 20);
            this.cmdTextBox.TabIndex = 12;
            this.cmdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmdTextBox_KeyPress);
            // 
            // SendCmd
            // 
            this.SendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendCmd.Enabled = false;
            this.SendCmd.Location = new System.Drawing.Point(786, 426);
            this.SendCmd.Name = "SendCmd";
            this.SendCmd.Size = new System.Drawing.Size(170, 23);
            this.SendCmd.TabIndex = 13;
            this.SendCmd.Text = "Send command to server";
            this.SendCmd.UseVisualStyleBackColor = true;
            this.SendCmd.Click += new System.EventHandler(this.SendCmd_Click);
            // 
            // EditCfg
            // 
            this.EditCfg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditCfg.Enabled = false;
            this.EditCfg.Location = new System.Drawing.Point(601, 224);
            this.EditCfg.Name = "EditCfg";
            this.EditCfg.Size = new System.Drawing.Size(170, 23);
            this.EditCfg.TabIndex = 15;
            this.EditCfg.Text = "Edit server config";
            this.EditCfg.UseVisualStyleBackColor = true;
            this.EditCfg.Click += new System.EventHandler(this.EditCfg_Click);
            // 
            // PlayerManagerBtn
            // 
            this.PlayerManagerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayerManagerBtn.Enabled = false;
            this.PlayerManagerBtn.Location = new System.Drawing.Point(601, 398);
            this.PlayerManagerBtn.Name = "PlayerManagerBtn";
            this.PlayerManagerBtn.Size = new System.Drawing.Size(170, 23);
            this.PlayerManagerBtn.TabIndex = 16;
            this.PlayerManagerBtn.Text = "Player Manager";
            this.PlayerManagerBtn.UseVisualStyleBackColor = true;
            this.PlayerManagerBtn.Click += new System.EventHandler(this.PlayerManager_Click);
            // 
            // EditStCmd
            // 
            this.EditStCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditStCmd.Enabled = false;
            this.EditStCmd.Location = new System.Drawing.Point(601, 253);
            this.EditStCmd.Name = "EditStCmd";
            this.EditStCmd.Size = new System.Drawing.Size(170, 23);
            this.EditStCmd.TabIndex = 18;
            this.EditStCmd.Text = "Edit start commands";
            this.EditStCmd.UseVisualStyleBackColor = true;
            this.EditStCmd.Click += new System.EventHandler(this.EditStCmd_Click);
            // 
            // ManPacks
            // 
            this.ManPacks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ManPacks.Enabled = false;
            this.ManPacks.Location = new System.Drawing.Point(601, 369);
            this.ManPacks.Name = "ManPacks";
            this.ManPacks.Size = new System.Drawing.Size(170, 23);
            this.ManPacks.TabIndex = 19;
            this.ManPacks.Text = "R/B Pack Manager";
            this.ManPacks.UseVisualStyleBackColor = true;
            // 
            // SingBackup
            // 
            this.SingBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SingBackup.Enabled = false;
            this.SingBackup.Location = new System.Drawing.Point(601, 282);
            this.SingBackup.Name = "SingBackup";
            this.SingBackup.Size = new System.Drawing.Size(170, 23);
            this.SingBackup.TabIndex = 20;
            this.SingBackup.Text = "Backup selected server";
            this.SingBackup.UseVisualStyleBackColor = true;
            this.SingBackup.Click += new System.EventHandler(this.SingBackup_Click);
            // 
            // RestartSrv
            // 
            this.RestartSrv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RestartSrv.Enabled = false;
            this.RestartSrv.Location = new System.Drawing.Point(601, 311);
            this.RestartSrv.Name = "RestartSrv";
            this.RestartSrv.Size = new System.Drawing.Size(170, 23);
            this.RestartSrv.TabIndex = 21;
            this.RestartSrv.Text = "Restart selected server";
            this.RestartSrv.UseVisualStyleBackColor = true;
            this.RestartSrv.Click += new System.EventHandler(this.RestartSrv_Click);
            // 
            // BackupManager
            // 
            this.BackupManagerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BackupManagerBtn.Enabled = false;
            this.BackupManagerBtn.Location = new System.Drawing.Point(601, 340);
            this.BackupManagerBtn.Name = "BackupManager";
            this.BackupManagerBtn.Size = new System.Drawing.Size(170, 23);
            this.BackupManagerBtn.TabIndex = 22;
            this.BackupManagerBtn.Text = "Backup Manager";
            this.BackupManagerBtn.UseVisualStyleBackColor = true;
            this.BackupManagerBtn.Click += new System.EventHandler(this.BackupManager_Click);
            // 
            // SvcLog
            // 
            this.SvcLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SvcLog.Enabled = false;
            this.SvcLog.Location = new System.Drawing.Point(225, 391);
            this.SvcLog.Name = "SvcLog";
            this.SvcLog.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SvcLog.Size = new System.Drawing.Size(137, 23);
            this.SvcLog.TabIndex = 24;
            this.SvcLog.Text = "Switch to service logs";
            this.SvcLog.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.SvcLog.UseVisualStyleBackColor = true;
            // 
            // ServerInfoBox
            // 
            this.ServerInfoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ServerInfoBox.Location = new System.Drawing.Point(601, 112);
            this.ServerInfoBox.Multiline = true;
            this.ServerInfoBox.Name = "ServerInfoBox";
            this.ServerInfoBox.ReadOnly = true;
            this.ServerInfoBox.Size = new System.Drawing.Size(170, 95);
            this.ServerInfoBox.TabIndex = 25;
            // 
            // newSrvBtn
            // 
            this.newSrvBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.newSrvBtn.Enabled = false;
            this.newSrvBtn.Location = new System.Drawing.Point(786, 224);
            this.newSrvBtn.Name = "newSrvBtn";
            this.newSrvBtn.Size = new System.Drawing.Size(170, 23);
            this.newSrvBtn.TabIndex = 26;
            this.newSrvBtn.Text = "Deploy new server";
            this.newSrvBtn.UseVisualStyleBackColor = true;
            this.newSrvBtn.Click += new System.EventHandler(this.newSrvBtn_Click);
            // 
            // scrollLockChkBox
            // 
            this.scrollLockChkBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollLockChkBox.Enabled = false;
            this.scrollLockChkBox.Location = new System.Drawing.Point(425, 391);
            this.scrollLockChkBox.Name = "scrollLockChkBox";
            this.scrollLockChkBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.scrollLockChkBox.Size = new System.Drawing.Size(137, 23);
            this.scrollLockChkBox.TabIndex = 27;
            this.scrollLockChkBox.Text = "Lock scrollbar to end";
            this.scrollLockChkBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.scrollLockChkBox.UseVisualStyleBackColor = true;
            this.scrollLockChkBox.CheckedChanged += new System.EventHandler(this.scrollLockChkBox_CheckedChanged);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 460);
            this.Controls.Add(this.scrollLockChkBox);
            this.Controls.Add(this.newSrvBtn);
            this.Controls.Add(this.ServerInfoBox);
            this.Controls.Add(this.SvcLog);
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
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.HostListBox);
            this.Controls.Add(this.HostInfoLabel);
            this.Controls.Add(this.Connect);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bedrock Service Management";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Label HostInfoLabel;
        public System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.ListBox ServerSelectBox;
        private System.Windows.Forms.Button Disconn;
        private System.Windows.Forms.Button EditGlobals;
        private System.Windows.Forms.Button removeSrvBtn;
        private System.Windows.Forms.Button ChkUpdates;
        private System.Windows.Forms.Button GlobBackup;
        private System.Windows.Forms.TextBox cmdTextBox;
        private System.Windows.Forms.Button SendCmd;
        private System.Windows.Forms.Button EditCfg;
        private System.Windows.Forms.Button PlayerManagerBtn;
        private System.Windows.Forms.Button EditStCmd;
        private System.Windows.Forms.Button ManPacks;
        private System.Windows.Forms.Button SingBackup;
        private System.Windows.Forms.Button RestartSrv;
        private System.Windows.Forms.Button BackupManagerBtn;
        private System.Windows.Forms.CheckBox SvcLog;
        private System.Windows.Forms.TextBox ServerInfoBox;
        public System.Windows.Forms.ComboBox HostListBox;
        private System.Windows.Forms.Button newSrvBtn;
        private System.Windows.Forms.CheckBox scrollLockChkBox;
    }
}

