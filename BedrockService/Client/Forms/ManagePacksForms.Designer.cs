
namespace BedrockService.Client.Forms
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
            this.serverListBox = new System.Windows.Forms.ListBox();
            this.parsedPacksListBox = new System.Windows.Forms.ListBox();
            this.sendPacksBtn = new System.Windows.Forms.Button();
            this.sendAllBtn = new System.Windows.Forms.Button();
            this.removePackBtn = new System.Windows.Forms.Button();
            this.removeAllPacksBtn = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.selectedPackIcon = new System.Windows.Forms.PictureBox();
            this.openFileBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.selectedPackIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // serverListBox
            // 
            this.serverListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.serverListBox.FormattingEnabled = true;
            this.serverListBox.Location = new System.Drawing.Point(12, 82);
            this.serverListBox.Name = "serverListBox";
            this.serverListBox.Size = new System.Drawing.Size(189, 251);
            this.serverListBox.TabIndex = 0;
            this.serverListBox.Click += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // parsedPacksListBox
            // 
            this.parsedPacksListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parsedPacksListBox.FormattingEnabled = true;
            this.parsedPacksListBox.Location = new System.Drawing.Point(599, 82);
            this.parsedPacksListBox.Name = "parsedPacksListBox";
            this.parsedPacksListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.parsedPacksListBox.Size = new System.Drawing.Size(189, 251);
            this.parsedPacksListBox.TabIndex = 1;
            this.parsedPacksListBox.Click += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // sendPacksBtn
            // 
            this.sendPacksBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendPacksBtn.Location = new System.Drawing.Point(468, 82);
            this.sendPacksBtn.Name = "sendPacksBtn";
            this.sendPacksBtn.Size = new System.Drawing.Size(125, 23);
            this.sendPacksBtn.TabIndex = 2;
            this.sendPacksBtn.Text = "Send selected packs";
            this.sendPacksBtn.UseVisualStyleBackColor = true;
            this.sendPacksBtn.Click += new System.EventHandler(this.sendPacksBtn_Click);
            // 
            // sendAllBtn
            // 
            this.sendAllBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendAllBtn.Location = new System.Drawing.Point(468, 111);
            this.sendAllBtn.Name = "sendAllBtn";
            this.sendAllBtn.Size = new System.Drawing.Size(125, 23);
            this.sendAllBtn.TabIndex = 3;
            this.sendAllBtn.Text = "Send all packs";
            this.sendAllBtn.UseVisualStyleBackColor = true;
            this.sendAllBtn.Click += new System.EventHandler(this.sendAllBtn_Click);
            // 
            // removePackBtn
            // 
            this.removePackBtn.Location = new System.Drawing.Point(207, 82);
            this.removePackBtn.Name = "removePackBtn";
            this.removePackBtn.Size = new System.Drawing.Size(119, 23);
            this.removePackBtn.TabIndex = 4;
            this.removePackBtn.Text = "Remove pack";
            this.removePackBtn.UseVisualStyleBackColor = true;
            this.removePackBtn.Click += new System.EventHandler(this.removePackBtn_Click);
            // 
            // removeAllPacksBtn
            // 
            this.removeAllPacksBtn.Location = new System.Drawing.Point(207, 111);
            this.removeAllPacksBtn.Name = "removeAllPacksBtn";
            this.removeAllPacksBtn.Size = new System.Drawing.Size(119, 23);
            this.removeAllPacksBtn.TabIndex = 5;
            this.removeAllPacksBtn.Text = "Remove all packs";
            this.removeAllPacksBtn.UseVisualStyleBackColor = true;
            this.removeAllPacksBtn.Click += new System.EventHandler(this.removeAllPacksBtn_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(288, 211);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(219, 122);
            this.textBox1.TabIndex = 6;
            // 
            // selectedPackIcon
            // 
            this.selectedPackIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectedPackIcon.Location = new System.Drawing.Point(332, 75);
            this.selectedPackIcon.Name = "selectedPackIcon";
            this.selectedPackIcon.Size = new System.Drawing.Size(130, 130);
            this.selectedPackIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.selectedPackIcon.TabIndex = 7;
            this.selectedPackIcon.TabStop = false;
            // 
            // openFileBtn
            // 
            this.openFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openFileBtn.Location = new System.Drawing.Point(599, 29);
            this.openFileBtn.Name = "openFileBtn";
            this.openFileBtn.Size = new System.Drawing.Size(189, 23);
            this.openFileBtn.TabIndex = 8;
            this.openFileBtn.Text = "Open pack file(s)";
            this.openFileBtn.UseVisualStyleBackColor = true;
            this.openFileBtn.Click += new System.EventHandler(this.openFileBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Current packs found on server:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(596, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Packs found in archive file(s):";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(204, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(418, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "!! NOTICE !! Send map pack alone, do not send all! Not all packs will parse serve" +
    "r-side!";
            // 
            // ManagePacksForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.openFileBtn);
            this.Controls.Add(this.selectedPackIcon);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.removeAllPacksBtn);
            this.Controls.Add(this.removePackBtn);
            this.Controls.Add(this.sendAllBtn);
            this.Controls.Add(this.sendPacksBtn);
            this.Controls.Add(this.parsedPacksListBox);
            this.Controls.Add(this.serverListBox);
            this.Name = "ManagePacksForms";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.selectedPackIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox serverListBox;
        private System.Windows.Forms.ListBox parsedPacksListBox;
        private System.Windows.Forms.Button sendPacksBtn;
        private System.Windows.Forms.Button sendAllBtn;
        private System.Windows.Forms.Button removePackBtn;
        private System.Windows.Forms.Button removeAllPacksBtn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox selectedPackIcon;
        private System.Windows.Forms.Button openFileBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}