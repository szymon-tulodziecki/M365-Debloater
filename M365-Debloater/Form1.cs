using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;

namespace M365Debloater
{
    public partial class Form1 : Form
    {
        // Mapa identyfikatorów aplikacji dla ODT
        private static readonly Dictionary<string, string> AppIdMap = new Dictionary<string, string>
        {
            { "\U0001F4AC   Skype for Business", "Lync"     },
            { "\U0001F465   Microsoft Teams",    "Teams"    },
            { "\u2601\uFE0F   OneDrive",         "OneDrive" },
            { "\U0001F4E7   Outlook",            "Outlook"  },
            { "\U0001F4F0   Publisher",          "Publisher"},
            { "\U0001F5C4\uFE0F   Access",       "Access"   },
            { "\U0001F4D3   OneNote",            "OneNote"  },
            { "\U0001F50D   Bing Search",        "Bing"     },
        };

        private static readonly Dictionary<string, string> ProductIdMap = new Dictionary<string, string>
        {
            { "O365ProPlusRetail", "O365ProPlusRetail" },
            { "O365BusinessRetail", "O365BusinessRetail" },
            { "ProPlus2019Retail", "ProPlus2019Retail" },
            { "ProPlus2021Retail", "ProPlus2021Retail" },
            { "ProPlus2024Volume", "ProPlus2024Volume" },
        };

        private string _odtSetupPath = null;
        private string _detectedProductId = "O365BusinessRetail";
        private string _detectedArch = "64";
        private string _detectedChannel = "Current";
        private int _lastOdtExitCode = int.MinValue;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetStatus("⚠ Ready to debloat Microsoft 365.");
            DetectOfficeFromRegistry();
        }

