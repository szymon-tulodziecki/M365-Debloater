#Requires -RunAsAdministrator
<#
.SYNOPSIS
    M365 Debloater – Warstwa 1: PowerShell Loader
    Użycie (jako administrator):
    irm https://github.com/TWOJ_USER/m365-debloater/releases/latest/download/loader.ps1 | iex
#>

$ErrorActionPreference = "Stop"

# ── Ukryj okno PowerShell (terminal znika, procesy działają w tle) ─────────────
Add-Type -Name Win32 -Namespace "" -MemberDefinition '
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
'
[Win32]::ShowWindow((Get-Process -Id $PID).MainWindowHandle, 0) | Out-Null

# ── Konfiguracja ──────────────────────────────────────────────────────────────
$GithubUser  = "TWOJ_USER"
$GithubRepo  = "m365-debloater"
$ExeName     = "M365Debloater.exe"
$DownloadUrl = "https://github.com/$GithubUser/$GithubRepo/releases/latest/download/$ExeName"
$TempPath    = Join-Path $env:TEMP $ExeName

# ── Krok 1: Pobierz najnowszą wersję z GitHub ─────────────────────────────────
try {
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $TempPath -UseBasicParsing
} catch {
    [System.Windows.Forms.MessageBox]::Show(
        "Nie udalo sie pobrac pliku:`n$_",
        "M365 Debloater - Blad",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error
    )
    exit 1
}

# ── Krok 2: Usuń Mark of the Web (MOTW) ──────────────────────────────────────
try {
    Unblock-File -Path $TempPath
} catch {}

# ── Krok 3: Uruchom aplikację i czekaj na zamknięcie ─────────────────────────
try {
    Start-Process -FilePath $TempPath -Verb RunAs -Wait
} catch {
    [System.Windows.Forms.MessageBox]::Show(
        "Nie udalo sie uruchomic aplikacji:`n$_",
        "M365 Debloater - Blad",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error
    )
    exit 1
}

# ── Sprzątanie: usuń .exe po zamknięciu ───────────────────────────────────────
Start-Sleep -Seconds 2
Remove-Item -Path $TempPath -Force -ErrorAction SilentlyContinue