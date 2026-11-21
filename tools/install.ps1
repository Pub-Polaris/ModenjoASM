param(
    [string]$GameRoot = 'E:\SteamLibrary\steamapps\common\Aotenjo',
    [string]$ModFolder = '7nnkmj_0.001'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$modsRoot = Join-Path $GameRoot 'Aotenjo_Data\StreamingAssets\mods'
$target = Join-Path $modsRoot $ModFolder
$backupRoot = Join-Path $GameRoot '7nnkmj_backup'
$backup = Join-Path $backupRoot "$ModFolder.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

if (-not (Test-Path $modsRoot)) {
    throw "Mods directory not found: $modsRoot"
}

if (Test-Path $target) {
    New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null
    Copy-Item $target $backup -Recurse
    Write-Host "backup: $backup"
}

New-Item -ItemType Directory -Force -Path $target | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $target 'modules') | Out-Null
Copy-Item (Join-Path $repo 'mod\*') $target -Recurse -Force
Copy-Item (Join-Path $repo 'bin\Release\netstandard2.1\ModenjoASM.dll') $target -Force
Copy-Item (Join-Path $repo 'src\ModenjoASM.Seven\bin\Release\netstandard2.1\ModenjoASM.Seven.dll') (Join-Path $target 'modules') -Force

Write-Host "installed host: $target"
Write-Host "installed module: $(Join-Path $target 'modules\ModenjoASM.Seven.dll')"
Write-Host "no Assembly-CSharp.dll changes"
