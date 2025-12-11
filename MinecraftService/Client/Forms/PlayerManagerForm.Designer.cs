
namespace MinecraftService.Client.Forms
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
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            var dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            searchEntryBox = new System.Windows.Forms.TextBox();
            saveBtn = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            gridView = new System.Windows.Forms.DataGridView();
            xuidColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            usernameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            permColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            whitelistColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ignorePlayerColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            firstConnColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            lastConnColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            disconnTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            registerPlayerBtn = new System.Windows.Forms.Button();
            entryTextToolTip = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            SuspendLayout();
            // 
            // searchEntryBox
            // 
            searchEntryBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            searchEntryBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            searchEntryBox.Location = new System.Drawing.Point(206, 349);
            searchEntryBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchEntryBox.Name = "searchEntryBox";
            searchEntryBox.Size = new System.Drawing.Size(629, 26);
            searchEntryBox.TabIndex = 4;
            searchEntryBox.TextChanged += searchEntryBox_TextChanged;
            // 
            // saveBtn
            // 
            saveBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            saveBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            saveBtn.Location = new System.Drawing.Point(843, 349);
            saveBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new System.Drawing.Size(102, 30);
            saveBtn.TabIndex = 5;
            saveBtn.Text = "Save";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += saveBtn_Click;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            label2.Location = new System.Drawing.Point(148, 352);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(44, 20);
            label2.TabIndex = 6;
            label2.Text = "Find:";
            // 
            // gridView
            // 
            gridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            gridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            gridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { xuidColumn, usernameColumn, permColumn, whitelistColumn, ignorePlayerColumn, firstConnColumn, lastConnColumn, disconnTimeColumn });
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            gridView.DefaultCellStyle = dataGridViewCellStyle2;
            gridView.Location = new System.Drawing.Point(19, 31);
            gridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gridView.Name = "gridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            gridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            gridView.Size = new System.Drawing.Size(926, 289);
            gridView.TabIndex = 3;
            gridView.CellClick += gridView_CellContentClick;
            gridView.CellEndEdit += gridView_CellEndEdit;
            gridView.CellStateChanged += gridView_CellStateChanged;
            // 
            // xuidColumn
            // 
            xuidColumn.HeaderText = "XUID";
            xuidColumn.Name = "xuidColumn";
            xuidColumn.ReadOnly = true;
            xuidColumn.Width = 58;
            // 
            // usernameColumn
            // 
            usernameColumn.HeaderText = "Username";
            usernameColumn.Name = "usernameColumn";
            usernameColumn.Width = 80;
            // 
            // permColumn
            // 
            permColumn.HeaderText = "Permission";
            permColumn.Items.AddRange(new object[] { "visitor", "member", "operator" });
            permColumn.MaxDropDownItems = 4;
            permColumn.Name = "permColumn";
            permColumn.Width = 63;
            // 
            // whitelistColumn
            // 
            whitelistColumn.FalseValue = "False";
            whitelistColumn.HeaderText = "Whitelisted";
            whitelistColumn.Name = "whitelistColumn";
            whitelistColumn.TrueValue = "True";
            whitelistColumn.Width = 65;
            // 
            // ignorePlayerColumn
            // 
            ignorePlayerColumn.FalseValue = "False";
            ignorePlayerColumn.HeaderText = "Ignores max players";
            ignorePlayerColumn.Name = "ignorePlayerColumn";
            ignorePlayerColumn.TrueValue = "True";
            ignorePlayerColumn.Width = 96;
            // 
            // firstConnColumn
            // 
            firstConnColumn.HeaderText = "First connected on";
            firstConnColumn.Name = "firstConnColumn";
            firstConnColumn.ReadOnly = true;
            firstConnColumn.Width = 99;
            // 
            // lastConnColumn
            // 
            lastConnColumn.HeaderText = "Last connected on";
            lastConnColumn.Name = "lastConnColumn";
            lastConnColumn.ReadOnly = true;
            // 
            // disconnTimeColumn
            // 
            disconnTimeColumn.HeaderText = "Time spent in game";
            disconnTimeColumn.Name = "disconnTimeColumn";
            disconnTimeColumn.ReadOnly = true;
            disconnTimeColumn.Width = 90;
            // 
            // registerPlayerBtn
            // 
            registerPlayerBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            registerPlayerBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            registerPlayerBtn.Location = new System.Drawing.Point(19, 348);
            registerPlayerBtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            registerPlayerBtn.Name = "registerPlayerBtn";
            registerPlayerBtn.Size = new System.Drawing.Size(122, 30);
            registerPlayerBtn.TabIndex = 7;
            registerPlayerBtn.Text = "Register new...";
            registerPlayerBtn.UseVisualStyleBackColor = true;
            registerPlayerBtn.Click += registerPlayerBtn_Click;
            // 
            // PlayerManagerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(964, 401);
            Controls.Add(registerPlayerBtn);
            Controls.Add(gridView);
            Controls.Add(label2);
            Controls.Add(saveBtn);
            Controls.Add(searchEntryBox);
            Margin = new System.Windows.Forms.Padding(2);
            MinimumSize = new System.Drawing.Size(980, 440);
            Name = "PlayerManagerForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Player Manager";
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ResumeLayout(false);
            PerformLayout();

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
