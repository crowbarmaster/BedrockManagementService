namespace MinecraftService.Client.Forms {
    partial class BackupManagerForm {
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
            base.Invoke(() => base.Dispose(disposing));
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupManagerForm));
            this.backupSelectBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.infoTextBox = new System.Windows.Forms.TextBox();
            this.closeBtn = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteThisBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rollbackToThisBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allBackupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.purgeBackupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editServerBackupSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // backupSelectBox
            // 
            this.backupSelectBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.backupSelectBox.FormattingEnabled = true;
            this.backupSelectBox.Location = new System.Drawing.Point(177, 31);
            this.backupSelectBox.Margin = new System.Windows.Forms.Padding(5, 10, 10, 10);
            this.backupSelectBox.Name = "backupSelectBox";
            this.backupSelectBox.Size = new System.Drawing.Size(274, 23);
            this.backupSelectBox.TabIndex = 0;
            this.backupSelectBox.SelectedIndexChanged += new System.EventHandler(this.backupSelectBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(10, 10, 5, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select an available backup:";
            // 
            // infoTextBox
            // 
            this.infoTextBox.Location = new System.Drawing.Point(19, 74);
            this.infoTextBox.Margin = new System.Windows.Forms.Padding(10);
            this.infoTextBox.Multiline = true;
            this.infoTextBox.Name = "infoTextBox";
            this.infoTextBox.ReadOnly = true;
            this.infoTextBox.Size = new System.Drawing.Size(432, 126);
            this.infoTextBox.TabIndex = 2;
            this.infoTextBox.Text = resources.GetString("infoTextBox.Text");
            this.infoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // closeBtn
            // 
            this.closeBtn.Location = new System.Drawing.Point(200, 220);
            this.closeBtn.Margin = new System.Windows.Forms.Padding(10);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(75, 23);
            this.closeBtn.TabIndex = 4;
            this.closeBtn.Text = "Close";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(470, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedBackupToolStripMenuItem,
            this.allBackupsToolStripMenuItem,
            this.editServerBackupSettingsToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // selectedBackupToolStripMenuItem
            // 
            this.selectedBackupToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteThisBackupToolStripMenuItem,
            this.rollbackToThisBackupToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.selectedBackupToolStripMenuItem.Name = "selectedBackupToolStripMenuItem";
            this.selectedBackupToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.selectedBackupToolStripMenuItem.Text = "Selected backup";
            // 
            // deleteThisBackupToolStripMenuItem
            // 
            this.deleteThisBackupToolStripMenuItem.Name = "deleteThisBackupToolStripMenuItem";
            this.deleteThisBackupToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.deleteThisBackupToolStripMenuItem.Text = "Delete";
            this.deleteThisBackupToolStripMenuItem.Click += new System.EventHandler(this.deleteThisBackupToolStripMenuItem_Click);
            // 
            // rollbackToThisBackupToolStripMenuItem
            // 
            this.rollbackToThisBackupToolStripMenuItem.Name = "rollbackToThisBackupToolStripMenuItem";
            this.rollbackToThisBackupToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.rollbackToThisBackupToolStripMenuItem.Text = "Rollback";
            this.rollbackToThisBackupToolStripMenuItem.Click += new System.EventHandler(this.rollbackToThisBackupToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // allBackupsToolStripMenuItem
            // 
            this.allBackupsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.purgeBackupsToolStripMenuItem});
            this.allBackupsToolStripMenuItem.Name = "allBackupsToolStripMenuItem";
            this.allBackupsToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.allBackupsToolStripMenuItem.Text = "All backups";
            // 
            // purgeBackupsToolStripMenuItem
            // 
            this.purgeBackupsToolStripMenuItem.Name = "purgeBackupsToolStripMenuItem";
            this.purgeBackupsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.purgeBackupsToolStripMenuItem.Text = "Purge backups";
            this.purgeBackupsToolStripMenuItem.Click += new System.EventHandler(this.purgeBackupsToolStripMenuItem_Click);
            // 
            // editServerBackupSettingsToolStripMenuItem
            // 
            this.editServerBackupSettingsToolStripMenuItem.Name = "editServerBackupSettingsToolStripMenuItem";
            this.editServerBackupSettingsToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.editServerBackupSettingsToolStripMenuItem.Text = "Edit server backup settings";
            this.editServerBackupSettingsToolStripMenuItem.Click += new System.EventHandler(this.editServerBackupSettingsToolStripMenuItem_Click);
            // 
            // BackupManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 262);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.infoTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.backupSelectBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.ShowInTaskbar = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "BackupManagerForm";
            this.Text = "BackupManagerForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox backupSelectBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox infoTextBox;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectedBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allBackupsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteThisBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rollbackToThisBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem purgeBackupsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editServerBackupSettingsToolStripMenuItem;
    }
}