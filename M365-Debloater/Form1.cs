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

        // Mapowanie CDNBaseUrl → nazwa kanału dla ODT
        private static readonly Dictionary<string, string> ChannelMap = new Dictionary<string, string>
        {
            { "492350f6-3a01-4f97-b9c0-c7c6ddf67d60", "Current"                  },
            { "7ffbc6bf-bc32-4f92-8982-f9dd17fd3114", "SemiAnnual"               },
            { "55336b82-a18d-4dd6-b5f6-9e5095c314a6", "MonthlyEnterprise"        },
            { "64256afe-f5d9-4f86-8936-8840a6a4f5be", "CurrentPreview"           },
            { "b8f9b850-328d-4355-ab41-e373f2a48ea2", "SemiAnnualPreview"        },
        };

        private string _odtSetupPath = null;
        private string _detectedProductId = "O365BusinessRetail";
        private string _detectedArch = "64";
        private string _detectedChannel = "Current";  // odczytany z rejestru

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetStatus("⚠ Operation may take several minutes — do not close any windows.");
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
                    string version = key.GetValue("VersionToReport") as string ?? "—";
                    string cdnUrl = key.GetValue("CDNBaseUrl") as string ?? "";

                    _detectedArch = platform.Equals("x86",
                        StringComparison.OrdinalIgnoreCase) ? "32" : "64";

                    // Wykryj ProductID dynamicznie z rejestru
                    if (!string.IsNullOrWhiteSpace(releaseIds))
                    {
                        _detectedProductId = releaseIds.Split(',')[0].Trim();
                    }

                    // Wykryj kanał z CDNBaseUrl (zawiera GUID kanału)
                    foreach (var kvp in ChannelMap)
                        if (cdnUrl.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                        { _detectedChannel = kvp.Value; break; }

                    SetStatus(
                        $"✓ {_detectedProductId} | {platform} | {_detectedChannel} | v{version}");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Registry error: " + ex.Message);
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            await RunDebloaterAsync();
        }

        private void btnRunAgain_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Windows will restart now. Continue?",
                "Restart required",
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
                    Verb = "runas",
                    CreateNoWindow = true
                });
            }
            catch (Exception ex) { SetStatus("✗ Restart failed: " + ex.Message); }
        }

        private async Task RunDebloaterAsync()
        {
            var selected = GetSelectedAppIds();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one component.",
                    "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnStart.Enabled = false;
            btnRunAgain.Visible = false;
            pbProgress.Value = 0;

            try
            {
                // Krok 1: Upewnij się, że ODT zostało wcześniej pobrane przez loader (WinGet)
                if (EnsureOdtExists())
                {
                    SetStatus("✓ Office Deployment Tool found in cache.");
                    SetProgress(35);
                }
                else
                {
                    SetStatus("✗ Office Deployment Tool (ODT) not found. Run the PowerShell loader again so it can download ODT via WinGet.");
                    return;
                }

                // Krok 2: generuj XML z PRAWDZIWYM kanałem z rejestru
                SetStatus($"📄 Config: {_detectedProductId} | {_detectedArch}-bit | Channel: {_detectedChannel} | {selected.Count} app(s)");
                string xmlPath = GenerateConfigXml(selected);

                // Zapisz XML do Desktop żeby można było sprawdzić
                string debugXml = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "odt_debug.xml");
                File.Copy(xmlPath, debugXml, true);

                SetProgress(50);

                SetStatus("⚙ Reconfiguring Office — please wait…");
                bool success = await RunOdtAsync(xmlPath);
                SetProgress(90);

                CleanupTempFiles(xmlPath);
                SetProgress(100);

                if (success)
                {
                    SetStatus("✓ Done! Please restart your PC.");
                    ShowRestartButton();
                }
                else
                {
                    // Odczytaj log ODT z %TEMP%
                    string logInfo = GetLatestOdtLogPath();
                    SetStatus($"⚠ ODT finished. Check Desktop\\odt_debug.xml and {logInfo}");
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
                        new XAttribute("Channel", _detectedChannel),   // ← kanał z rejestru!
                        new XAttribute("Version", "MatchInstalled"),
                        new XElement("Product",
                            new XAttribute("ID", _detectedProductId),
                            new XElement("Language", new XAttribute("ID", "MatchOS")),
                            excludeIds.Select(id =>
                                new XElement("ExcludeApp", new XAttribute("ID", id)))
                        )
                    ),
                    new XElement("Property",
                        new XAttribute("Name", "FORCEAPPSHUTDOWN"),
                        new XAttribute("Value", "TRUE")),
                    new XElement("Display",
                        new XAttribute("Level", "None"),
                        new XAttribute("AcceptEULA", "TRUE"))
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
                        UseShellExecute = true,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    var proc = Process.Start(psi);
                    proc.WaitForExit(600000);
                    return proc.ExitCode == 0 || proc.ExitCode == 17002;
                }
                catch (Exception ex)
                {
                    SetStatus("✗ ODT launch failed: " + ex.Message);
                    return false;
                }
            });
        }

        private string GetLatestOdtLogPath()
        {
            try
            {
                var log = Directory.GetFiles(Path.GetTempPath(), "*.log")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();
                return log != null ? $"%TEMP%\\{log.Name}" : "%TEMP%\\*.log";
            }
            catch { return "%TEMP%\\*.log"; }
        }

        private bool EnsureOdtExists()
        {
            string path = Path.Combine(Path.GetTempPath(), "odt", "setup.exe");
            if (File.Exists(path)) { _odtSetupPath = path; return true; }
            return false;
        }

        private List<string> GetSelectedAppIds()
        {
            var ids = new List<string>();
            foreach (var item in clbApps.CheckedItems)
                if (AppIdMap.TryGetValue(item.ToString(), out string id))
                    ids.Add(id);
            return ids;
        }

        private void CleanupTempFiles(string xmlPath)
        {
            try { if (File.Exists(xmlPath)) File.Delete(xmlPath); } catch { }
        }

        private void ShowRestartButton()
        {
            if (btnStart.InvokeRequired) { btnStart.Invoke(new Action(ShowRestartButton)); return; }
            btnStart.Size = new System.Drawing.Size(185, 44);
            btnStart.Location = new System.Drawing.Point(20, 10);
            btnStart.Text = "RUN AGAIN";
            btnRunAgain.Location = new System.Drawing.Point(215, 10);
            btnRunAgain.Size = new System.Drawing.Size(185, 44);
            btnRunAgain.Visible = true;
        }

        private void SetStatus(string text)
        {
            if (lblStatus.InvokeRequired)
                lblStatus.Invoke(new Action(() => lblStatus.Text = text));
            else
                lblStatus.Text = text;
        }

        private void SetProgress(int value)
        {
            if (pbProgress.InvokeRequired)
                pbProgress.Invoke(new Action(() => pbProgress.Value = value));
            else
                pbProgress.Value = value;
        }
    }
}