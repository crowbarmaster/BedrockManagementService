
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
            this.SuspendLayout();
            // 
            // srvNameBox
            // 
            this.srvNameBox.Location = new System.Drawing.Point(106, 56);
            this.srvNameBox.Name = "srvNameBox";
            this.srvNameBox.Size = new System.Drawing.Size(100, 20);
            this.srvNameBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Server name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "IP v4 port:";
            // 
            // ipV4Box
            // 
            this.ipV4Box.Location = new System.Drawing.Point(106, 82);
            this.ipV4Box.Name = "ipV4Box";
            this.ipV4Box.Size = new System.Drawing.Size(100, 20);
            this.ipV4Box.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "IP v6 port:";
            // 
            // ipV6Box
            // 
            this.ipV6Box.Location = new System.Drawing.Point(106, 108);
            this.ipV6Box.Name = "ipV6Box";
            this.ipV6Box.Size = new System.Drawing.Size(100, 20);
            this.ipV6Box.TabIndex = 4;
            // 
            // editPropsBtn
            // 
            this.editPropsBtn.Location = new System.Drawing.Point(33, 149);
            this.editPropsBtn.Name = "editPropsBtn";
            this.editPropsBtn.Size = new System.Drawing.Size(173, 23);
            this.editPropsBtn.TabIndex = 6;
            this.editPropsBtn.Text = "Edit server settings...";
            this.editPropsBtn.UseVisualStyleBackColor = true;
            this.editPropsBtn.Click += new System.EventHandler(this.editPropsBtn_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(106, 191);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(100, 23);
            this.saveBtn.TabIndex = 7;
            this.saveBtn.Text = "Save Server";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(30, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(176, 44);
            this.label4.TabIndex = 8;
            this.label4.Text = "Add a new server to the service. Note: Requires a Global restart to run new serve" +
    "r!";
            // 
            // AddNewServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 226);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.editPropsBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ipV6Box);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ipV4Box);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.srvNameBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddNewServerForm";
            this.ShowInTaskbar = false;
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
    }
}