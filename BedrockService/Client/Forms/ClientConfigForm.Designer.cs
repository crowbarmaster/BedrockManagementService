
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
            this.scrollLockCheckbox = new System.Windows.Forms.CheckBox();
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
            this.serverGridView.Location = new System.Drawing.Point(13, 37);
            this.serverGridView.Name = "serverGridView";
            this.serverGridView.RowHeadersWidth = 62;
            this.serverGridView.RowTemplate.Height = 28;
            this.serverGridView.Size = new System.Drawing.Size(1123, 307);
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
            this.nbtPathLabel.Location = new System.Drawing.Point(13, 377);
            this.nbtPathLabel.Name = "nbtPathLabel";
            this.nbtPathLabel.Size = new System.Drawing.Size(144, 25);
            this.nbtPathLabel.TabIndex = 1;
            this.nbtPathLabel.Text = "NBT Studio path:";
            // 
            // nbtButton
            // 
            this.nbtButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nbtButton.Location = new System.Drawing.Point(17, 422);
            this.nbtButton.Name = "nbtButton";
            this.nbtButton.Size = new System.Drawing.Size(233, 47);
            this.nbtButton.TabIndex = 2;
            this.nbtButton.Text = "Set NBT Studio path";
            this.nbtButton.UseVisualStyleBackColor = true;
            this.nbtButton.Click += new System.EventHandler(this.nbtButton_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Location = new System.Drawing.Point(903, 422);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(233, 47);
            this.saveBtn.TabIndex = 3;
            this.saveBtn.Text = "Save settings";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // scrollLockCheckbox
            // 
            this.scrollLockCheckbox.AutoSize = true;
            this.scrollLockCheckbox.Location = new System.Drawing.Point(275, 432);
            this.scrollLockCheckbox.Name = "scrollLockCheckbox";
            this.scrollLockCheckbox.Size = new System.Drawing.Size(325, 29);
            this.scrollLockCheckbox.TabIndex = 4;
            this.scrollLockCheckbox.Text = "Set scrollbar lock enabled by default";
            this.scrollLockCheckbox.UseVisualStyleBackColor = true;
            // 
            // ClientConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1149, 510);
            this.Controls.Add(this.scrollLockCheckbox);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.nbtButton);
            this.Controls.Add(this.nbtPathLabel);
            this.Controls.Add(this.serverGridView);
            this.MinimumSize = new System.Drawing.Size(1162, 538);
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
        private System.Windows.Forms.CheckBox scrollLockCheckbox;
    }
}