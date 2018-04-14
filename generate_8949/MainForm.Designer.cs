namespace generate_8949
{
    partial class MainForm
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
            this.SelectFileButton = new System.Windows.Forms.Button();
            this.SourceDataFileName = new System.Windows.Forms.TextBox();
            this.StartCalc = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SelectFileButton
            // 
            this.SelectFileButton.Location = new System.Drawing.Point(12, 12);
            this.SelectFileButton.Name = "SelectFileButton";
            this.SelectFileButton.Size = new System.Drawing.Size(25, 23);
            this.SelectFileButton.TabIndex = 0;
            this.SelectFileButton.Text = "...";
            this.SelectFileButton.UseVisualStyleBackColor = true;
            this.SelectFileButton.Click += new System.EventHandler(this.SelectFileButton_Click);
            // 
            // SourceDataFileName
            // 
            this.SourceDataFileName.Location = new System.Drawing.Point(43, 14);
            this.SourceDataFileName.Name = "SourceDataFileName";
            this.SourceDataFileName.Size = new System.Drawing.Size(237, 20);
            this.SourceDataFileName.TabIndex = 1;
            // 
            // StartCalc
            // 
            this.StartCalc.Location = new System.Drawing.Point(107, 40);
            this.StartCalc.Name = "StartCalc";
            this.StartCalc.Size = new System.Drawing.Size(75, 23);
            this.StartCalc.TabIndex = 2;
            this.StartCalc.Text = "Calculate";
            this.StartCalc.UseVisualStyleBackColor = true;
            this.StartCalc.Click += new System.EventHandler(this.StartCalc_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 70);
            this.Controls.Add(this.StartCalc);
            this.Controls.Add(this.SourceDataFileName);
            this.Controls.Add(this.SelectFileButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Form 8949 Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SelectFileButton;
        private System.Windows.Forms.TextBox SourceDataFileName;
        private System.Windows.Forms.Button StartCalc;
    }
}

