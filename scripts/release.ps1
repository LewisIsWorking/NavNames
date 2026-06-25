#requires -Version 7
<#
.SYNOPSIS
    Build a local Velopack release of NavNames (Setup.exe + full/delta .nupkg).

.DESCRIPTION
    Mirrors the ClaudeDesktopLauncher release flow but runs locally, matching the
    "CI is local, not GitHub Actions" workflow. Produces artefacts under .\Releases.
    The .github/workflows/release.yml does the same on a tag push if Actions are enabled.

.EXAMPLE
    .\scripts\release.ps1                # version read from NavNames.csproj <Version>
    .\scripts\release.ps1 -Version 0.2.0 # explicit version
    .\scripts\release.ps1 -SkipTests
#>
[CmdletBinding()]
param(
    [string]$Version,
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root       = Split-Path -Parent $PSScriptRoot
$proj       = Join-Path $root 'NavNames\NavNames.csproj'
$testProj   = Join-Path $root 'NavNames.Tests\NavNames.Tests.csproj'
$publishDir = Join-Path $root 'publish'
$releasesDir = Join-Path $root 'Releases'

# Version: explicit arg wins, else read <Version> from the csproj. Strip any 'v'.
if (-not $Version) {
    [xml]$csproj = Get-Content $proj
    $Version = @($csproj.Project.PropertyGroup.Version | Where-Object { $_ })[0]
}
if (-not $Version) { throw 'No -Version supplied and no <Version> found in NavNames.csproj.' }
$Version = $Version -replace '^v', ''
Write-Host "Releasing NavNames $Version" -ForegroundColor Cyan

# Velopack CLI
if (-not (Get-Command vpk -ErrorAction SilentlyContinue)) {
    Write-Host 'Installing Velopack CLI (vpk 1.2.0, matched to the library)...' -ForegroundColor Yellow
    dotnet tool install -g vpk --version 1.2.0
    $env:PATH = "$env:PATH;$HOME\.dotnet\tools"
}

if (-not $SkipTests) {
    Write-Host 'Running tests...' -ForegroundColor Cyan
    dotnet test $testProj --configuration Release --nologo
}

# Multi-file self-contained publish (Velopack needs multi-file for delta updates).
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
Write-Host 'Publishing win-x64 self-contained...' -ForegroundColor Cyan
dotnet publish $proj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDir `
    -p:PublishSingleFile=false

# Pack -> Setup.exe + full/delta .nupkg under .\Releases
Write-Host 'Packing with Velopack...' -ForegroundColor Cyan
vpk pack `
    --packId NavNames `
    --packVersion $Version `
    --packDir $publishDir `
    --mainExe NavNames.exe `
    --packTitle 'NavNames' `
    --outputDir $releasesDir `
    --shortcuts 'Desktop,StartMenu'

Write-Host "Done. Artefacts in $releasesDir" -ForegroundColor Green
if (Test-Path $releasesDir) {
    Get-ChildItem $releasesDir | Select-Object Name, @{N = 'MB'; E = { [math]::Round($_.Length / 1MB, 1) } }
}
