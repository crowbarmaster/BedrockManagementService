
namespace BedrockService.Client.Forms
{
    partial class PropEditorForm
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
            this.CancelBtn = new System.Windows.Forms.Button();
            this.SaveBtn = new System.Windows.Forms.Button();
            this.gridView = new System.Windows.Forms.DataGridView();
            this.EntryKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EntryData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DelBackupBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            this.CancelBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CancelBtn.Location = new System.Drawing.Point(27, 628);
            this.CancelBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(333, 45);
            this.CancelBtn.TabIndex = 48;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // SaveBtn
            // 
            this.SaveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.SaveBtn.Location = new System.Drawing.Point(936, 628);
            this.SaveBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.SaveBtn.Name = "SaveBtn";
            this.SaveBtn.Size = new System.Drawing.Size(333, 45);
            this.SaveBtn.TabIndex = 49;
            this.SaveBtn.Text = "Save";
            this.SaveBtn.UseVisualStyleBackColor = true;
            this.SaveBtn.Click += new System.EventHandler(this.SaveBtn_Click);
            // 
            // gridView
            // 
            this.gridView.AllowUserToDeleteRows = false;
            this.gridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EntryKey,
            this.EntryData});
            this.gridView.Location = new System.Drawing.Point(21, 25);
            this.gridView.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.gridView.MultiSelect = false;
            this.gridView.Name = "gridView";
            this.gridView.RowHeadersWidth = 62;
            this.gridView.Size = new System.Drawing.Size(1248, 560);
            this.gridView.TabIndex = 1;
            this.gridView.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.gridView_NewRowNeeded);
            // 
            // EntryKey
            // 
            this.EntryKey.HeaderText = "Entry:";
            this.EntryKey.MinimumWidth = 20;
            this.EntryKey.Name = "EntryKey";
            this.EntryKey.ReadOnly = true;
            this.EntryKey.Width = 300;
            // 
            // EntryData
            // 
            this.EntryData.HeaderText = "Value:";
            this.EntryData.MinimumWidth = 8;
            this.EntryData.Name = "EntryData";
            this.EntryData.Width = 500;
            // 
            // DelBackupBtn
            // 
            this.DelBackupBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.DelBackupBtn.Enabled = false;
            this.DelBackupBtn.Location = new System.Drawing.Point(488, 628);
            this.DelBackupBtn.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.DelBackupBtn.Name = "DelBackupBtn";
            this.DelBackupBtn.Size = new System.Drawing.Size(333, 45);
            this.DelBackupBtn.TabIndex = 50;
            this.DelBackupBtn.Text = "Delete Backup";
            this.DelBackupBtn.UseVisualStyleBackColor = true;
            this.DelBackupBtn.Visible = false;
            this.DelBackupBtn.Click += new System.EventHandler(this.DelBackupBtn_Click);
            // 
            // PropEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1344, 766);
            this.Controls.Add(this.DelBackupBtn);
            this.Controls.Add(this.gridView);
            this.Controls.Add(this.SaveBtn);
            this.Controls.Add(this.CancelBtn);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "PropEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button CancelBtn;
        private System.Windows.Forms.Button SaveBtn;
        private System.Windows.Forms.DataGridView gridView;
        private System.Windows.Forms.Button DelBackupBtn;
        private System.Windows.Forms.DataGridViewTextBoxColumn EntryKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn EntryData;
    }
}