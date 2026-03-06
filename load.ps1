#Requires -RunAsAdministrator
<#
.SYNOPSIS
    M365 Debloater – Warstwa 1: PowerShell Loader
    Użycie (jako administrator):
    irm https://github.com/TWOJ_USER/m365-debloater/releases/latest/download/loader.ps1 | iex
#>

$ErrorActionPreference = "Stop"

# ── Konfiguracja ──────────────────────────────────────────────────────────────
$GithubUser  = "TWOJ_USER"
$GithubRepo  = "m365-debloater"
$ExeName     = "M365Debloater.exe"
$DownloadUrl = "https://github.com/$GithubUser/$GithubRepo/releases/latest/download/$ExeName"
$TempPath    = Join-Path $env:TEMP $ExeName

# ── Banner ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ╔══════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "  ║        M365 Debloater  v1.0          ║" -ForegroundColor Cyan
Write-Host "  ║   Warstwa 1: PowerShell Loader       ║" -ForegroundColor Cyan
Write-Host "  ╚══════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ── Krok 1: Pobierz najnowszą wersję z GitHub ─────────────────────────────────
Write-Host "[1/3] Pobieranie aplikacji z GitHub..." -ForegroundColor Yellow
try {
    Invoke-WebRequest -Uri $DownloadUrl -OutFile $TempPath -UseBasicParsing
    Write-Host "      OK: $TempPath" -ForegroundColor Green
} catch {
    Write-Host "BLAD: Nie udalo sie pobrac pliku. Sprawdz URL i polaczenie." -ForegroundColor Red
    Write-Host "      $_"
    exit 1
}

# ── Krok 2: Usuń Mark of the Web (MOTW) ──────────────────────────────────────
Write-Host "[2/3] Odblokowywanie pliku (Unblock-File)..." -ForegroundColor Yellow
try {
    Unblock-File -Path $TempPath
    Write-Host "      OK: MOTW usuniete" -ForegroundColor Green
} catch {
    Write-Host "      OSTRZEZENIE: Nie udalo sie odblokowac pliku: $_" -ForegroundColor DarkYellow
}

# ── Krok 3: Uruchom aplikację ─────────────────────────────────────────────────
Write-Host "[3/3] Uruchamianie M365 Debloater..." -ForegroundColor Yellow
try {
    Start-Process -FilePath $TempPath -Verb RunAs -Wait
    Write-Host "      OK: Aplikacja zakonczona" -ForegroundColor Green
} catch {
    Write-Host "BLAD: Nie udalo sie uruchomic aplikacji." -ForegroundColor Red
    Write-Host "      $_"
    exit 1
}

# ── Sprzątanie: usuń .exe po zamknięciu ───────────────────────────────────────
Write-Host ""
Write-Host "[CLEANUP] Usuwanie plikow tymczasowych..." -ForegroundColor DarkGray
Start-Sleep -Seconds 2
try {
    Remove-Item -Path $TempPath -Force -ErrorAction SilentlyContinue
    Write-Host "          OK: $TempPath usunieto" -ForegroundColor DarkGray
} catch {}

Write-Host ""
Write-Host "  Gotowe. System jest czysty." -ForegroundColor Cyan
Write-Host ""