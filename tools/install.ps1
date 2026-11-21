param(
    [string]$GameRoot = 'E:\SteamLibrary\steamapps\common\Aotenjo',
    [string]$ModFolder = '7nnkmj_0.001'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$modsRoot = Join-Path $GameRoot 'Aotenjo_Data\StreamingAssets\mods'
$target = Join-Path $modsRoot $ModFolder
$backup = "$target.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

if (-not (Test-Path $modsRoot)) {
    throw "Mods directory not found: $modsRoot"
}

if (Test-Path $target) {
    Copy-Item $target $backup -Recurse
    Write-Host "backup: $backup"
}

New-Item -ItemType Directory -Force -Path $target | Out-Null
Copy-Item (Join-Path $repo 'mod\*') $target -Recurse -Force
Copy-Item (Join-Path $repo 'bin\Release\netstandard2.1\ModenjoASM.dll') $target -Force

Write-Host "installed: $target"
Write-Host "no Assembly-CSharp.dll changes"
