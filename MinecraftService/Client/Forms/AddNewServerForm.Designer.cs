
namespace MinecraftService.Client.Forms
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
        private void InitializeComponent() {
            srvNameBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            ipV4Box = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            ipV6Box = new System.Windows.Forms.TextBox();
            editPropsBtn = new System.Windows.Forms.Button();
            saveBtn = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            serverSettingsBtn = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            ServerTypeComboBox = new System.Windows.Forms.ComboBox();
            VersionSelectComboBox = new System.Windows.Forms.ComboBox();
            BetaVersionCheckBox = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // srvNameBox
            // 
            srvNameBox.Location = new System.Drawing.Point(124, 94);
            srvNameBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            srvNameBox.Name = "srvNameBox";
            srvNameBox.Size = new System.Drawing.Size(116, 23);
            srvNameBox.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(41, 97);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(75, 15);
            label1.TabIndex = 1;
            label1.Text = "Server name:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(56, 127);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(60, 15);
            label2.TabIndex = 3;
            label2.Text = "IP v4 port:";
            // 
            // ipV4Box
            // 
            ipV4Box.Location = new System.Drawing.Point(124, 124);
            ipV4Box.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ipV4Box.Name = "ipV4Box";
            ipV4Box.Size = new System.Drawing.Size(116, 23);
            ipV4Box.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(56, 157);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(60, 15);
            label3.TabIndex = 5;
            label3.Text = "IP v6 port:";
            // 
            // ipV6Box
            // 
            ipV6Box.Location = new System.Drawing.Point(124, 154);
            ipV6Box.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ipV6Box.Name = "ipV6Box";
            ipV6Box.Size = new System.Drawing.Size(116, 23);
            ipV6Box.TabIndex = 4;
            // 
            // editPropsBtn
            // 
            editPropsBtn.Location = new System.Drawing.Point(38, 237);
            editPropsBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            editPropsBtn.Name = "editPropsBtn";
            editPropsBtn.Size = new System.Drawing.Size(202, 27);
            editPropsBtn.TabIndex = 6;
            editPropsBtn.Text = "Edit server settings";
            editPropsBtn.UseVisualStyleBackColor = true;
            editPropsBtn.Click += editPropsBtn_Click;
            // 
            // saveBtn
            // 
            saveBtn.Location = new System.Drawing.Point(124, 303);
            saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new System.Drawing.Size(117, 27);
            saveBtn.TabIndex = 8;
            saveBtn.Text = "Save Server";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += saveBtn_Click;
            // 
            // label4
            // 
            label4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            label4.Location = new System.Drawing.Point(35, 10);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(201, 51);
            label4.TabIndex = 8;
            label4.Text = "Add a new server to the service. Note: Requires a Global restart to run new server!";
            // 
            // serverSettingsBtn
            // 
            serverSettingsBtn.Location = new System.Drawing.Point(39, 270);
            serverSettingsBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            serverSettingsBtn.Name = "serverSettingsBtn";
            serverSettingsBtn.Size = new System.Drawing.Size(202, 27);
            serverSettingsBtn.TabIndex = 7;
            serverSettingsBtn.Text = "Edit Service settings for server";
            serverSettingsBtn.UseVisualStyleBackColor = true;
            serverSettingsBtn.Click += serverSettingsBtn_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(29, 207);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(88, 15);
            label5.TabIndex = 11;
            label5.Text = "Deploy version:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(49, 67);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(68, 15);
            label6.TabIndex = 12;
            label6.Text = "Server type:";
            // 
            // ServerTypeComboBox
            // 
            ServerTypeComboBox.FormattingEnabled = true;
            ServerTypeComboBox.Location = new System.Drawing.Point(124, 64);
            ServerTypeComboBox.Name = "ServerTypeComboBox";
            ServerTypeComboBox.Size = new System.Drawing.Size(116, 23);
            ServerTypeComboBox.TabIndex = 13;
            ServerTypeComboBox.SelectedIndexChanged += ServerTypeComboBox_SelectedIndexChanged;
            // 
            // VersionSelectComboBox
            // 
            VersionSelectComboBox.FormattingEnabled = true;
            VersionSelectComboBox.Location = new System.Drawing.Point(124, 204);
            VersionSelectComboBox.Name = "VersionSelectComboBox";
            VersionSelectComboBox.Size = new System.Drawing.Size(116, 23);
            VersionSelectComboBox.TabIndex = 14;
            VersionSelectComboBox.SelectedIndexChanged += VersionSelectComboBox_SelectedIndexChanged;
            // 
            // BetaVersionCheckBox
            // 
            BetaVersionCheckBox.AutoSize = true;
            BetaVersionCheckBox.Location = new System.Drawing.Point(24, 181);
            BetaVersionCheckBox.Name = "BetaVersionCheckBox";
            BetaVersionCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            BetaVersionCheckBox.Size = new System.Drawing.Size(115, 19);
            BetaVersionCheckBox.TabIndex = 15;
            BetaVersionCheckBox.Text = ":Use beta version";
            BetaVersionCheckBox.TextAlign = System.Drawing.ContentAlignment.TopRight;
            BetaVersionCheckBox.UseVisualStyleBackColor = true;
            BetaVersionCheckBox.CheckedChanged += BetaVersionCheckBox_CheckedChanged;
            // 
            // AddNewServerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(274, 341);
            Controls.Add(BetaVersionCheckBox);
            Controls.Add(VersionSelectComboBox);
            Controls.Add(ServerTypeComboBox);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(serverSettingsBtn);
            Controls.Add(label4);
            Controls.Add(saveBtn);
            Controls.Add(editPropsBtn);
            Controls.Add(label3);
            Controls.Add(ipV6Box);
            Controls.Add(label2);
            Controls.Add(ipV4Box);
            Controls.Add(label1);
            Controls.Add(srvNameBox);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(290, 380);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(290, 380);
            Name = "AddNewServerForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "AddNewServerForm";
            ResumeLayout(false);
            PerformLayout();

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
