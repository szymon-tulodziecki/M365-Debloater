#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms

# ── Ukryj okno PowerShell (stealth mode) ──────────────────────────────────────
Add-Type -Name Win32 -Namespace "" -MemberDefinition '[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);'
[Win32]::ShowWindow((Get-Process -Id $PID).MainWindowHandle, 0) | Out-Null

# ── Konfiguracja ścieżek ──────────────────────────────────────────────────────
$CurrentDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($CurrentDir)) { $CurrentDir = Get-Location }
$ExePath = Join-Path $CurrentDir "M365Debloater.exe"
$OdtDir  = Join-Path $env:TEMP "odt"
$OdtDownloadDir = Join-Path $env:TEMP "M365Debloater\odt-download"

function Get-WinGetPath {
    $cmd = Get-Command winget -ErrorAction SilentlyContinue
    if ($cmd -and $cmd.Source) { return $cmd.Source }

    $candidates = @(
        "$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller*_x64__8wekyb3d8bbwe\winget.exe",
        "$env:ProgramFiles\WindowsApps\Microsoft.DesktopAppInstaller*_x86__8wekyb3d8bbwe\winget.exe",
        "$env:LOCALAPPDATA\Microsoft\WindowsApps\winget.exe"
    )

    foreach ($pattern in $candidates) {
        $match = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
        if ($match) { return $match.FullName }
    }

    return $null
}

# ── Krok 1: Przygotowanie ODT przez WinGet ────────────────────────────────────
try {
    if (Test-Path $OdtDir) { Remove-Item -Path $OdtDir -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path $OdtDownloadDir) { Remove-Item -Path $OdtDownloadDir -Recurse -Force -ErrorAction SilentlyContinue }

    New-Item -ItemType Directory -Path $OdtDir -Force | Out-Null
    New-Item -ItemType Directory -Path $OdtDownloadDir -Force | Out-Null

    $WinGetPath = Get-WinGetPath
    if (-not $WinGetPath) {
        throw "Nie znaleziono winget.exe. Zainstaluj App Installer i uruchom ponownie."
    }
    
    # Pobieranie ODT z oficjalnych serwerów MS przez WinGet
    & $WinGetPath download --id Microsoft.Office.DeploymentTool --location $OdtDownloadDir --accept-source-agreements --accept-package-agreements | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "Polecenie winget zakonczone kodem: $LASTEXITCODE"
    }
    
    $DownloadedExe = Get-ChildItem -Path $OdtDownloadDir -Filter "officedeploymenttool*.exe" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    
    if ($DownloadedExe) {
        # Rozpakowanie ciche do Temp
        Start-Process -FilePath $DownloadedExe.FullName -ArgumentList "/extract:`"$OdtDir`" /quiet" -Wait -WindowStyle Hidden

        if (-not (Test-Path (Join-Path $OdtDir "setup.exe"))) {
            throw "Po wypakowaniu nie znaleziono setup.exe w: $OdtDir"
        }
    } else {
        throw "Nie znaleziono instalatora ODT w katalogu: $OdtDownloadDir"
    }
} catch {
    [System.Windows.Forms.MessageBox]::Show("Błąd przygotowania ODT (WinGet):`n$_", "M365 Debloater", 0, 16)
    exit 1
}

# ── Krok 2: Uruchomienie lokalnej aplikacji EXE ───────────────────────────────
if (Test-Path $ExePath) {
    try {
        Start-Process -FilePath $ExePath -Verb RunAs -Wait
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Błąd uruchamiania M365Debloater.exe:`n$_", "Błąd", 0, 16)
    }
} else {
    [System.Windows.Forms.MessageBox]::Show("Nie znaleziono pliku M365Debloater.exe w folderze ze skryptem!`nSciezka: $ExePath", "Błąd", 0, 16)
}

# ── Czyszczenie ODT z Temp po zamknieciu apki ─────────────────────────────────
Remove-Item -Path $OdtDir -Recurse -Force -ErrorAction SilentlyContinue
if ($DownloadedExe) { Remove-Item -Path $DownloadedExe.FullName -Force -ErrorAction SilentlyContinue }
Remove-Item -Path $OdtDownloadDir -Recurse -Force -ErrorAction SilentlyContinue
exit 0