        private void DetectOfficeFromRegistry()
        {
            try
            {
                const string regPath = @"SOFTWARE\Microsoft\Office\ClickToRun\Configuration";
                using (var key = Registry.LocalMachine.OpenSubKey(regPath, false))
                {
                    if (key == null)
                    {
                        SetStatus("⚠ Microsoft 365 not detected.");
                        return;
                    }

                    string releaseIds = key.GetValue("ProductReleaseIds") as string ?? "";
                    string platform = key.GetValue("Platform") as string ?? "x64";
                    string version = key.GetValue("VersionToReport") as string ?? "-";
                    string cdnBaseUrl = key.GetValue("CDNBaseUrl") as string ?? "";
                    string updateChannel = key.GetValue("UpdateChannel") as string ?? "";
                    _detectedArch = platform.Equals("x86", StringComparison.OrdinalIgnoreCase) ? "32" : "64";

                    foreach (var product in ProductIdMap)
                    {
                        if (releaseIds.IndexOf(product.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _detectedProductId = product.Value;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(_detectedProductId) && !string.IsNullOrWhiteSpace(releaseIds))
                        _detectedProductId = releaseIds.Split(',').First().Trim();

                    _detectedChannel = DetectChannelFromCdnUrl(cdnBaseUrl, updateChannel);

                    SetStatus($"✓ Detected: {_detectedProductId} | {_detectedArch}-bit | {_detectedChannel} | v{version}");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Registry error: " + ex.Message);
            }
        }

        private static string DetectChannelFromCdnUrl(string cdnBaseUrl, string updateChannel)
        {
            string channelSource = string.IsNullOrWhiteSpace(cdnBaseUrl) ? updateChannel ?? "" : cdnBaseUrl;

            if (channelSource.IndexOf("492350f6-3a01-4f97-b9c0-c7c6ddf67d60", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Current";
            if (channelSource.IndexOf("55336b82-a18d-4dd6-b5f6-9e5095c314a6", StringComparison.OrdinalIgnoreCase) >= 0)
                return "MonthlyEnterprise";
            if (channelSource.IndexOf("7ffbc6bf-bc32-4f92-8982-f9dd17fd3114", StringComparison.OrdinalIgnoreCase) >= 0)
                return "SemiAnnual";
            if (channelSource.IndexOf("b8f9b850-328d-4355-9145-c59439a0c4cf", StringComparison.OrdinalIgnoreCase) >= 0)
                return "BetaChannel";

            return "Current";
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            await RunDebloaterAsync();
        }

        // TA METODA BYŁA BRAKUJĄCA - NAPRAWIA BŁĄD KOMPILACJI
        private void btnRunAgain_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Windows will restart now to apply changes. Continue?",
                "Restart Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 0",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                SetStatus("✗ Restart failed: " + ex.Message);
            }
        }

        private async Task RunDebloaterAsync()
        {
            var selected = GetSelectedAppIds();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one component to remove.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnStart.Enabled = false;
            pbProgress.Value = 10;

            try
            {
                if (!EnsureOdtExists())
                {
                    SetStatus("✗ ODT not found. Run the PowerShell loader again.");
                    return;
                }

                string xmlPath = GenerateConfigXml(selected);
                SetStatus("⚙ Reconfiguring Office... Do not close.");
                pbProgress.Value = 40;

                bool success = await RunOdtAsync(xmlPath);

                try
                {
                    if (File.Exists(xmlPath))
                        File.Delete(xmlPath);
                }
                catch
                {
                    // Best-effort cleanup only.
                }

                pbProgress.Value = 100;

                if (success)
                {
                    SetStatus("✓ Done! Please restart your PC.");
                    ShowRestartButton();
                }
                else
                {
                    if (_lastOdtExitCode == 17002)
                        SetStatus("⚠ ODT returned 17002. Close all Office apps/services and run again.");
                    else
                        SetStatus($"⚠ ODT failed (exit code: {_lastOdtExitCode}). Check ODT logs in %TEMP%.");
                    ShowRestartButton();
                }
            }
            catch (Exception ex)
            {
                SetStatus("✗ Error: " + ex.Message);
            }
            finally
            {
                btnStart.Enabled = true;
            }
        }

        private string GenerateConfigXml(List<string> excludeIds)
        {
            var doc = new XDocument(
                new XElement("Configuration",
                    new XElement("Add",
                        new XAttribute("OfficeClientEdition", _detectedArch),
                        new XAttribute("Channel", _detectedChannel),
                        new XAttribute("Version", "MatchInstalled"),
                        new XElement("Product",
                            new XAttribute("ID", _detectedProductId),
                            new XElement("Language", new XAttribute("ID", "MatchInstalled")),
                            excludeIds.Select(id => new XElement("ExcludeApp", new XAttribute("ID", id)))
                        )
                    ),
                    new XElement("Property", new XAttribute("Name", "FORCEAPPSHUTDOWN"), new XAttribute("Value", "TRUE")),
                    new XElement("Display", new XAttribute("Level", "None"), new XAttribute("AcceptEULA", "TRUE"))
                )
            );

            string xmlPath = Path.Combine(Path.GetTempPath(), "odt_config.xml");
            doc.Save(xmlPath);
            return xmlPath;
        }

        private async Task<bool> RunOdtAsync(string xmlPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = _odtSetupPath,
                        Arguments = $"/configure \"{xmlPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    var proc = Process.Start(psi);
                    proc.WaitForExit(1200000); // 20 min
                    _lastOdtExitCode = proc.ExitCode;
                    return proc.ExitCode == 0;
                }
                catch
                {
                    _lastOdtExitCode = -1;
                    return false;
                }
            });
        }

        private bool EnsureOdtExists()
        {
            string path = Path.Combine(Path.GetTempPath(), "odt", "setup.exe");
            if (File.Exists(path)) { _odtSetupPath = path; return true; }
            return false;
        }

        private List<string> GetSelectedAppIds()
        {
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in clbApps.CheckedItems)
            {
                if (AppIdMap.TryGetValue(item.ToString(), out string id))
                {
                    ids.Add(id);

                    // Legacy OneDrive for Business sync client can remain unless Groove is also excluded.
                    if (string.Equals(id, "OneDrive", StringComparison.OrdinalIgnoreCase))
                        ids.Add("Groove");
                }
            }
            return ids.ToList();
        }

        private void ShowRestartButton()
        {
            if (btnRunAgain.InvokeRequired)
            {
                btnRunAgain.Invoke(new Action(ShowRestartButton));
                return;
            }
            btnStart.Text = "TASK FINISHED";
            btnStart.Enabled = false;
            btnRunAgain.Visible = true;
            btnRunAgain.Location = new System.Drawing.Point(20, 10);
            btnRunAgain.Width = 380;
        }

        private void SetStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(new Action(() => lblStatus.Text = text));
            else
                lblStatus.Text = text;
        }
    }
}