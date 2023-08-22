
namespace BedrockService.Client.Forms
{
    partial class AddNewServerForm
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
            this.srvNameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ipV4Box = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ipV6Box = new System.Windows.Forms.TextBox();
            this.editPropsBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.serverSettingsBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ServerTypeComboBox = new System.Windows.Forms.ComboBox();
            this.VersionSelectComboBox = new System.Windows.Forms.ComboBox();
            this.BetaVersionCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // srvNameBox
            // 
            this.srvNameBox.Location = new System.Drawing.Point(124, 94);
            this.srvNameBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.srvNameBox.Name = "srvNameBox";
            this.srvNameBox.Size = new System.Drawing.Size(116, 23);
            this.srvNameBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 97);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 127);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "IP v4 port:";
            // 
            // ipV4Box
            // 
            this.ipV4Box.Location = new System.Drawing.Point(124, 124);
            this.ipV4Box.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ipV4Box.Name = "ipV4Box";
            this.ipV4Box.Size = new System.Drawing.Size(116, 23);
            this.ipV4Box.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 157);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "IP v6 port:";
            // 
            // ipV6Box
            // 
            this.ipV6Box.Location = new System.Drawing.Point(124, 154);
            this.ipV6Box.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ipV6Box.Name = "ipV6Box";
            this.ipV6Box.Size = new System.Drawing.Size(116, 23);
            this.ipV6Box.TabIndex = 4;
            // 
            // editPropsBtn
            // 
            this.editPropsBtn.Location = new System.Drawing.Point(38, 237);
            this.editPropsBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.editPropsBtn.Name = "editPropsBtn";
            this.editPropsBtn.Size = new System.Drawing.Size(202, 27);
            this.editPropsBtn.TabIndex = 6;
            this.editPropsBtn.Text = "Edit server settings";
            this.editPropsBtn.UseVisualStyleBackColor = true;
            this.editPropsBtn.Click += new System.EventHandler(this.editPropsBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(124, 303);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(117, 27);
            this.saveBtn.TabIndex = 8;
            this.saveBtn.Text = "Save Server";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(35, 10);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(201, 51);
            this.label4.TabIndex = 8;
            this.label4.Text = "Add a new server to the service. Note: Requires a Global restart to run new serve" +
    "r!";
            // 
            // serverSettingsBtn
            // 
            this.serverSettingsBtn.Location = new System.Drawing.Point(39, 270);
            this.serverSettingsBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serverSettingsBtn.Name = "serverSettingsBtn";
            this.serverSettingsBtn.Size = new System.Drawing.Size(202, 27);
            this.serverSettingsBtn.TabIndex = 7;
            this.serverSettingsBtn.Text = "Edit Service settings for server";
            this.serverSettingsBtn.UseVisualStyleBackColor = true;
            this.serverSettingsBtn.Click += new System.EventHandler(this.serverSettingsBtn_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 207);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Deploy version:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(49, 67);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 15);
            this.label6.TabIndex = 12;
            this.label6.Text = "Server type:";
            // 
            // ServerTypeComboBox
            // 
            this.ServerTypeComboBox.FormattingEnabled = true;
            this.ServerTypeComboBox.Location = new System.Drawing.Point(124, 64);
            this.ServerTypeComboBox.Name = "ServerTypeComboBox";
            this.ServerTypeComboBox.Size = new System.Drawing.Size(116, 23);
            this.ServerTypeComboBox.TabIndex = 13;
            this.ServerTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.ServerTypeComboBox_SelectedIndexChanged);
            // 
            // VersionSelectComboBox
            // 
            this.VersionSelectComboBox.FormattingEnabled = true;
            this.VersionSelectComboBox.Location = new System.Drawing.Point(124, 204);
            this.VersionSelectComboBox.Name = "VersionSelectComboBox";
            this.VersionSelectComboBox.Size = new System.Drawing.Size(116, 23);
            this.VersionSelectComboBox.TabIndex = 14;
            this.VersionSelectComboBox.SelectedIndexChanged += new System.EventHandler(this.VersionSelectComboBox_SelectedIndexChanged);
            // 
            // BetaVersionCheckBox
            // 
            this.BetaVersionCheckBox.AutoSize = true;
            this.BetaVersionCheckBox.Location = new System.Drawing.Point(24, 181);
            this.BetaVersionCheckBox.Name = "BetaVersionCheckBox";
            this.BetaVersionCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.BetaVersionCheckBox.Size = new System.Drawing.Size(115, 19);
            this.BetaVersionCheckBox.TabIndex = 15;
            this.BetaVersionCheckBox.Text = ":Use beta version";
            this.BetaVersionCheckBox.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.BetaVersionCheckBox.UseVisualStyleBackColor = true;
            this.BetaVersionCheckBox.CheckedChanged += new System.EventHandler(this.BetaVersionCheckBox_CheckedChanged);
            // 
            // AddNewServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 341);
            this.Controls.Add(this.BetaVersionCheckBox);
            this.Controls.Add(this.VersionSelectComboBox);
            this.Controls.Add(this.ServerTypeComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.serverSettingsBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.editPropsBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ipV6Box);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ipV4Box);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.srvNameBox);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(290, 380);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(290, 380);
            this.Name = "AddNewServerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AddNewServerForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox srvNameBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ipV4Box;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ipV6Box;
        private System.Windows.Forms.Button editPropsBtn;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button serverSettingsBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox ServerTypeComboBox;
        private System.Windows.Forms.ComboBox VersionSelectComboBox;
        private System.Windows.Forms.CheckBox BetaVersionCheckBox;
    }
}