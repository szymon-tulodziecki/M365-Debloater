#Requires -RunAsAdministrator
<#
.SYNOPSIS
    M365 Debloater - Warstwa 1: PowerShell Loader
    Uzycie (jako administrator w PowerShell):
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
        Write-Host "      Pobieranie ODT przez WinGet..." -ForegroundColor Yellow
        & $WinGetPath download --id Microsoft.Office.DeploymentTool --location $DestinationDirectory --accept-source-agreements --accept-package-agreements | Out-Null

        if ($LASTEXITCODE -eq 0) {
            $downloaded = Get-ChildItem -Path $DestinationDirectory -Filter "officedeploymenttool*.exe" |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

            if ($downloaded) { return $downloaded.FullName }
        }

        Write-Host "      WinGet nie pobral ODT, proba pobrania bezposrednio z Microsoft Download Center..." -ForegroundColor DarkYellow
    } else {
        Write-Host "      WinGet niedostepny, pobieranie bezposrednio z Microsoft Download Center..." -ForegroundColor Yellow
    }

    $fallbackPath = Join-Path $DestinationDirectory "officedeploymenttool.exe"
    $resolvedUrl = Resolve-OdtDownloadUrl
    Invoke-WebRequest -Uri $resolvedUrl -OutFile $fallbackPath -UseBasicParsing

    if (Test-Path $fallbackPath) { return $fallbackPath }
    throw "Nie udalo sie pobrac instalatora ODT."
}

Write-Host "" 
Write-Host ""
Write-Host "  ======================================" -ForegroundColor Cyan
Write-Host "          M365 Debloater  v1.0          " -ForegroundColor Cyan
Write-Host "  ======================================" -ForegroundColor Cyan

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

    $DownloadedExe = Get-OdtInstaller -DestinationDirectory $OdtDownloadDir

    Write-Host "      Wypakowywanie ODT do $OdtDir..." -ForegroundColor Yellow
    Start-Process -FilePath $DownloadedExe -ArgumentList "/extract:`"$OdtDir`" /quiet" -Wait

    if (-not (Test-Path (Join-Path $OdtDir "setup.exe"))) {
        throw "Po wypakowaniu nie znaleziono setup.exe w $OdtDir."
    }

    Write-Host "      OK: ODT przygotowany w $OdtDir" -ForegroundColor Green
}
catch {
    Write-Host "BLAD: Nie udalo sie przygotowac Office Deployment Tool (ODT)." -ForegroundColor Red
    Write-Host "      $_" -ForegroundColor Red
    Write-Host "Sprawdz polaczenie z internetem i dostep do Microsoft Download Center." -ForegroundColor Red
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
