param([string] $AssemblyDirectory = '.vp-validation/machine-control-build/bin')
$ErrorActionPreference = 'Stop'
$controlRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$controlAssemblies = (Resolve-Path -LiteralPath $AssemblyDirectory).Path
$controlOutput = Join-Path $controlRoot ('.vp-validation/machine-run-control/' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $controlOutput -Force | Out-Null
$controlExecutable = Join-Path $controlOutput 'MachineRunControl.Regression.exe'
$controlArguments = @('/nologo', '/target:exe', '/platform:x64', "/out:$controlExecutable", '/r:System.Data.dll', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Core.dll')
foreach ($controlAssembly in @('AlphaRap.exe', 'AlphaRap.Framework.dll', 'NPSDK.dll')) { $controlArguments += '/r:' + (Join-Path $controlAssemblies $controlAssembly) }
$controlArguments += Join-Path $PSScriptRoot 'Regression.cs'
& 'C:/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe' @controlArguments
if ($LASTEXITCODE -ne 0) { throw 'Machine control regression compilation failed.' }
& $controlExecutable $controlAssemblies $controlOutput
if ($LASTEXITCODE -ne 0) { throw "Machine control regression failed: $controlOutput" }
