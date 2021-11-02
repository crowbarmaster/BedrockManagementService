
namespace BedrockService.Client.Forms
{
    partial class ClientConfigForm
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
            this.serverGridView = new System.Windows.Forms.DataGridView();
            this.HostName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.HostAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Port = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nbtPathLabel = new System.Windows.Forms.Label();
            this.nbtButton = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.serverGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // serverGridView
            // 
            this.serverGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.serverGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.HostName,
            this.HostAddress,
            this.Port});
            this.serverGridView.Location = new System.Drawing.Point(12, 30);
            this.serverGridView.Name = "serverGridView";
            this.serverGridView.RowHeadersWidth = 62;
            this.serverGridView.RowTemplate.Height = 28;
            this.serverGridView.Size = new System.Drawing.Size(1154, 381);
            this.serverGridView.TabIndex = 0;
            // 
            // HostName
            // 
            this.HostName.HeaderText = "Host Name";
            this.HostName.MinimumWidth = 25;
            this.HostName.Name = "HostName";
            this.HostName.Width = 250;
            // 
            // HostAddress
            // 
            this.HostAddress.HeaderText = "IP Address";
            this.HostAddress.MinimumWidth = 8;
            this.HostAddress.Name = "HostAddress";
            this.HostAddress.Width = 250;
            // 
            // Port
            // 
            this.Port.HeaderText = "IP Port";
            this.Port.MinimumWidth = 8;
            this.Port.Name = "Port";
            this.Port.Width = 200;
            // 
            // nbtPathLabel
            // 
            this.nbtPathLabel.AutoSize = true;
            this.nbtPathLabel.Location = new System.Drawing.Point(12, 437);
            this.nbtPathLabel.Name = "nbtPathLabel";
            this.nbtPathLabel.Size = new System.Drawing.Size(130, 20);
            this.nbtPathLabel.TabIndex = 1;
            this.nbtPathLabel.Text = "NBT Studio path:";
            // 
            // nbtButton
            // 
            this.nbtButton.Location = new System.Drawing.Point(16, 473);
            this.nbtButton.Name = "nbtButton";
            this.nbtButton.Size = new System.Drawing.Size(209, 37);
            this.nbtButton.TabIndex = 2;
            this.nbtButton.Text = "Set NBT Studio path";
            this.nbtButton.UseVisualStyleBackColor = true;
            this.nbtButton.Click += new System.EventHandler(this.nbtButton_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(957, 473);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(209, 37);
            this.saveBtn.TabIndex = 3;
            this.saveBtn.Text = "Save settings";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // ClientConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1178, 544);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.nbtButton);
            this.Controls.Add(this.nbtPathLabel);
            this.Controls.Add(this.serverGridView);
            this.Name = "ClientConfigForm";
            this.Text = "ClientConfigForm";
            ((System.ComponentModel.ISupportInitialize)(this.serverGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView serverGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostName;
        private System.Windows.Forms.DataGridViewTextBoxColumn HostAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn Port;
        private System.Windows.Forms.Label nbtPathLabel;
        private System.Windows.Forms.Button nbtButton;
        private System.Windows.Forms.Button saveBtn;
    }
}