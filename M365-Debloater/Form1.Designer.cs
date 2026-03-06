namespace M365Debloater
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.pnlDivider = new System.Windows.Forms.Panel();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.lblInstruction = new System.Windows.Forms.Label();
            this.clbApps = new System.Windows.Forms.CheckedListBox();
            this.pnlFooter = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnRunAgain = new System.Windows.Forms.Button();
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();

            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 34, 54);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 80;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);

            this.lblTitle.AutoSize = false;
            this.lblTitle.Text = "M365 Debloater";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(230, 230, 255);
            this.lblTitle.Location = new System.Drawing.Point(20, 12);
            this.lblTitle.Size = new System.Drawing.Size(380, 32);
            this.lblTitle.TabIndex = 0;

            this.lblSubtitle.AutoSize = false;
            this.lblSubtitle.Text = "Select Microsoft 365 components to remove";
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(130, 138, 180);
            this.lblSubtitle.Location = new System.Drawing.Point(22, 50);
            this.lblSubtitle.Size = new System.Drawing.Size(380, 20);
            this.lblSubtitle.TabIndex = 1;

            // pnlDivider
            this.pnlDivider.BackColor = System.Drawing.Color.FromArgb(60, 68, 120);
            this.pnlDivider.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDivider.Height = 1;

            // pnlMain
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(22, 25, 42);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Controls.Add(this.clbApps);
            this.pnlMain.Controls.Add(this.lblInstruction);
            this.pnlMain.Controls.Add(this.lblWarning);

            this.lblInstruction.AutoSize = false;
            this.lblInstruction.Text = "COMPONENTS TO REMOVE";
            this.lblInstruction.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblInstruction.ForeColor = System.Drawing.Color.FromArgb(90, 100, 160);
            this.lblInstruction.Location = new System.Drawing.Point(20, 16);
            this.lblInstruction.Size = new System.Drawing.Size(380, 16);
            this.lblInstruction.TabIndex = 0;

            // clbApps
            this.clbApps.CheckOnClick = true;
            this.clbApps.FormattingEnabled = true;
            this.clbApps.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbApps.BackColor = System.Drawing.Color.FromArgb(28, 32, 52);
            this.clbApps.ForeColor = System.Drawing.Color.FromArgb(205, 210, 240);
            this.clbApps.Font = new System.Drawing.Font("Segoe UI Emoji", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.clbApps.ItemHeight = 28;
            this.clbApps.Location = new System.Drawing.Point(20, 40);
            this.clbApps.Size = new System.Drawing.Size(380, 210);
            this.clbApps.TabIndex = 1;
            this.clbApps.Items.AddRange(new object[] {
                "\U0001F4AC   Skype for Business",
                "\U0001F465   Microsoft Teams",
                "\u2601\uFE0F   OneDrive",
                "\U0001F4E7   Outlook",
                "\U0001F4F0   Publisher",
                "\U0001F5C4\uFE0F   Access",
                "\U0001F4D3   OneNote",
                "\U0001F50D   Bing Search"
            });

            // lblWarning – stały komunikat pod listą (zamiast MessageBox)
            this.lblWarning.AutoSize = false;
            this.lblWarning.Text = "⚠  Do not close any windows or shut down your PC while Office is being reconfigured. This operation may take several minutes.";
            this.lblWarning.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblWarning.ForeColor = System.Drawing.Color.FromArgb(180, 160, 80);
            this.lblWarning.Location = new System.Drawing.Point(20, 258);
            this.lblWarning.Size = new System.Drawing.Size(380, 34);
            this.lblWarning.TabIndex = 2;

            // pnlFooter
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(22, 25, 42);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 110;
            this.pnlFooter.Controls.Add(this.btnRunAgain);
            this.pnlFooter.Controls.Add(this.btnStart);
            this.pnlFooter.Controls.Add(this.pbProgress);
            this.pnlFooter.Controls.Add(this.lblStatus);

            // btnStart
            this.btnStart.Text = "REMOVE SELECTED COMPONENTS";
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnStart.ForeColor = System.Drawing.Color.White;
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(124, 127, 255);
            this.btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(79, 82, 210);
            this.btnStart.Location = new System.Drawing.Point(20, 10);
            this.btnStart.Size = new System.Drawing.Size(380, 44);
            this.btnStart.TabIndex = 0;
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);

            // btnRunAgain – UKRYTY na start, pojawia się dopiero po zakończeniu
            this.btnRunAgain.Text = "🔄  RESTART PC";
            this.btnRunAgain.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnRunAgain.ForeColor = System.Drawing.Color.White;
            this.btnRunAgain.BackColor = System.Drawing.Color.FromArgb(34, 120, 80);
            this.btnRunAgain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunAgain.FlatAppearance.BorderSize = 0;
            this.btnRunAgain.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(46, 160, 105);
            this.btnRunAgain.Location = new System.Drawing.Point(20, 10);
            this.btnRunAgain.Size = new System.Drawing.Size(180, 44);
            this.btnRunAgain.TabIndex = 3;
            this.btnRunAgain.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRunAgain.UseVisualStyleBackColor = false;
            this.btnRunAgain.Visible = false;   // ukryty na start
            this.btnRunAgain.Click += new System.EventHandler(this.btnRunAgain_Click);

            // pbProgress
            this.pbProgress.Location = new System.Drawing.Point(20, 64);
            this.pbProgress.Size = new System.Drawing.Size(380, 6);
            this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbProgress.BackColor = System.Drawing.Color.FromArgb(40, 44, 68);
            this.pbProgress.ForeColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.pbProgress.TabIndex = 1;

            // lblStatus
            this.lblStatus.AutoSize = false;
            this.lblStatus.Text = "Ready";
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(90, 100, 160);
            this.lblStatus.Location = new System.Drawing.Point(20, 78);
            this.lblStatus.Size = new System.Drawing.Size(380, 18);
            this.lblStatus.TabIndex = 2;

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(22, 25, 42);
            this.ClientSize = new System.Drawing.Size(420, 500);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "M365 Debloater";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);

            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlDivider);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlFooter);

            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlDivider;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblInstruction;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.CheckedListBox clbApps;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnRunAgain;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.Label lblStatus;
    }
}