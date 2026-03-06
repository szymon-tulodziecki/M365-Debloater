using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;

namespace M365Debloater
{
    public partial class Form1 : Form
    {
        // ── Mapowanie: etykieta UI → ID aplikacji w ODT ───────────────
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

        // ── Mapowanie: klucz rejestru → ODT ProductID ─────────────────
        private static readonly Dictionary<string, string> ProductIdMap = new Dictionary<string, string>
        {
            { "O365ProPlusRetail",  "O365ProPlusRetail"  },
            { "O365BusinessRetail", "O365BusinessRetail" },
            { "ProPlus2019Retail",  "ProPlus2019Retail"  },
            { "ProPlus2024Volume",  "ProPlus2024Volume"  },
        };

        // ── GitHub: plik z aktualnym URL do ODT ───────────────────────
        // GitHub Actions aktualizuje ten plik co tydzień automatycznie
        private const string OdtUrlFileRaw =
            "https://raw.githubusercontent.com/szymon-tulodziecki/M365-Debloater/main/odt_url.txt";

        private readonly HttpClient _http = new HttpClient();
        private string _odtSetupPath = null;
        private string _detectedProductId = "O365BusinessRetail";
        private string _detectedArch = "64";

        public Form1()
        {
            InitializeComponent();
            _http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            this.Load += Form1_Load;
        }

        // ── Detekcja Office z rejestru przy starcie ────────────────────
        private void Form1_Load(object sender, EventArgs e)
        {
            DetectOfficeFromRegistry();
        }

