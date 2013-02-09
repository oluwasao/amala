namespace FeedImporter
{
    partial class Form1
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
            this.lstThisDay = new System.Windows.Forms.ListBox();
            this.btnGuardianImporter = new System.Windows.Forms.Button();
            this.btnScrapeAll = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstThisDay
            // 
            this.lstThisDay.FormattingEnabled = true;
            this.lstThisDay.Location = new System.Drawing.Point(143, 3);
            this.lstThisDay.Name = "lstThisDay";
            this.lstThisDay.Size = new System.Drawing.Size(271, 95);
            this.lstThisDay.TabIndex = 0;
            // 
            // btnGuardianImporter
            // 
            this.btnGuardianImporter.Location = new System.Drawing.Point(198, 104);
            this.btnGuardianImporter.Name = "btnGuardianImporter";
            this.btnGuardianImporter.Size = new System.Drawing.Size(75, 23);
            this.btnGuardianImporter.TabIndex = 1;
            this.btnGuardianImporter.Text = "Scrape";
            this.btnGuardianImporter.UseVisualStyleBackColor = true;
            this.btnGuardianImporter.Click += new System.EventHandler(this.btnGuardianImporter_Click);
            // 
            // btnScrapeAll
            // 
            this.btnScrapeAll.Location = new System.Drawing.Point(295, 104);
            this.btnScrapeAll.Name = "btnScrapeAll";
            this.btnScrapeAll.Size = new System.Drawing.Size(75, 23);
            this.btnScrapeAll.TabIndex = 2;
            this.btnScrapeAll.Text = "Scrape All";
            this.btnScrapeAll.UseVisualStyleBackColor = true;
            this.btnScrapeAll.Click += new System.EventHandler(this.btnScrapeAll_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(0, 145);
            this.lblStatus.MinimumSize = new System.Drawing.Size(517, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblStatus.Size = new System.Drawing.Size(517, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lstThisDay);
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Controls.Add(this.btnGuardianImporter);
            this.panel1.Controls.Add(this.btnScrapeAll);
            this.panel1.Location = new System.Drawing.Point(86, 56);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(563, 215);
            this.panel1.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 331);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstThisDay;
        private System.Windows.Forms.Button btnGuardianImporter;
        private System.Windows.Forms.Button btnScrapeAll;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panel1;
    }
}

