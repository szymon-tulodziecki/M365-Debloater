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

        #region Kod generowany przez Projektanta formularzy systemu Windows

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
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();

            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();

            // ── pnlHeader ─────────────────────────────────────────
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
            this.lblSubtitle.Text = "Wybierz składniki Microsoft 365 do usunięcia";
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(130, 138, 180);
            this.lblSubtitle.Location = new System.Drawing.Point(22, 50);
            this.lblSubtitle.Size = new System.Drawing.Size(380, 20);
            this.lblSubtitle.TabIndex = 1;

            // ── pnlDivider ────────────────────────────────────────
            this.pnlDivider.BackColor = System.Drawing.Color.FromArgb(60, 68, 120);
            this.pnlDivider.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDivider.Height = 1;

            // ── pnlMain ───────────────────────────────────────────
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(22, 25, 42);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Controls.Add(this.clbApps);
            this.pnlMain.Controls.Add(this.lblInstruction);

            this.lblInstruction.AutoSize = false;
            this.lblInstruction.Text = "KOMPONENTY DO USUNIĘCIA";
            this.lblInstruction.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblInstruction.ForeColor = System.Drawing.Color.FromArgb(90, 100, 160);
            this.lblInstruction.Location = new System.Drawing.Point(20, 16);
            this.lblInstruction.Size = new System.Drawing.Size(380, 16);
            this.lblInstruction.TabIndex = 0;

            // ── clbApps ───────────────────────────────────────────
            this.clbApps.CheckOnClick = true;
            this.clbApps.FormattingEnabled = true;
            this.clbApps.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.clbApps.BackColor = System.Drawing.Color.FromArgb(28, 32, 52);
            this.clbApps.ForeColor = System.Drawing.Color.FromArgb(205, 210, 240);
            this.clbApps.Font = new System.Drawing.Font("Segoe UI Emoji", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.clbApps.ItemHeight = 28;
            this.clbApps.Location = new System.Drawing.Point(20, 40);
            this.clbApps.Size = new System.Drawing.Size(380, 224);
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

            // ── pnlFooter ─────────────────────────────────────────
            this.pnlFooter.BackColor = System.Drawing.Color.FromArgb(22, 25, 42);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Height = 110;
            this.pnlFooter.Controls.Add(this.btnStart);
            this.pnlFooter.Controls.Add(this.pbProgress);
            this.pnlFooter.Controls.Add(this.lblStatus);

            // ── btnStart ──────────────────────────────────────────
            this.btnStart.Text = "USUŃ ZAZNACZONE SKŁADNIKI";
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

            // ── pbProgress ────────────────────────────────────────
            this.pbProgress.Location = new System.Drawing.Point(20, 64);
            this.pbProgress.Size = new System.Drawing.Size(380, 6);
            this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbProgress.BackColor = System.Drawing.Color.FromArgb(40, 44, 68);
            this.pbProgress.ForeColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.pbProgress.TabIndex = 1;

            // ── lblStatus ─────────────────────────────────────────
            this.lblStatus.AutoSize = false;
            this.lblStatus.Text = "Gotowy";
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(90, 100, 160);
            this.lblStatus.Location = new System.Drawing.Point(20, 78);
            this.lblStatus.Size = new System.Drawing.Size(380, 18);
            this.lblStatus.TabIndex = 2;

            // ── Form1 ─────────────────────────────────────────────
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
        private System.Windows.Forms.CheckedListBox clbApps;
        private System.Windows.Forms.Panel pnlFooter;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.Label lblStatus;
    }
}