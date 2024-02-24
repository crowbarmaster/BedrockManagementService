
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
            this.checkAllLocalButton = new System.Windows.Forms.Button();
            this.uncheckAllServerButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.openFileBtn = new System.Windows.Forms.Button();
            this.selectedPackIcon = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.checkAllServerButton = new System.Windows.Forms.Button();
            this.removePackBtn = new System.Windows.Forms.Button();
            this.uncheckAllLocalButton = new System.Windows.Forms.Button();
            this.sendPacksBtn = new System.Windows.Forms.Button();
            this.parsedPacksListBox = new System.Windows.Forms.CheckedListBox();
            this.serverListBox = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.selectedPackIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // checkAllLocalButton
            // 
            this.checkAllLocalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkAllLocalButton.Location = new System.Drawing.Point(585, 123);
            this.checkAllLocalButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkAllLocalButton.Name = "checkAllLocalButton";
            this.checkAllLocalButton.Size = new System.Drawing.Size(133, 27);
            this.checkAllLocalButton.TabIndex = 40;
            this.checkAllLocalButton.Text = "Check all boxes";
            this.checkAllLocalButton.UseVisualStyleBackColor = true;
            this.checkAllLocalButton.Click += new System.EventHandler(this.checkAllLocalButton_Click);
            // 
            // uncheckAllServerButton
            // 
            this.uncheckAllServerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uncheckAllServerButton.Location = new System.Drawing.Point(278, 156);
            this.uncheckAllServerButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.uncheckAllServerButton.Name = "uncheckAllServerButton";
            this.uncheckAllServerButton.Size = new System.Drawing.Size(133, 27);
            this.uncheckAllServerButton.TabIndex = 39;
            this.uncheckAllServerButton.Text = "Uncheck all boxes";
            this.uncheckAllServerButton.UseVisualStyleBackColor = true;
            this.uncheckAllServerButton.Click += new System.EventHandler(this.uncheckAllServerButton_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 48);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(454, 15);
            this.label3.TabIndex = 38;
            this.label3.Text = "!! NOTICE !! Send map pack alone, do not send all! Not all packs will parse serve" +
    "r-side!";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(722, 91);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(161, 15);
            this.label2.TabIndex = 37;
            this.label2.Text = "Packs found in archive file(s):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 91);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 15);
            this.label1.TabIndex = 36;
            this.label1.Text = "Current packs found on server:";
            // 
            // openFileBtn
            // 
            this.openFileBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openFileBtn.Location = new System.Drawing.Point(726, 48);
            this.openFileBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.openFileBtn.Name = "openFileBtn";
            this.openFileBtn.Size = new System.Drawing.Size(220, 27);
            this.openFileBtn.TabIndex = 35;
            this.openFileBtn.Text = "Open pack file(s)";
            this.openFileBtn.UseVisualStyleBackColor = true;
            this.openFileBtn.Click += new System.EventHandler(this.openFileBtn_Click);
            // 
            // selectedPackIcon
            // 
            this.selectedPackIcon.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.selectedPackIcon.Location = new System.Drawing.Point(419, 102);
            this.selectedPackIcon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.selectedPackIcon.Name = "selectedPackIcon";
            this.selectedPackIcon.Size = new System.Drawing.Size(158, 150);
            this.selectedPackIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.selectedPackIcon.TabIndex = 34;
            this.selectedPackIcon.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.textBox1.Location = new System.Drawing.Point(368, 258);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(255, 140);
            this.textBox1.TabIndex = 33;
            // 
            // checkAllServerButton
            // 
            this.checkAllServerButton.Location = new System.Drawing.Point(278, 123);
            this.checkAllServerButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkAllServerButton.Name = "checkAllServerButton";
            this.checkAllServerButton.Size = new System.Drawing.Size(133, 27);
            this.checkAllServerButton.TabIndex = 32;
            this.checkAllServerButton.Text = "Check all boxes";
            this.checkAllServerButton.UseVisualStyleBackColor = true;
            this.checkAllServerButton.Click += new System.EventHandler(this.checkAllServerButton_Click);
            // 
            // removePackBtn
            // 
            this.removePackBtn.Location = new System.Drawing.Point(80, 390);
            this.removePackBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.removePackBtn.Name = "removePackBtn";
            this.removePackBtn.Size = new System.Drawing.Size(139, 27);
            this.removePackBtn.TabIndex = 31;
            this.removePackBtn.Text = "Remove selected packs";
            this.removePackBtn.UseVisualStyleBackColor = true;
            this.removePackBtn.Click += new System.EventHandler(this.removePacksBtn_Click);
            // 
            // uncheckAllLocalButton
            // 
            this.uncheckAllLocalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uncheckAllLocalButton.Location = new System.Drawing.Point(585, 156);
            this.uncheckAllLocalButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.uncheckAllLocalButton.Name = "uncheckAllLocalButton";
            this.uncheckAllLocalButton.Size = new System.Drawing.Size(133, 27);
            this.uncheckAllLocalButton.TabIndex = 30;
            this.uncheckAllLocalButton.Text = "Uncheck all boxes";
            this.uncheckAllLocalButton.UseVisualStyleBackColor = true;
            this.uncheckAllLocalButton.Click += new System.EventHandler(this.uncheckAllLocalButton_Click);
            // 
            // sendPacksBtn
            // 
            this.sendPacksBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendPacksBtn.Location = new System.Drawing.Point(765, 390);
            this.sendPacksBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.sendPacksBtn.Name = "sendPacksBtn";
            this.sendPacksBtn.Size = new System.Drawing.Size(146, 27);
            this.sendPacksBtn.TabIndex = 29;
            this.sendPacksBtn.Text = "Send selected packs";
            this.sendPacksBtn.UseVisualStyleBackColor = true;
            this.sendPacksBtn.Click += new System.EventHandler(this.sendPacksBtn_Click);
            // 
            // parsedPacksListBox
            // 
            this.parsedPacksListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parsedPacksListBox.FormattingEnabled = true;
            this.parsedPacksListBox.Location = new System.Drawing.Point(726, 110);
            this.parsedPacksListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.parsedPacksListBox.Name = "parsedPacksListBox";
            this.parsedPacksListBox.Size = new System.Drawing.Size(220, 274);
            this.parsedPacksListBox.TabIndex = 13;
            this.parsedPacksListBox.Click += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // serverListBox
            // 
            this.serverListBox.FormattingEnabled = true;
            this.serverListBox.Location = new System.Drawing.Point(50, 110);
            this.serverListBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serverListBox.Name = "serverListBox";
            this.serverListBox.Size = new System.Drawing.Size(220, 274);
            this.serverListBox.TabIndex = 27;
            this.serverListBox.Click += new System.EventHandler(this.ListBox_SelectedIndexChanged);
            // 
            // ManagePacksForms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 464);
            this.Controls.Add(this.checkAllLocalButton);
            this.Controls.Add(this.uncheckAllServerButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.openFileBtn);
            this.Controls.Add(this.selectedPackIcon);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkAllServerButton);
            this.Controls.Add(this.removePackBtn);
            this.Controls.Add(this.uncheckAllLocalButton);
            this.Controls.Add(this.sendPacksBtn);
            this.Controls.Add(this.parsedPacksListBox);
            this.Controls.Add(this.serverListBox);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(940, 444);
            this.Name = "ManagePacksForms";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pack Manager";
            ((System.ComponentModel.ISupportInitialize)(this.selectedPackIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button openFileBtn;
        private System.Windows.Forms.PictureBox selectedPackIcon;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button checkAllServerButton;
        private System.Windows.Forms.Button removePackBtn;
        private System.Windows.Forms.Button uncheckAllLocalButton;
        private System.Windows.Forms.Button sendPacksBtn;
        private System.Windows.Forms.CheckedListBox parsedPacksListBox;
        private System.Windows.Forms.CheckedListBox serverListBox;
        private System.Windows.Forms.Button uncheckAllServerButton;
        private System.Windows.Forms.Button checkAllLocalButton;
    }
}
