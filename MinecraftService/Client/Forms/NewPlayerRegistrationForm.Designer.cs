
namespace MinecraftService.Client.Forms
{
    partial class NewPlayerRegistrationForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.xuidTextBox = new System.Windows.Forms.TextBox();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.permissionComboBox = new System.Windows.Forms.ComboBox();
            this.whitelistedChkBox = new System.Windows.Forms.CheckBox();
            this.ignoreLimitChkBox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(82, 38);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "XUID:";
            // 
            // xuidTextBox
            // 
            this.xuidTextBox.Location = new System.Drawing.Point(130, 35);
            this.xuidTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.xuidTextBox.Name = "xuidTextBox";
            this.xuidTextBox.Size = new System.Drawing.Size(215, 23);
            this.xuidTextBox.TabIndex = 2;
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(130, 65);
            this.usernameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(215, 23);
            this.usernameTextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Username:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(54, 98);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Permission:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 126);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Whitelisted?";
            // 
            // permissionComboBox
            // 
            this.permissionComboBox.FormattingEnabled = true;
            this.permissionComboBox.Items.AddRange(new object[] {
            "visitor",
            "member",
            "operator"});
            this.permissionComboBox.Location = new System.Drawing.Point(130, 95);
            this.permissionComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.permissionComboBox.Name = "permissionComboBox";
            this.permissionComboBox.Size = new System.Drawing.Size(215, 23);
            this.permissionComboBox.TabIndex = 7;
            // 
            // whitelistedChkBox
            // 
            this.whitelistedChkBox.AutoSize = true;
            this.whitelistedChkBox.Location = new System.Drawing.Point(130, 126);
            this.whitelistedChkBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.whitelistedChkBox.Name = "whitelistedChkBox";
            this.whitelistedChkBox.Size = new System.Drawing.Size(15, 14);
            this.whitelistedChkBox.TabIndex = 8;
            this.whitelistedChkBox.UseVisualStyleBackColor = true;
            // 
            // ignoreLimitChkBox
            // 
            this.ignoreLimitChkBox.AutoSize = true;
            this.ignoreLimitChkBox.Location = new System.Drawing.Point(130, 152);
            this.ignoreLimitChkBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ignoreLimitChkBox.Name = "ignoreLimitChkBox";
            this.ignoreLimitChkBox.Size = new System.Drawing.Size(15, 14);
            this.ignoreLimitChkBox.TabIndex = 9;
            this.ignoreLimitChkBox.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 152);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(112, 15);
            this.label6.TabIndex = 10;
            this.label6.Text = "Ignore max players?";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(258, 142);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 29);
            this.button1.TabIndex = 11;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.saveClick);
            // 
            // NewPlayerRegistrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 191);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ignoreLimitChkBox);
            this.Controls.Add(this.whitelistedChkBox);
            this.Controls.Add(this.permissionComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.usernameTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.xuidTextBox);
            this.Controls.Add(this.label2);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(375, 230);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(375, 230);
            this.Name = "NewPlayerRegistrationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Player Registration Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox xuidTextBox;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox permissionComboBox;
        private System.Windows.Forms.CheckBox whitelistedChkBox;
        private System.Windows.Forms.CheckBox ignoreLimitChkBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
    }
}