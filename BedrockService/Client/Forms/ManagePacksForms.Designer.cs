
namespace MinecraftService.Client.Forms
{
    partial class ManagePacksForms
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
            this.pluginTabControl = new System.Windows.Forms.TabControl();
            this.addonTab = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.openFileBtn = new System.Windows.Forms.Button();
            this.selectedPackIcon = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.removeAllPacksBtn = new System.Windows.Forms.Button();
            this.removePackBtn = new System.Windows.Forms.Button();
            this.sendAllBtn = new System.Windows.Forms.Button();
            this.sendPacksBtn = new System.Windows.Forms.Button();
            this.parsedPacksListBox = new System.Windows.Forms.ListBox();
            this.serverListBox = new System.Windows.Forms.ListBox();
            this._pluginsTab = new System.Windows.Forms.TabPage();
            this._pluginReportBtn = new System.Windows.Forms.Button();
            this._pluginDisableBtn = new System.Windows.Forms.Button();
            this._pluginFromFileBtn = new System.Windows.Forms.Button();
            this._pluginInstallBtn = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pluginName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pluginDesc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pluginVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pluginTargetProtocol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pluginSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pluginInstalled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.pluginTabControl.SuspendLayout();
            this.addonTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectedPackIcon)).BeginInit();
            this._pluginsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // pluginTabControl
            // 
            this.pluginTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginTabControl.Controls.Add(this.addonTab);
            this.pluginTabControl.Controls.Add(this._pluginsTab);
            this.pluginTabControl.Location = new System.Drawing.Point(10, 10);
            this.pluginTabControl.Margin = new System.Windows.Forms.Padding(1);
            this.pluginTabControl.Name = "pluginTabControl";
            this.pluginTabControl.SelectedIndex = 0;
            this.pluginTabControl.Size = new System.Drawing.Size(976, 444);
            this.pluginTabControl.TabIndex = 0;
            // 
            // addonTab
            // 
            this.addonTab.BackColor = System.Drawing.SystemColors.Menu;
            this.addonTab.Controls.Add(this.label3);
            this.addonTab.Controls.Add(this.label2);
            this.addonTab.Controls.Add(this.label1);
            this.addonTab.Controls.Add(this.openFileBtn);
            this.addonTab.Controls.Add(this.selectedPackIcon);
            this.addonTab.Controls.Add(this.textBox1);
            this.addonTab.Controls.Add(this.removeAllPacksBtn);
            this.addonTab.Controls.Add(this.removePackBtn);
            this.addonTab.Controls.Add(this.sendAllBtn);
            this.addonTab.Controls.Add(this.sendPacksBtn);
            this.addonTab.Controls.Add(this.parsedPacksListBox);
            this.addonTab.Controls.Add(this.serverListBox);
            this.addonTab.Location = new System.Drawing.Point(4, 24);
            this.addonTab.Name = "addonTab";
            this.addonTab.Padding = new System.Windows.Forms.Padding(3);
            this.addonTab.Size = new System.Drawing.Size(968, 416);
            this.addonTab.TabIndex = 0;
            this.addonTab.Text = "MC Addons";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 44);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(454, 15);
            this.label3.TabIndex = 23;
            this.label3.Text = "!! NOTICE !! Send map pack alone, do not send all! Not all packs will parse serve" +
    "r-side!";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(708, 87);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(161, 15);
            this.label2.TabIndex = 22;
            this.label2.Text = "Packs found in archive file(s):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 87);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 15);
            this.label1.TabIndex = 21;
            this.label1.Text = "Current packs found on server:";
            // 
            // openFileBtn
            // 
            this.openFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openFileBtn.Location = new System.Drawing.Point(712, 44);
            this.openFileBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.openFileBtn.Name = "openFileBtn";
            this.openFileBtn.Size = new System.Drawing.Size(220, 27);
            this.openFileBtn.TabIndex = 20;
            this.openFileBtn.Text = "Open pack file(s)";
            this.openFileBtn.UseVisualStyleBackColor = true;
            // 
            // selectedPackIcon
            // 
            this.selectedPackIcon.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.selectedPackIcon.Location = new System.Drawing.Point(405, 98);
            this.selectedPackIcon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.selectedPackIcon.Name = "selectedPackIcon";
            this.selectedPackIcon.Size = new System.Drawing.Size(152, 150);
            this.selectedPackIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.selectedPackIcon.TabIndex = 19;
            this.selectedPackIcon.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox1.Location = new System.Drawing.Point(354, 254);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(255, 140);
            this.textBox1.TabIndex = 18;
            // 
            // removeAllPacksBtn
            // 
            this.removeAllPacksBtn.Location = new System.Drawing.Point(263, 139);
            this.removeAllPacksBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.removeAllPacksBtn.Name = "removeAllPacksBtn";
            this.removeAllPacksBtn.Size = new System.Drawing.Size(139, 27);
            this.removeAllPacksBtn.TabIndex = 17;
            this.removeAllPacksBtn.Text = "Remove all packs";
            this.removeAllPacksBtn.UseVisualStyleBackColor = true;
            // 
            // removePackBtn
            // 
            this.removePackBtn.Location = new System.Drawing.Point(263, 106);
            this.removePackBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.removePackBtn.Name = "removePackBtn";
            this.removePackBtn.Size = new System.Drawing.Size(139, 27);
            this.removePackBtn.TabIndex = 16;
            this.removePackBtn.Text = "Remove pack";
            this.removePackBtn.UseVisualStyleBackColor = true;
            // 
            // sendAllBtn
            // 
            this.sendAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendAllBtn.Location = new System.Drawing.Point(559, 139);
            this.sendAllBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.sendAllBtn.Name = "sendAllBtn";
            this.sendAllBtn.Size = new System.Drawing.Size(146, 27);
            this.sendAllBtn.TabIndex = 15;
            this.sendAllBtn.Text = "Send all packs";
            this.sendAllBtn.UseVisualStyleBackColor = true;
            // 
            // sendPacksBtn
            // 
            this.sendPacksBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendPacksBtn.Location = new System.Drawing.Point(559, 106);
            this.sendPacksBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.sendPacksBtn.Name = "sendPacksBtn";
            this.sendPacksBtn.Size = new System.Drawing.Size(146, 27);
            this.sendPacksBtn.TabIndex = 14;
            this.sendPacksBtn.Text = "Send selected packs";
            this.sendPacksBtn.UseVisualStyleBackColor = true;
            // 
            // parsedPacksListBox
            // 
            this.parsedPacksListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parsedPacksListBox.FormattingEnabled = true;
            this.parsedPacksListBox.ItemHeight = 15;
            this.parsedPacksListBox.Location = new System.Drawing.Point(712, 106);
            this.parsedPacksListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.parsedPacksListBox.Name = "parsedPacksListBox";
            this.parsedPacksListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.parsedPacksListBox.Size = new System.Drawing.Size(220, 289);
            this.parsedPacksListBox.TabIndex = 13;
            // 
            // serverListBox
            // 
            this.serverListBox.FormattingEnabled = true;
            this.serverListBox.ItemHeight = 15;
            this.serverListBox.Location = new System.Drawing.Point(36, 106);
            this.serverListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serverListBox.Name = "serverListBox";
            this.serverListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.serverListBox.Size = new System.Drawing.Size(220, 289);
            this.serverListBox.TabIndex = 12;
            // 
            // _pluginsTab
            // 
            this._pluginsTab.BackColor = System.Drawing.SystemColors.Menu;
            this._pluginsTab.Controls.Add(this.textBox2);
            this._pluginsTab.Controls.Add(this._pluginReportBtn);
            this._pluginsTab.Controls.Add(this._pluginDisableBtn);
            this._pluginsTab.Controls.Add(this._pluginFromFileBtn);
            this._pluginsTab.Controls.Add(this._pluginInstallBtn);
            this._pluginsTab.Controls.Add(this.dataGridView1);
            this._pluginsTab.Location = new System.Drawing.Point(4, 24);
            this._pluginsTab.Name = "_pluginsTab";
            this._pluginsTab.Padding = new System.Windows.Forms.Padding(3);
            this._pluginsTab.Size = new System.Drawing.Size(968, 416);
            this._pluginsTab.TabIndex = 1;
            this._pluginsTab.Text = "LL Plugin Manager";
            // 
            // _pluginReportBtn
            // 
            this._pluginReportBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._pluginReportBtn.Enabled = false;
            this._pluginReportBtn.Location = new System.Drawing.Point(263, 315);
            this._pluginReportBtn.Margin = new System.Windows.Forms.Padding(50, 10, 10, 10);
            this._pluginReportBtn.Name = "_pluginReportBtn";
            this._pluginReportBtn.Size = new System.Drawing.Size(150, 26);
            this._pluginReportBtn.TabIndex = 4;
            this._pluginReportBtn.Text = "Report plugin issue";
            this._pluginReportBtn.UseVisualStyleBackColor = true;
            this._pluginReportBtn.Visible = false;
            // 
            // _pluginDisableBtn
            // 
            this._pluginDisableBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._pluginDisableBtn.Enabled = false;
            this._pluginDisableBtn.Location = new System.Drawing.Point(263, 269);
            this._pluginDisableBtn.Margin = new System.Windows.Forms.Padding(50, 30, 10, 10);
            this._pluginDisableBtn.Name = "_pluginDisableBtn";
            this._pluginDisableBtn.Size = new System.Drawing.Size(150, 26);
            this._pluginDisableBtn.TabIndex = 3;
            this._pluginDisableBtn.Text = "Disable plugin";
            this._pluginDisableBtn.UseVisualStyleBackColor = true;
            this._pluginDisableBtn.Visible = false;
            // 
            // _pluginFromFileBtn
            // 
            this._pluginFromFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._pluginFromFileBtn.Location = new System.Drawing.Point(53, 315);
            this._pluginFromFileBtn.Margin = new System.Windows.Forms.Padding(50, 10, 10, 10);
            this._pluginFromFileBtn.Name = "_pluginFromFileBtn";
            this._pluginFromFileBtn.Size = new System.Drawing.Size(150, 26);
            this._pluginFromFileBtn.TabIndex = 2;
            this._pluginFromFileBtn.Text = "Add from file...";
            this._pluginFromFileBtn.UseVisualStyleBackColor = true;
            // 
            // _pluginInstallBtn
            // 
            this._pluginInstallBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._pluginInstallBtn.Location = new System.Drawing.Point(53, 269);
            this._pluginInstallBtn.Margin = new System.Windows.Forms.Padding(50, 30, 10, 10);
            this._pluginInstallBtn.Name = "_pluginInstallBtn";
            this._pluginInstallBtn.Size = new System.Drawing.Size(150, 26);
            this._pluginInstallBtn.TabIndex = 1;
            this._pluginInstallBtn.Text = "Install to server";
            this._pluginInstallBtn.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pluginName,
            this.pluginDesc,
            this.pluginVersion,
            this.pluginTargetProtocol,
            this.pluginSource,
            this.pluginInstalled});
            this.dataGridView1.Location = new System.Drawing.Point(6, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.Size = new System.Drawing.Size(956, 230);
            this.dataGridView1.TabIndex = 0;
            // 
            // pluginName
            // 
            this.pluginName.HeaderText = "Plugin Name";
            this.pluginName.MinimumWidth = 120;
            this.pluginName.Name = "pluginName";
            this.pluginName.ReadOnly = true;
            this.pluginName.Width = 120;
            // 
            // pluginDesc
            // 
            this.pluginDesc.HeaderText = "Description";
            this.pluginDesc.MinimumWidth = 50;
            this.pluginDesc.Name = "pluginDesc";
            this.pluginDesc.Width = 350;
            // 
            // pluginVersion
            // 
            this.pluginVersion.HeaderText = "Plugin Version";
            this.pluginVersion.MinimumWidth = 100;
            this.pluginVersion.Name = "pluginVersion";
            this.pluginVersion.ReadOnly = true;
            this.pluginVersion.Width = 120;
            // 
            // pluginTargetProtocol
            // 
            this.pluginTargetProtocol.HeaderText = "Proto Version";
            this.pluginTargetProtocol.MinimumWidth = 100;
            this.pluginTargetProtocol.Name = "pluginTargetProtocol";
            this.pluginTargetProtocol.ReadOnly = true;
            this.pluginTargetProtocol.Width = 120;
            // 
            // pluginSource
            // 
            this.pluginSource.HeaderText = "Source";
            this.pluginSource.MinimumWidth = 100;
            this.pluginSource.Name = "pluginSource";
            this.pluginSource.ReadOnly = true;
            this.pluginSource.Width = 120;
            // 
            // pluginInstalled
            // 
            this.pluginInstalled.HeaderText = "Installed";
            this.pluginInstalled.MinimumWidth = 50;
            this.pluginInstalled.Name = "pluginInstalled";
            this.pluginInstalled.ReadOnly = true;
            this.pluginInstalled.Width = 80;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(473, 269);
            this.textBox2.Margin = new System.Windows.Forms.Padding(50, 30, 3, 3);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(489, 141);
            this.textBox2.TabIndex = 5;
            // 
            // ManagePacksForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 464);
            this.Controls.Add(this.pluginTabControl);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(940, 444);
            this.Name = "ManagePacksForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pack Manager";
            this.pluginTabControl.ResumeLayout(false);
            this.addonTab.ResumeLayout(false);
            this.addonTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.selectedPackIcon)).EndInit();
            this._pluginsTab.ResumeLayout(false);
            this._pluginsTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl pluginTabControl;
        private System.Windows.Forms.TabPage addonTab;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button openFileBtn;
        private System.Windows.Forms.PictureBox selectedPackIcon;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button removeAllPacksBtn;
        private System.Windows.Forms.Button removePackBtn;
        private System.Windows.Forms.Button sendAllBtn;
        private System.Windows.Forms.Button sendPacksBtn;
        private System.Windows.Forms.ListBox parsedPacksListBox;
        private System.Windows.Forms.ListBox serverListBox;
        private System.Windows.Forms.TabPage _pluginsTab;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button _pluginInstallBtn;
        private System.Windows.Forms.Button _pluginReportBtn;
        private System.Windows.Forms.Button _pluginDisableBtn;
        private System.Windows.Forms.Button _pluginFromFileBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginName;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginDesc;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginTargetProtocol;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn pluginInstalled;
        private System.Windows.Forms.TextBox textBox2;
    }
}