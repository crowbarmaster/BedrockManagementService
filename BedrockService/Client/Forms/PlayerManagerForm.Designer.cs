
namespace BedrockService.Client.Forms
{
    partial class PlayerManagerForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.searchEntryBox = new System.Windows.Forms.TextBox();
            this.saveBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.gridView = new System.Windows.Forms.DataGridView();
            this.xuidColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.usernameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.permColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.whitelistColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ignorePlayerColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.firstConnColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastConnColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.disconnTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.registerPlayerBtn = new System.Windows.Forms.Button();
            this.entryTextToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            this.SuspendLayout();
            // 
            // searchEntryBox
            // 
            this.searchEntryBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchEntryBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.searchEntryBox.Location = new System.Drawing.Point(206, 349);
            this.searchEntryBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.searchEntryBox.Name = "searchEntryBox";
            this.searchEntryBox.Size = new System.Drawing.Size(629, 26);
            this.searchEntryBox.TabIndex = 4;
            this.searchEntryBox.TextChanged += new System.EventHandler(this.searchEntryBox_TextChanged);
            // 
            // saveBtn
            // 
            this.saveBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.saveBtn.Location = new System.Drawing.Point(843, 349);
            this.saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(102, 30);
            this.saveBtn.TabIndex = 5;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.saveBtn_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(148, 352);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Find:";
            // 
            // gridView
            // 
            this.gridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.xuidColumn,
            this.usernameColumn,
            this.permColumn,
            this.whitelistColumn,
            this.ignorePlayerColumn,
            this.firstConnColumn,
            this.lastConnColumn,
            this.disconnTimeColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridView.Location = new System.Drawing.Point(19, 31);
            this.gridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gridView.Name = "gridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.gridView.Size = new System.Drawing.Size(926, 289);
            this.gridView.TabIndex = 3;
            // 
            // xuidColumn
            // 
            this.xuidColumn.HeaderText = "XUID";
            this.xuidColumn.Name = "xuidColumn";
            this.xuidColumn.ReadOnly = true;
            this.xuidColumn.Width = 58;
            // 
            // usernameColumn
            // 
            this.usernameColumn.HeaderText = "Username";
            this.usernameColumn.Name = "usernameColumn";
            this.usernameColumn.Width = 80;
            // 
            // permColumn
            // 
            this.permColumn.HeaderText = "Permission";
            this.permColumn.Items.AddRange(new object[] {
            "visitor",
            "member",
            "operator"});
            this.permColumn.MaxDropDownItems = 4;
            this.permColumn.Name = "permColumn";
            this.permColumn.Width = 63;
            // 
            // whitelistColumn
            // 
            this.whitelistColumn.FalseValue = "False";
            this.whitelistColumn.HeaderText = "Whitelisted";
            this.whitelistColumn.Name = "whitelistColumn";
            this.whitelistColumn.TrueValue = "True";
            this.whitelistColumn.Width = 65;
            // 
            // ignorePlayerColumn
            // 
            this.ignorePlayerColumn.FalseValue = "False";
            this.ignorePlayerColumn.HeaderText = "Ignores max players";
            this.ignorePlayerColumn.Name = "ignorePlayerColumn";
            this.ignorePlayerColumn.TrueValue = "True";
            this.ignorePlayerColumn.Width = 96;
            // 
            // firstConnColumn
            // 
            this.firstConnColumn.HeaderText = "First connected on";
            this.firstConnColumn.Name = "firstConnColumn";
            this.firstConnColumn.ReadOnly = true;
            this.firstConnColumn.Width = 99;
            // 
            // lastConnColumn
            // 
            this.lastConnColumn.HeaderText = "Last connected on";
            this.lastConnColumn.Name = "lastConnColumn";
            this.lastConnColumn.ReadOnly = true;
            // 
            // disconnTimeColumn
            // 
            this.disconnTimeColumn.HeaderText = "Time spent in game";
            this.disconnTimeColumn.Name = "disconnTimeColumn";
            this.disconnTimeColumn.ReadOnly = true;
            this.disconnTimeColumn.Width = 90;
            // 
            // registerPlayerBtn
            // 
            this.registerPlayerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.registerPlayerBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.registerPlayerBtn.Location = new System.Drawing.Point(19, 348);
            this.registerPlayerBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.registerPlayerBtn.Name = "registerPlayerBtn";
            this.registerPlayerBtn.Size = new System.Drawing.Size(122, 30);
            this.registerPlayerBtn.TabIndex = 7;
            this.registerPlayerBtn.Text = "Register new...";
            this.registerPlayerBtn.UseVisualStyleBackColor = true;
            this.registerPlayerBtn.Click += new System.EventHandler(this.registerPlayerBtn_Click);
            // 
            // PlayerManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 401);
            this.Controls.Add(this.registerPlayerBtn);
            this.Controls.Add(this.gridView);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.searchEntryBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(980, 440);
            this.Name = "PlayerManagerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Player Manager";
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox searchEntryBox;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView gridView;
        private System.Windows.Forms.Button registerPlayerBtn;
        private System.Windows.Forms.ToolTip entryTextToolTip;
        private System.Windows.Forms.DataGridViewTextBoxColumn xuidColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn usernameColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn permColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn whitelistColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ignorePlayerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn firstConnColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastConnColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn disconnTimeColumn;
    }
}