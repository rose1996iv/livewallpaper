param(
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$publishScript = Join-Path $PSScriptRoot "Publish-Release.ps1"
$installerScript = Join-Path $root "installer\WallpaperEngine.iss"
$iscc = Get-Command iscc -ErrorAction SilentlyContinue

& $publishScript -Runtime $Runtime -SelfContained

if (-not $iscc) {
    Write-Warning "Inno Setup compiler (iscc) was not found. Publish output is ready, but installer EXE was not built."
    exit 0
}

& $iscc.Source "/DRuntime=$Runtime" $installerScript

Write-Host "Installer build completed."
