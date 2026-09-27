using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace M365Debloater
{
    public partial class MainForm : Form
    {
        private string _product;
        private string _architecture;
        private string _channel;
        private string[] _existingExclusions = new string[0];
        private string _odtSetupPath;
        private bool _ready;
        private bool _running;

        public MainForm()
        {
            InitializeComponent();
            clbApps.Items.AddRange(OfficeConfiguration.Apps);
            clbApps.ItemCheck += (sender, e) =>
            {
                // ItemCheck runs before CheckedItems contains the new value.
                BeginInvoke(new Action(UpdateSelection));
            };
            Load += async (sender, e) => await DetectOfficeAsync();
            FormClosing += (sender, e) =>
            {
                if (!_running || e.CloseReason != CloseReason.UserClosing) return;
                e.Cancel = true;
                SetStatus("Office preparation or setup is still running. Please wait before closing.", true);
            };
        }

        private async Task DetectOfficeAsync()
        {
            if (_running) return;
            _running = true;
            _ready = false;
            UpdateSelection();
            _product = null;
            _existingExclusions = new string[0];
            lblInstallation.Text = "No supported installation detected.";
            pbProgress.Value = 0;
            try
            {
                const string path = @"SOFTWARE\Microsoft\Office\ClickToRun\Configuration";
                using (var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                    Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
                using (var key = machine.OpenSubKey(path, false))
                {
                    if (key == null)
                        throw new InvalidOperationException("Office Click-to-Run was not found. Install Office, then refresh detection.");

                    _product = OfficeConfiguration.DetectProduct(key.GetValue("ProductReleaseIds") as string);
                    if (_product == null)
                        throw new InvalidOperationException("The Office edition is unsupported or ambiguous. No changes can be applied.");

                    string platform = key.GetValue("Platform") as string;
                    _architecture = string.Equals(platform, "x86", StringComparison.OrdinalIgnoreCase) ? "32"
                        : string.Equals(platform, "x64", StringComparison.OrdinalIgnoreCase) ? "64" : null;
                    _channel = OfficeConfiguration.DetectChannel(_product,
                        key.GetValue("CDNBaseUrl") as string, key.GetValue("UpdateChannel") as string);
                    if (_architecture == null)
                        throw new InvalidOperationException("The Office architecture was not recognized. No changes can be applied.");
                    if (_channel == null)
                        throw new InvalidOperationException("The Office update channel was not recognized. No changes can be applied.");
                    _existingExclusions = OfficeConfiguration.SplitIds(key.GetValue(_product + ".ExcludedApps") as string);
                    // Validate the detected installation and exclusions before enabling Apply.
                    OfficeConfiguration.Create(_product, _architecture, _channel, new string[0], _existingExclusions);
                    string version = key.GetValue("VersionToReport") as string ?? "Unknown";
                    lblInstallation.Text = $"{_product}  ·  {_architecture}-bit  ·  {_channel}\nInstalled version: {version}";
                }

                _odtSetupPath = Path.Combine(Path.GetTempPath(), "odt", "setup.exe");
                if (!File.Exists(_odtSetupPath))
                {
                    SetStatus("Downloading and extracting Office Deployment Tool from Microsoft...");
                    pbProgress.Style = ProgressBarStyle.Marquee;
                    await PrepareOdtAsync();
                    if (!File.Exists(_odtSetupPath))
                        throw new InvalidOperationException("ODT preparation did not produce setup.exe. Use Refresh detection to retry.");
                }

                _ready = true;
                SetStatus("Ready. Select components to review your changes.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                _running = false;
                pbProgress.Style = ProgressBarStyle.Blocks;
            }
            UpdateSelection();
        }

        private static async Task PrepareOdtAsync()
        {
            string scriptPath = Path.Combine(Path.GetTempPath(), "M365Debloater-" + Guid.NewGuid().ToString("N") + ".ps1");
            try
            {
                using (var resource = typeof(MainForm).Assembly.GetManifestResourceStream("M365Debloater.PrepareOdt.ps1"))
                using (var file = File.Create(scriptPath))
                {
                    if (resource == null) throw new InvalidOperationException("The embedded ODT preparation script is missing.");
                    resource.CopyTo(file);
                }
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                            @"WindowsPowerShell\v1.0\powershell.exe"),
                        Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + scriptPath + "\" -PrepareOnly",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    process.Start();
                    var output = process.StandardOutput.ReadToEndAsync();
                    var error = process.StandardError.ReadToEndAsync();
                    await Task.Run(() => process.WaitForExit());
                    await output;
                    string details = await error;
                    if (process.ExitCode != 0)
                        throw new InvalidOperationException("Could not prepare ODT. Use Refresh detection to retry. " + details.Trim());
                }
            }
            finally
            {
                try { File.Delete(scriptPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        private OfficeApp[] SelectedApps()
        {
            return clbApps.CheckedItems.Cast<OfficeApp>().ToArray();
        }

        private void UpdateSelection()
        {
            var selected = SelectedApps();
            lblSelection.Text = selected.Length == 0 ? "No components selected"
                : selected.Length == 1 ? "1 component selected" : $"{selected.Length} components selected";
            txtSelection.Text = selected.Length == 0
                ? "Choose components from the list. Nothing changes until you review and confirm."
                : string.Join(Environment.NewLine, selected.Select(app => "• " + app.Name));
            if (_existingExclusions.Length > 0)
                txtSelection.Text += Environment.NewLine + Environment.NewLine
                    + "Existing exclusions retained: " + string.Join(", ", _existingExclusions);
            btnStart.Enabled = _ready && !_running && selected.Length > 0;
            btnClear.Enabled = !_running && selected.Length > 0;
            btnRefresh.Enabled = !_running;
            clbApps.Enabled = !_running;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbApps.Items.Count; i++) clbApps.SetItemChecked(i, false);
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await DetectOfficeAsync();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_running || !_ready || SelectedApps().Length == 0) return;
            // Recheck before confirmation: Office or its prerequisites may have changed.
            await DetectOfficeAsync();
            if (!_ready) return;
            var selected = SelectedApps();
            string summary = "Apply these exclusions to " + _product + "?\n\n"
                + string.Join("\n", selected.Select(app => "• " + app.Name))
                + (_existingExclusions.Length == 0 ? "" : "\n\nExisting exclusions retained: " + string.Join(", ", _existingExclusions))
                + "\n\nSave your work and close Office apps before continuing. "
                + "The detected version, architecture and channel will be requested. "
                + "Microsoft's license terms will be accepted by setup.";
            if (MessageBox.Show(this, summary, "Review Office changes", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) != DialogResult.OK) return;

            _running = true;
            UpdateSelection();
            btnStart.Text = "Applying changes…";
            pbProgress.Style = ProgressBarStyle.Marquee;
            pbProgress.MarqueeAnimationSpeed = 30;
            string xmlPath = null;
            try
            {
                xmlPath = Path.Combine(Path.GetTempPath(), "M365Debloater-" + Guid.NewGuid().ToString("N") + ".xml");
                OfficeConfiguration.Create(_product, _architecture, _channel,
                    selected.SelectMany(app => app.ExclusionIds), _existingExclusions).Save(xmlPath);
                SetStatus("Office setup is running. This may take several minutes; keep this window open.");
                int exitCode = await RunOdtAsync(xmlPath);
                if (exitCode == 0)
                {
                    // Capture the effective exclusions for any subsequent operation in this session.
                    _existingExclusions = _existingExclusions.Concat(selected.SelectMany(app => app.ExclusionIds))
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                    btnClear_Click(this, EventArgs.Empty);
                    SetStatus("Completed. Office setup reported success. Verify your Office apps before continuing.");
                    lblStatus.ForeColor = Color.FromArgb(21, 128, 61);
                    pbProgress.Style = ProgressBarStyle.Blocks;
                    pbProgress.Value = 100;
                }
                else
                {
                    SetStatus($"Office setup returned code {exitCode}. Check its logs in %TEMP% before retrying.", true);
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                SetStatus("Administrator approval was cancelled. Office setup was not started.");
            }
            catch (Exception ex)
            {
                SetStatus("Could not complete the operation: " + ex.Message, true);
            }
            finally
            {
                if (xmlPath != null)
                {
                    try { File.Delete(xmlPath); }
                    catch (IOException) { /* Temporary configuration can be removed later. */ }
                    catch (UnauthorizedAccessException) { /* Best-effort cleanup. */ }
                }
                pbProgress.MarqueeAnimationSpeed = 0;
                pbProgress.Style = ProgressBarStyle.Blocks;
                _running = false;
                btnStart.Text = "Review && &apply…";
                UpdateSelection();
            }
        }

        private Task<int> RunOdtAsync(string xmlPath)
        {
            return Task.Run(() =>
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = _odtSetupPath,
                    Arguments = "/configure \"" + xmlPath + "\"",
                    UseShellExecute = true,
                    Verb = "runas"
                }))
                {
                    if (process == null) throw new InvalidOperationException("Office setup could not be started.");
                    // Keep the operation locked until setup actually exits; a timeout is not completion.
                    process.WaitForExit();
                    return process.ExitCode;
                }
            });
        }

        private void SetStatus(string text, bool error = false)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = error ? Color.FromArgb(153, 27, 27) : Color.FromArgb(71, 85, 105);
        }
    }
}
