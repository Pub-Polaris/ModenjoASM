param(
    [switch]$Install
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

dotnet build (Join-Path $repo 'ModenjoASM.csproj') -c Release -v q --nologo
dotnet build (Join-Path $repo 'src\ModenjoASM.Seven\ModenjoASM.Seven.csproj') -c Release -v q --nologo

Write-Host "built host + sample module"
if ($Install) {
    & (Join-Path $PSScriptRoot 'install.ps1')
}
