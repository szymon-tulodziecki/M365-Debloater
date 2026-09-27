using System.Drawing;
using System.Windows.Forms;

namespace M365Debloater
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private CheckedListBox clbApps;
        private Button btnStart;
        private Button btnClear;
        private Button btnRefresh;
        private Label lblInstallation;
        private Label lblSelection;
        private Label lblStatus;
        private TextBox txtSelection;
        private ProgressBar pbProgress;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.FromArgb(30, 41, 59);
            BackColor = Color.FromArgb(241, 245, 249);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(940, 740);
            MinimumSize = new Size(940, 740);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "M365 Debloater · Office configuration";

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var header = CreateStack(Color.FromArgb(15, 23, 42), new Padding(28, 20, 28, 20));
            header.Controls.Add(CreateLabel("M365 Debloater", 23F, Color.White, true));
            header.Controls.Add(CreateLabel("Configure your Office installation, one component at a time.", 10F, Color.FromArgb(203, 213, 225)));
            layout.Controls.Add(header, 0, 0);

            var installation = CreateStack(Color.White, new Padding(24, 16, 24, 16));
            installation.Margin = new Padding(24, 20, 24, 16);
            installation.Controls.Add(CreateLabel("01  /  YOUR INSTALLATION", 9F, Color.FromArgb(71, 85, 105), true));
            lblInstallation = CreateLabel("Checking the Office installation…", 10F, ForeColor);
            installation.Controls.Add(lblInstallation);
            layout.Controls.Add(installation, 0, 1);

            var workspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Margin = new Padding(24, 0, 24, 16)
            };
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var selection = CreateStack(Color.White, new Padding(20));
            selection.AutoSize = false;
            selection.Margin = new Padding(0, 0, 16, 0);
            selection.RowCount = 4;
            selection.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selection.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selection.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            selection.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            selection.Controls.Add(CreateLabel("02  /  CHOOSE COMPONENTS", 9F, Color.FromArgb(71, 85, 105), true), 0, 0);
            selection.Controls.Add(CreateLabel("Select the apps to exclude", 15F, ForeColor, true), 0, 1);
            clbApps = new CheckedListBox
            {
                Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false,
                BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = ForeColor,
                Font = new Font("Segoe UI", 12F), Margin = new Padding(0, 12, 0, 12),
                AccessibleName = "Office components to exclude", TabIndex = 0
            };
            selection.Controls.Add(clbApps, 0, 2);
            selection.Controls.Add(CreateLabel("These are configuration options, not a list of installed apps. Separately installed Teams or OneDrive may need separate removal.", 9F, Color.FromArgb(71, 85, 105)), 0, 3);
            workspace.Controls.Add(selection, 0, 0);

            var summary = CreateStack(Color.FromArgb(230, 239, 250), new Padding(20));
            summary.AutoSize = false;
            summary.RowCount = 4;
            summary.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summary.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summary.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            summary.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            summary.Controls.Add(CreateLabel("03  /  REVIEW CHANGES", 9F, Color.FromArgb(51, 65, 85), true), 0, 0);
            lblSelection = CreateLabel("No components selected", 15F, ForeColor, true);
            summary.Controls.Add(lblSelection, 0, 1);
            txtSelection = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
                BorderStyle = BorderStyle.None, ScrollBars = ScrollBars.Vertical,
                BackColor = summary.BackColor, ForeColor = ForeColor,
                Margin = new Padding(0, 12, 0, 12), AccessibleName = "Selection summary", TabIndex = 0
            };
            summary.Controls.Add(txtSelection, 0, 2);
            summary.Controls.Add(CreateLabel("Before applying\nSave your work and close Office apps. Setup can take several minutes and may need internet access.", 9F, Color.FromArgb(51, 65, 85)), 0, 3);
            workspace.Controls.Add(summary, 1, 0);
            layout.Controls.Add(workspace, 0, 2);

            var status = CreateStack(BackColor, new Padding(0));
            status.Margin = new Padding(24, 0, 24, 16);
            lblStatus = CreateLabel("Checking prerequisites…", 10F, Color.FromArgb(71, 85, 105));
            lblStatus.AccessibleName = "Operation status";
            status.Controls.Add(lblStatus);
            pbProgress = new ProgressBar { Dock = DockStyle.Top, Height = 6, Margin = new Padding(0, 8, 0, 0), TabStop = false };
            status.Controls.Add(pbProgress);
            layout.Controls.Add(status, 0, 3);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false, BackColor = Color.White, Padding = new Padding(24, 14, 24, 14),
                Margin = Padding.Empty
            };
            btnStart = CreateButton("Review && &apply…", true);
            btnStart.Enabled = false;
            btnStart.TabIndex = 0;
            btnStart.Click += btnStart_Click;
            btnClear = CreateButton("&Clear selection", false);
            btnClear.TabIndex = 1;
            btnClear.Click += btnClear_Click;
            btnRefresh = CreateButton("&Refresh detection", false);
            btnRefresh.TabIndex = 2;
            btnRefresh.Click += btnRefresh_Click;
            footer.Controls.AddRange(new Control[] { btnStart, btnClear, btnRefresh });
            layout.Controls.Add(footer, 0, 4);
            Controls.Add(layout);
            ResumeLayout(true);
        }

        private static TableLayoutPanel CreateStack(Color background, Padding padding)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1,
                BackColor = background, Padding = padding, Margin = Padding.Empty
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return panel;
        }

        private static Label CreateLabel(string text, float size, Color color, bool bold = false)
        {
            return new Label
            {
                Text = text, AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color, Margin = new Padding(0, 0, 0, 8)
            };
        }

        private static Button CreateButton(string text, bool primary)
        {
            var button = new Button
            {
                Text = text, AutoSize = true, MinimumSize = new Size(150, 42),
                Padding = new Padding(12, 6, 12, 6), Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                BackColor = primary ? Color.FromArgb(29, 78, 216) : Color.White,
                ForeColor = primary ? Color.White : Color.FromArgb(51, 65, 85),
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = primary ? button.BackColor : Color.FromArgb(203, 213, 225);
            return button;
        }
    }
}
