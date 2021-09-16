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
            this.StopStartSvc = new System.Windows.Forms.Button();
            this.RestartSvc = new System.Windows.Forms.Button();
            this.EditGlobals = new System.Windows.Forms.Button();
            this.EditService = new System.Windows.Forms.Button();
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
            this.Rollbackup = new System.Windows.Forms.Button();
            this.SvcLog = new System.Windows.Forms.CheckBox();
            this.ServerInfoBox = new System.Windows.Forms.TextBox();
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
            // StopStartSvc
            // 
            this.StopStartSvc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StopStartSvc.Enabled = false;
            this.StopStartSvc.Location = new System.Drawing.Point(786, 229);
            this.StopStartSvc.Name = "StopStartSvc";
            this.StopStartSvc.Size = new System.Drawing.Size(75, 23);
            this.StopStartSvc.TabIndex = 6;
            this.StopStartSvc.Text = "StopStart";
            this.StopStartSvc.UseVisualStyleBackColor = true;
            this.StopStartSvc.Click += new System.EventHandler(this.StopStartSvc_Click);
            // 
            // RestartSvc
            // 
            this.RestartSvc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RestartSvc.Enabled = false;
            this.RestartSvc.Location = new System.Drawing.Point(881, 229);
            this.RestartSvc.Name = "RestartSvc";
            this.RestartSvc.Size = new System.Drawing.Size(75, 23);
            this.RestartSvc.TabIndex = 7;
            this.RestartSvc.Text = "RestartSvc";
            this.RestartSvc.UseVisualStyleBackColor = true;
            // 
            // EditGlobals
            // 
            this.EditGlobals.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditGlobals.Enabled = false;
            this.EditGlobals.Location = new System.Drawing.Point(786, 258);
            this.EditGlobals.Name = "EditGlobals";
            this.EditGlobals.Size = new System.Drawing.Size(75, 23);
            this.EditGlobals.TabIndex = 8;
            this.EditGlobals.Text = "EditGlobals";
            this.EditGlobals.UseVisualStyleBackColor = true;
            this.EditGlobals.Click += new System.EventHandler(this.EditGlobals_Click);
            // 
            // EditService
            // 
            this.EditService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditService.Enabled = false;
            this.EditService.Location = new System.Drawing.Point(881, 258);
            this.EditService.Name = "EditService";
            this.EditService.Size = new System.Drawing.Size(75, 23);
            this.EditService.TabIndex = 9;
            this.EditService.Text = "EditService";
            this.EditService.UseVisualStyleBackColor = true;
            // 
            // ChkUpdates
            // 
            this.ChkUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkUpdates.Enabled = false;
            this.ChkUpdates.Location = new System.Drawing.Point(786, 287);
            this.ChkUpdates.Name = "ChkUpdates";
            this.ChkUpdates.Size = new System.Drawing.Size(75, 23);
            this.ChkUpdates.TabIndex = 10;
            this.ChkUpdates.Text = "ChkUpdates";
            this.ChkUpdates.UseVisualStyleBackColor = true;
            // 
            // GlobBackup
            // 
            this.GlobBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GlobBackup.Enabled = false;
            this.GlobBackup.Location = new System.Drawing.Point(881, 287);
            this.GlobBackup.Name = "GlobBackup";
            this.GlobBackup.Size = new System.Drawing.Size(75, 23);
            this.GlobBackup.TabIndex = 11;
            this.GlobBackup.Text = "GlobBckup";
            this.GlobBackup.UseVisualStyleBackColor = true;
            this.GlobBackup.Click += new System.EventHandler(this.GlobBackup_Click);
            // 
            // cmdTextBox
            // 
            this.cmdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdTextBox.Enabled = false;
            this.cmdTextBox.Location = new System.Drawing.Point(601, 391);
            this.cmdTextBox.Name = "cmdTextBox";
            this.cmdTextBox.Size = new System.Drawing.Size(355, 20);
            this.cmdTextBox.TabIndex = 12;
            // 
            // SendCmd
            // 
            this.SendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SendCmd.Enabled = false;
            this.SendCmd.Location = new System.Drawing.Point(601, 362);
            this.SendCmd.Name = "SendCmd";
            this.SendCmd.Size = new System.Drawing.Size(170, 23);
            this.SendCmd.TabIndex = 13;
            this.SendCmd.Text = "SendCmd";
            this.SendCmd.UseVisualStyleBackColor = true;
            this.SendCmd.Click += new System.EventHandler(this.SendCmd_Click);
            // 
            // EditCfg
            // 
            this.EditCfg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditCfg.Enabled = false;
            this.EditCfg.Location = new System.Drawing.Point(601, 229);
            this.EditCfg.Name = "EditCfg";
            this.EditCfg.Size = new System.Drawing.Size(75, 23);
            this.EditCfg.TabIndex = 15;
            this.EditCfg.Text = "EditCfg";
            this.EditCfg.UseVisualStyleBackColor = true;
            this.EditCfg.Click += new System.EventHandler(this.EditCfg_Click);
            // 
            // EditPerms
            // 
            this.PlayerManagerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayerManagerBtn.Enabled = false;
            this.PlayerManagerBtn.Location = new System.Drawing.Point(601, 316);
            this.PlayerManagerBtn.Name = "EditPerms";
            this.PlayerManagerBtn.Size = new System.Drawing.Size(170, 23);
            this.PlayerManagerBtn.TabIndex = 16;
            this.PlayerManagerBtn.Text = "PlayerManager";
            this.PlayerManagerBtn.UseVisualStyleBackColor = true;
            this.PlayerManagerBtn.Click += new System.EventHandler(this.PlayerManager_Click);
            // 
            // EditStCmd
            // 
            this.EditStCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditStCmd.Enabled = false;
            this.EditStCmd.Location = new System.Drawing.Point(696, 229);
            this.EditStCmd.Name = "EditStCmd";
            this.EditStCmd.Size = new System.Drawing.Size(75, 23);
            this.EditStCmd.TabIndex = 18;
            this.EditStCmd.Text = "EditStCmd";
            this.EditStCmd.UseVisualStyleBackColor = true;
            // 
            // ManPacks
            // 
            this.ManPacks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ManPacks.Enabled = false;
            this.ManPacks.Location = new System.Drawing.Point(601, 287);
            this.ManPacks.Name = "ManPacks";
            this.ManPacks.Size = new System.Drawing.Size(75, 23);
            this.ManPacks.TabIndex = 19;
            this.ManPacks.Text = "ManPacks";
            this.ManPacks.UseVisualStyleBackColor = true;
            // 
            // SingBackup
            // 
            this.SingBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SingBackup.Enabled = false;
            this.SingBackup.Location = new System.Drawing.Point(696, 258);
            this.SingBackup.Name = "SingBackup";
            this.SingBackup.Size = new System.Drawing.Size(75, 23);
            this.SingBackup.TabIndex = 20;
            this.SingBackup.Text = "SingBackup";
            this.SingBackup.UseVisualStyleBackColor = true;
            this.SingBackup.Click += new System.EventHandler(this.SingBackup_Click);
            // 
            // RestartSrv
            // 
            this.RestartSrv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RestartSrv.Enabled = false;
            this.RestartSrv.Location = new System.Drawing.Point(696, 287);
            this.RestartSrv.Name = "RestartSrv";
            this.RestartSrv.Size = new System.Drawing.Size(75, 23);
            this.RestartSrv.TabIndex = 21;
            this.RestartSrv.Text = "RestartSvr";
            this.RestartSrv.UseVisualStyleBackColor = true;
            this.RestartSrv.Click += new System.EventHandler(this.RestartSrv_Click);
            // 
            // Rollbackup
            // 
            this.Rollbackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Rollbackup.Enabled = false;
            this.Rollbackup.Location = new System.Drawing.Point(601, 258);
            this.Rollbackup.Name = "Rollbackup";
            this.Rollbackup.Size = new System.Drawing.Size(75, 23);
            this.Rollbackup.TabIndex = 22;
            this.Rollbackup.Text = "RollBackup";
            this.Rollbackup.UseVisualStyleBackColor = true;
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
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 460);
            this.Controls.Add(this.ServerInfoBox);
            this.Controls.Add(this.SvcLog);
            this.Controls.Add(this.Rollbackup);
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
            this.Controls.Add(this.EditService);
            this.Controls.Add(this.EditGlobals);
            this.Controls.Add(this.RestartSvc);
            this.Controls.Add(this.StopStartSvc);
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
        private System.Windows.Forms.Button StopStartSvc;
        private System.Windows.Forms.Button RestartSvc;
        private System.Windows.Forms.Button EditGlobals;
        private System.Windows.Forms.Button EditService;
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
        private System.Windows.Forms.Button Rollbackup;
        private System.Windows.Forms.CheckBox SvcLog;
        private System.Windows.Forms.TextBox ServerInfoBox;
        public System.Windows.Forms.ComboBox HostListBox;
    }
}

