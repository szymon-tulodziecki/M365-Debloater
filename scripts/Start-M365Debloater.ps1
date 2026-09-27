#Requires -Version 5.1
#Requires -RunAsAdministrator
<#
.SYNOPSIS
Prepares Microsoft's Office Deployment Tool and starts a local M365 Debloater build.
.PARAMETER ApplicationPath
Path to M365Debloater.exe. Defaults to the repository's Release build.
#>
[CmdletBinding()]
param(
    [string]$ApplicationPath = (Join-Path $PSScriptRoot '..\M365-Debloater\bin\Release\M365Debloater.exe')
)

$ErrorActionPreference = 'Stop'
$downloadPage = 'https://www.microsoft.com/en-us/download/details.aspx?id=49117'
$odtDirectory = Join-Path $env:TEMP 'odt'
$downloadDirectory = Join-Path $env:TEMP ('M365Debloater-download-' + [guid]::NewGuid().ToString('N'))

try {
    if (-not (Test-Path -LiteralPath $ApplicationPath -PathType Leaf)) {
        throw "Application not found: $ApplicationPath. Build the Release configuration or provide -ApplicationPath."
    }
    $ApplicationPath = (Resolve-Path -LiteralPath $ApplicationPath).Path
    New-Item -ItemType Directory -Path $downloadDirectory | Out-Null

    Write-Host 'Preparing Office Deployment Tool...'
    $page = Invoke-WebRequest -Uri $downloadPage -UseBasicParsing -TimeoutSec 60
    $download = [regex]::Match($page.Content, 'https://download\.microsoft\.com/[^\s"''<>]*officedeploymenttool[^\s"''<>]*\.exe')
    if (-not $download.Success) {
        throw "The ODT download link was not found. Extract ODT manually from $downloadPage to $odtDirectory."
    }
    $installer = Join-Path $downloadDirectory 'officedeploymenttool.exe'
    Invoke-WebRequest -Uri $download.Value -OutFile $installer -UseBasicParsing -TimeoutSec 600
    $signature = Get-AuthenticodeSignature -LiteralPath $installer
    if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'O=Microsoft Corporation(?:,|$)') {
        throw 'The downloaded ODT installer does not have a valid Microsoft signature.'
    }

    $extracted = Join-Path $downloadDirectory 'extracted'
    New-Item -ItemType Directory -Path $extracted | Out-Null
    $extractProcess = Start-Process -FilePath $installer -ArgumentList "/extract:`"$extracted`" /quiet" -PassThru -Wait
    if ($extractProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $extracted 'setup.exe'))) {
        throw 'ODT extraction failed. No application changes were started.'
    }
    New-Item -ItemType Directory -Path $odtDirectory -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $extracted 'setup.exe') -Destination (Join-Path $odtDirectory 'setup.exe') -Force

    Write-Host 'Starting M365 Debloater. Review and confirm changes in the application.'
    Start-Process -FilePath $ApplicationPath -Wait
}
catch {
    Write-Error $_
    exit 1
}
finally {
    if (Test-Path -LiteralPath $downloadDirectory) {
        Remove-Item -LiteralPath $downloadDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
