param(
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $true
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "src\WallpaperEngine.UI\WallpaperEngine.UI.csproj"
$output = Join-Path $root "dist\publish\$Runtime"

New-Item -ItemType Directory -Force -Path $output | Out-Null

$selfContainedValue = if ($SelfContained) { "true" } else { "false" }

dotnet publish $project `
    -c Release `
    -r $Runtime `
    --self-contained $selfContainedValue `
    -p:PublishSingleFile=false `
    -o $output

Write-Host "Publish completed:"
Write-Host $output
