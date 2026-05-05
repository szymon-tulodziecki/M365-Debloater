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
$OdtDownloadPageUrl = "https://www.microsoft.com/en-us/download/details.aspx?id=49117"

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

function Resolve-OdtDownloadUrl {
    $page = Invoke-WebRequest -Uri $OdtDownloadPageUrl -UseBasicParsing
    $link = $page.Links |
        Where-Object { $_.href -match "https://download\.microsoft\.com/.+officedeploymenttool.*\.exe" } |
        Select-Object -First 1

    if ($link -and $link.href) { return $link.href }
    if ($page.Content -match "https://download\.microsoft\.com/[^'""<>]+officedeploymenttool[^'""<>]+\.exe") {
        return $Matches[0]
    }

    throw "Nie znaleziono linku pobierania ODT na stronie Microsoft Download Center."
}

function Get-OdtInstaller {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory
    )

    $WinGetPath = Get-WinGetPath
    if ($WinGetPath) {
        & $WinGetPath download --id Microsoft.Office.DeploymentTool --location $DestinationDirectory --accept-source-agreements --accept-package-agreements | Out-Null

        if ($LASTEXITCODE -eq 0) {
            $downloaded = Get-ChildItem -Path $DestinationDirectory -Filter "officedeploymenttool*.exe" |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

            if ($downloaded) { return $downloaded.FullName }
        }
    }

    $fallbackPath = Join-Path $DestinationDirectory "officedeploymenttool.exe"
    $resolvedUrl = Resolve-OdtDownloadUrl
    Invoke-WebRequest -Uri $resolvedUrl -OutFile $fallbackPath -UseBasicParsing

    if (Test-Path $fallbackPath) { return $fallbackPath }
    throw "Nie udalo sie pobrac instalatora ODT."
}

# ── Krok 1: Przygotowanie ODT przez WinGet ────────────────────────────────────
try {
    if (Test-Path $OdtDir) { Remove-Item -Path $OdtDir -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path $OdtDownloadDir) { Remove-Item -Path $OdtDownloadDir -Recurse -Force -ErrorAction SilentlyContinue }

    New-Item -ItemType Directory -Path $OdtDir -Force | Out-Null
    New-Item -ItemType Directory -Path $OdtDownloadDir -Force | Out-Null

    $DownloadedExe = Get-OdtInstaller -DestinationDirectory $OdtDownloadDir

    # Rozpakowanie ciche do Temp
    Start-Process -FilePath $DownloadedExe -ArgumentList "/extract:`"$OdtDir`" /quiet" -Wait -WindowStyle Hidden

    if (-not (Test-Path (Join-Path $OdtDir "setup.exe"))) {
        throw "Po wypakowaniu nie znaleziono setup.exe w: $OdtDir"
    }
} catch {
    [System.Windows.Forms.MessageBox]::Show("Blad przygotowania ODT:`n$_", "M365 Debloater", 0, 16)
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
if ($DownloadedExe) { Remove-Item -Path $DownloadedExe -Force -ErrorAction SilentlyContinue }
Remove-Item -Path $OdtDownloadDir -Recurse -Force -ErrorAction SilentlyContinue
exit 0