        private void DetectOfficeFromRegistry()
        {
            try
            {
                const string regPath =
                    @"SOFTWARE\Microsoft\Office\ClickToRun\Configuration";

                using (var key = Registry.LocalMachine.OpenSubKey(regPath, false))
                {
                    if (key == null)
                    {
                        SetStatus("⚠ Nie wykryto pakietu Microsoft 365.");
                        return;
                    }

                    string releaseIds = key.GetValue("ProductReleaseIds") as string ?? "";
                    string platform = key.GetValue("Platform") as string ?? "x64";
                    string version = key.GetValue("VersionToReport") as string ?? "—";

                    _detectedArch = platform.Equals("x86",
                        StringComparison.OrdinalIgnoreCase) ? "32" : "64";

                    foreach (var kvp in ProductIdMap)
                    {
                        if (releaseIds.IndexOf(kvp.Key,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _detectedProductId = kvp.Value;
                            break;
                        }
                    }

                    SetStatus(
                        $"✓ Wykryto: {_detectedProductId}  |  {platform}  |  v{version}");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Błąd rejestru: " + ex.Message);
            }
        }

        // ── Główna akcja przycisku ─────────────────────────────────────
        private async void btnStart_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedAppIds();
            if (selected.Count == 0)
            {
                MessageBox.Show("Zaznacz co najmniej jeden składnik.",
                    "Brak zaznaczenia", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            btnStart.Enabled = false;
            pbProgress.Value = 0;

            try
            {
                if (!EnsureOdtExists())
                {
                    // Krok 1: pobierz aktualny URL z GitHub
                    SetStatus("🔍 Sprawdzanie aktualnej wersji ODT…");
                    SetProgress(5);
                    string odtUrl = await FetchOdtUrlAsync();

                    if (string.IsNullOrWhiteSpace(odtUrl))
                    {
                        SetStatus("✗ Nie udało się pobrać URL z repozytorium.");
                        return;
                    }

                    // Krok 2: pobierz i wypakuj ODT
                    SetStatus($"⬇ Pobieranie ODT…");
                    SetProgress(15);
                    bool ok = await DownloadAndExtractOdtAsync(odtUrl.Trim());
                    if (!ok) return; // SetStatus już ustawiony wewnątrz
                }
                SetProgress(35);

                // Krok 3: generuj XML
                SetStatus("📄 Generowanie konfiguracji XML…");
                string xmlPath = GenerateConfigXml(selected);
                SetProgress(50);

                // Krok 4: uruchom ODT
                SetStatus("⚙ Rekonfiguracja Office — może potrwać kilka minut…");
                bool success = await RunOdtAsync(xmlPath);
                SetProgress(90);

                CleanupTempFiles(xmlPath);
                SetProgress(100);

                SetStatus(success
                    ? "✓ Zakończono. Uruchom ponownie komputer."
                    : "✗ ODT zwróciło błąd — sprawdź logi w %TEMP%.");
            }
            catch (Exception ex)
            {
                SetStatus("✗ Błąd: " + ex.Message);
            }
            finally
            {
                btnStart.Enabled = true;
            }
        }

        // ── Pobierz URL z pliku w repozytorium GitHub ─────────────────
        // GitHub Actions aktualizuje odt_url.txt co tydzień
        private async Task<string> FetchOdtUrlAsync()
        {
            try
            {
                string url = await _http.GetStringAsync(OdtUrlFileRaw);
                return url.Trim();
            }
            catch (Exception ex)
            {
                SetStatus("✗ Błąd pobierania URL: " + ex.Message);
                return null;
            }
        }

        // ── Pobieranie zaznaczonych app ID ────────────────────────────
        private List<string> GetSelectedAppIds()
        {
            var ids = new List<string>();
            foreach (var item in clbApps.CheckedItems)
            {
                if (AppIdMap.TryGetValue(item.ToString(), out string id))
                    ids.Add(id);
            }
            return ids;
        }

        // ── ODT: sprawdź czy setup.exe już istnieje ───────────────────
        private bool EnsureOdtExists()
        {
            string path = Path.Combine(Path.GetTempPath(), "odt", "setup.exe");
            if (File.Exists(path)) { _odtSetupPath = path; return true; }
            return false;
        }

        // ── ODT: pobierz .exe i wypakuj do %TEMP%\odt\ ───────────────
        private async Task<bool> DownloadAndExtractOdtAsync(string url)
        {
            try
            {
                string tempDir = Path.GetTempPath();
                string odtExePath = Path.Combine(tempDir, "officedeploymenttool.exe");
                string odtDir = Path.Combine(tempDir, "odt");
                Directory.CreateDirectory(odtDir);

                using (var response = await _http.GetAsync(url,
                    HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    using (var fs = File.Create(odtExePath))
                        await response.Content.CopyToAsync(fs);
                }

                SetStatus("📦 Rozpakowywanie ODT…");
                SetProgress(28);

                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = odtExePath,
                    Arguments = $"/extract:\"{odtDir}\" /quiet",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc.WaitForExit(60000);

                string setupPath = Path.Combine(odtDir, "setup.exe");
                if (!File.Exists(setupPath))
                {
                    SetStatus("✗ setup.exe nie znaleziono po rozpakowaniu.");
                    return false;
                }

                _odtSetupPath = setupPath;
                return true;
            }
            catch (Exception ex)
            {
                SetStatus("✗ Błąd pobierania ODT: " + ex.Message);
                return false;
            }
        }

        // ── XML: generuj configuration.xml ────────────────────────────
        private string GenerateConfigXml(List<string> excludeIds)
        {
            var excludeElements = excludeIds
                .Select(id => new XElement("ExcludeApp",
                    new XAttribute("ID", id)));

            var doc = new XDocument(
                new XElement("Configuration",
                    new XElement("Add",
                        new XAttribute("OfficeClientEdition", _detectedArch),
                        new XAttribute("Channel", "Current"),
                        new XElement("Product",
                            new XAttribute("ID", _detectedProductId),
                            new XElement("Language",
                                new XAttribute("ID", "MatchOS")),
                            excludeElements
                        )
                    ),
                    new XElement("Property",
                        new XAttribute("Name", "FORCEAPPSHUTDOWN"),
                        new XAttribute("Value", "TRUE")
                    ),
                    new XElement("Display",
                        new XAttribute("Level", "None"),
                        new XAttribute("AcceptEULA", "TRUE")
                    )
                )
            );

            string xmlPath = Path.Combine(Path.GetTempPath(), "odt_config.xml");
            doc.Save(xmlPath);
            return xmlPath;
        }

        // ── ODT: uruchom setup.exe /configure ─────────────────────────
        private async Task<bool> RunOdtAsync(string xmlPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = _odtSetupPath,
                        Arguments = $"/configure \"{xmlPath}\"",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = false
                    });
                    proc.WaitForExit(600000);
                    return proc.ExitCode == 0;
                }
                catch { return false; }
            });
        }

        // ── Sprzątanie ────────────────────────────────────────────────
        private void CleanupTempFiles(string xmlPath)
        {
            try { if (File.Exists(xmlPath)) File.Delete(xmlPath); } catch { }
        }

        // ── Helpers UI (thread-safe) ───────────────────────────────────
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