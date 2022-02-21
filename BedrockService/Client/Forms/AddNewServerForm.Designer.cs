
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
            this.versionTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // srvNameBox
            // 
            this.srvNameBox.Location = new System.Drawing.Point(124, 65);
            this.srvNameBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.srvNameBox.Name = "srvNameBox";
            this.srvNameBox.Size = new System.Drawing.Size(116, 23);
            this.srvNameBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 68);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 98);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "IP v4 port:";
            // 
            // ipV4Box
            // 
            this.ipV4Box.Location = new System.Drawing.Point(124, 95);
            this.ipV4Box.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ipV4Box.Name = "ipV4Box";
            this.ipV4Box.Size = new System.Drawing.Size(116, 23);
            this.ipV4Box.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 128);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "IP v6 port:";
            // 
            // ipV6Box
            // 
            this.ipV6Box.Location = new System.Drawing.Point(124, 125);
            this.ipV6Box.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ipV6Box.Name = "ipV6Box";
            this.ipV6Box.Size = new System.Drawing.Size(116, 23);
            this.ipV6Box.TabIndex = 4;
            // 
            // editPropsBtn
            // 
            this.editPropsBtn.Location = new System.Drawing.Point(38, 186);
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
            this.saveBtn.Location = new System.Drawing.Point(124, 252);
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
            this.serverSettingsBtn.Location = new System.Drawing.Point(39, 219);
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
            this.label5.Location = new System.Drawing.Point(35, 158);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 15);
            this.label5.TabIndex = 11;
            this.label5.Text = "Deploy version:";
            // 
            // versionTextBox
            // 
            this.versionTextBox.Location = new System.Drawing.Point(124, 155);
            this.versionTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.versionTextBox.Name = "versionTextBox";
            this.versionTextBox.Size = new System.Drawing.Size(116, 23);
            this.versionTextBox.TabIndex = 5;
            this.versionTextBox.TextChanged += new System.EventHandler(this.versionTextBox_TextChanged);
            // 
            // AddNewServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 291);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.versionTextBox);
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
            this.MaximumSize = new System.Drawing.Size(290, 330);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(290, 330);
            this.Name = "AddNewServerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private System.Windows.Forms.TextBox versionTextBox;
    }
}