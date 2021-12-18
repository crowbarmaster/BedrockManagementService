
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
            this.serverGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.serverGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.serverGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.HostName,
            this.HostAddress,
            this.Port});
            this.serverGridView.Location = new System.Drawing.Point(9, 22);
            this.serverGridView.Margin = new System.Windows.Forms.Padding(2);
            this.serverGridView.Name = "serverGridView";
            this.serverGridView.RowHeadersWidth = 62;
            this.serverGridView.RowTemplate.Height = 28;
            this.serverGridView.Size = new System.Drawing.Size(786, 184);
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
            this.nbtPathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nbtPathLabel.AutoSize = true;
            this.nbtPathLabel.Location = new System.Drawing.Point(9, 226);
            this.nbtPathLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nbtPathLabel.Name = "nbtPathLabel";
            this.nbtPathLabel.Size = new System.Drawing.Size(95, 15);
            this.nbtPathLabel.TabIndex = 1;
            this.nbtPathLabel.Text = "NBT Studio path:";
            // 
            // nbtButton
            // 
            this.nbtButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nbtButton.Location = new System.Drawing.Point(12, 253);
            this.nbtButton.Margin = new System.Windows.Forms.Padding(2);
            this.nbtButton.Name = "nbtButton";
            this.nbtButton.Size = new System.Drawing.Size(163, 28);
            this.nbtButton.TabIndex = 2;
            this.nbtButton.Text = "Set NBT Studio path";
            this.nbtButton.UseVisualStyleBackColor = true;
            this.nbtButton.Click += new System.EventHandler(this.nbtButton_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(632, 253);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(2);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(163, 28);
            this.saveBtn.TabIndex = 3;
            this.saveBtn.Text = "Save settings";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // ClientConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 306);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.nbtButton);
            this.Controls.Add(this.nbtPathLabel);
            this.Controls.Add(this.serverGridView);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(820, 345);
            this.Name = "ClientConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Client Configuration";
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