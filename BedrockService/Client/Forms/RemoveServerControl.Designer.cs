namespace BedrockService.Client.Forms {
    partial class RemoveServerControl {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
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
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(28, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 55);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select options for server removal, and click \"Confirm\" to proceed.";
            // 
            // RemoveServerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 261);
            this.Controls.Add(this.label1);
            this.Name = "RemoveServerControl";
            this.Text = "Server removal prompt";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
    }
}