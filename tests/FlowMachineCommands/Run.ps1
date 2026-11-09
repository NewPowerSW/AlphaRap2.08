param([string]$AssemblyDirectory = '')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (!$AssemblyDirectory) { $AssemblyDirectory = Join-Path $projectRoot 'SDK/NPSDK/bin/x64/Debug' }
$AssemblyDirectory = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$output = Join-Path $PSScriptRoot 'bin'
New-Item -ItemType Directory -Path $output -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $AssemblyDirectory 'NPSDK.dll') -Destination $output -Force
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /target:exe /platform:x64 /langversion:5 "/out:$output/FlowMachineCommands.exe" "/reference:$output/NPSDK.dll" /reference:System.Core.dll /reference:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'Regression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Flow machine command regression compilation failed.' }
& (Join-Path $output 'FlowMachineCommands.exe')
if ($LASTEXITCODE -ne 0) { throw 'Flow machine command regression failed.' }
