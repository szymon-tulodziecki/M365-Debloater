#Requires -RunAsAdministrator
<#!
.SYNOPSIS
    M365 Debloater – Warstwa 1: PowerShell Loader
    Użycie (jako administrator w PowerShell):
    irm https://raw.githubusercontent.com/TWOJ_USER/m365-debloater/main/loader.ps1 | iex
#>

$ErrorActionPreference = "Stop"

$GithubUser  = "szymon-tulodziecki"       
$GithubRepo  = "M365-Debloater"    
$ExeName     = "M365Debloater.exe"
$DownloadUrl = "https://github.com/$GithubUser/$GithubRepo/releases/latest/download/$ExeName"
$TempPath    = Join-Path $env:TEMP $ExeName
$OdtDir      = Join-Path $env:TEMP "odt"
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

Write-Host "" 
Write-Host "  ╔══════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "  ║        M365 Debloater  v1.0          ║" -ForegroundColor Cyan
Write-Host "  ╚══════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host "" 

Write-Host "[0/3] Przygotowywanie Office Deployment Tool (ODT)..." -ForegroundColor Yellow
try {
    if (Test-Path $OdtDir) {
        Remove-Item -Path $OdtDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path $OdtDownloadDir) {
        Remove-Item -Path $OdtDownloadDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    New-Item -ItemType Directory -Path $OdtDir -Force | Out-Null
    New-Item -ItemType Directory -Path $OdtDownloadDir -Force | Out-Null

    $WinGetPath = Get-WinGetPath
    if (-not $WinGetPath) {
        throw "Nie znaleziono winget.exe. Zainstaluj App Installer i sprobuj ponownie."
    }

    Write-Host "      Pobieranie ODT przez WinGet..." -ForegroundColor Yellow

    # Pobierz instalator ODT przez WinGet do katalogu TEMP
    & $WinGetPath download --id Microsoft.Office.DeploymentTool --location $OdtDownloadDir --accept-source-agreements --accept-package-agreements | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "Polecenie winget zakonczone kodem: $LASTEXITCODE"
    }

    $DownloadedExe = Get-ChildItem -Path $OdtDownloadDir -Filter "officedeploymenttool*.exe" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $DownloadedExe) {
        throw "Nie znaleziono pobranego pliku officedeploymenttool*.exe w %TEMP%."
    }

    Write-Host "      Wypakowywanie ODT do $OdtDir..." -ForegroundColor Yellow
    Start-Process -FilePath $DownloadedExe.FullName -ArgumentList "/extract:`"$OdtDir`" /quiet" -Wait

    if (-not (Test-Path (Join-Path $OdtDir "setup.exe"))) {
        throw "Po wypakowaniu nie znaleziono setup.exe w $OdtDir."
    }

    Write-Host "      OK: ODT przygotowany w $OdtDir" -ForegroundColor Green
}
catch {
    Write-Host "BLAD: Nie udalo sie przygotowac Office Deployment Tool (ODT)." -ForegroundColor Red
    Write-Host "      $_" -ForegroundColor Red
    Write-Host "Upewnij sie, ze WinGet (App Installer) jest zainstalowany i sprobuj ponownie." -ForegroundColor Red
    exit 1
}

Write-Host "[1/3] Pobieranie aplikacji..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $TempPath -UseBasicParsing
    Write-Host "      OK: $TempPath" -ForegroundColor Green
} catch {
    Write-Host "BLAD: Nie udalo sie pobrac pliku." -ForegroundColor Red
    Write-Host "      $_"
    exit 1
}

Write-Host "[2/3] Odblokowywanie pliku..." -ForegroundColor Yellow
try {
    Unblock-File -Path $TempPath
    Write-Host "      OK: MOTW usuniete" -ForegroundColor Green
} catch {
    Write-Host "      OSTRZEZENIE: $_ " -ForegroundColor DarkYellow
}

Write-Host "[3/3] Uruchamianie M365 Debloater..." -ForegroundColor Yellow
try {
    Start-Process -FilePath $TempPath -Verb RunAs -Wait
    Write-Host "      OK: Aplikacja zakonczona" -ForegroundColor Green
} catch {
    Write-Host "BLAD: Nie udalo sie uruchomic aplikacji." -ForegroundColor Red
    exit 1
}

Write-Host "" 
Write-Host "[CLEANUP] Usuwanie plikow tymczasowych..." -ForegroundColor DarkGray
Start-Sleep -Seconds 2
Remove-Item -Path $TempPath -Force -ErrorAction SilentlyContinue
Remove-Item -Path $OdtDownloadDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "          OK" -ForegroundColor DarkGray
Write-Host "" 
Write-Host "  Gotowe." -ForegroundColor Cyan
